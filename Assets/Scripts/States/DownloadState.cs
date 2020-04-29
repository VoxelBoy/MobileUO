using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadState : IState
{
    private readonly DownloadPresenter downloadPresenter;
    
    private ServerConfiguration serverConfiguration;
    private string pathToSaveFiles;
    private int port;
    
    private int concurrentDownloadCounter;
    private int numberOfFilesDownloaded;
    private int numberOfFilesToDownload;
    private readonly Dictionary<string, int> downloadAttemptsPerFile = new Dictionary<string, int>();
    private List<string> downloadingFilesList;
    private readonly List<Tuple<UnityWebRequest, string>> activeRequestAndFileNameTupleList = new List<Tuple<UnityWebRequest, string>>();
    private Coroutine downloadCoroutine;

    private readonly bool forceDownloadsInEditor;
    
    private const string H_REF_PATTERN = @"<a\shref=[^>]*>([^<]*)<\/a>";
    private const int MAX_CONCURRENT_DOWNLOADS = 1;
    private const int MAX_DOWNLOAD_ATTEMPTS = 3;

    public DownloadState(DownloadPresenter downloadPresenter, bool forceDownloadsInEditor)
    {
        this.downloadPresenter = downloadPresenter;
        downloadPresenter.backButtonPressed += OnBackButtonPressed;
        this.forceDownloadsInEditor = forceDownloadsInEditor;
    }

    private void OnBackButtonPressed()
    {
        StateManager.GoToState<ServerConfigurationState>();
    }

    public void Enter()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        serverConfiguration = ServerConfigurationModel.ActiveConfiguration;
        pathToSaveFiles = serverConfiguration.GetPathToSaveFiles();
        Debug.Log($"Downloading files to {pathToSaveFiles}");
        port = int.Parse(serverConfiguration.FileDownloadServerPort);
        
        if (serverConfiguration.AllFilesDownloaded || (Application.isEditor && forceDownloadsInEditor == false))
        {
            StateManager.GoToState<GameState>();
        }
        else
        {
            downloadPresenter.gameObject.SetActive(true);
            //Get list of files to download from server
            var uri = GetUri(serverConfiguration.FileDownloadServerUrl, port);
            var request = UnityWebRequest.Get(uri);
            request.SendWebRequest().completed += operation =>
            {
                if (request.isHttpError || request.isNetworkError)
                {
                    var error = $"Error while getting list of files from server: {request.error}";
                    Debug.LogError(error);
                    downloadPresenter.ShowError(error);
                    return;
                }

                downloadingFilesList = new List<string>(Regex
                    .Matches(request.downloadHandler.text, H_REF_PATTERN, RegexOptions.IgnoreCase).Cast<Match>()
                    .Select(match => match.Groups[1].Value)
                    .Where(text => text.Contains(".") &&
                                   text.Contains(".exe") == false &&
                                   text.Contains(".app") == false &&
                                   text.Contains(".dll") == false &&
                                   text.Contains(".pdb") == false &&
                                   text.Contains("/") == false));
                numberOfFilesToDownload = downloadingFilesList.Count;
                downloadPresenter.SetFileList(downloadingFilesList);
                downloadCoroutine = downloadPresenter.StartCoroutine(DownloadFiles());
            };
        }
    }

    private static Uri GetUri(string serverUrl, int port, string fileName = null)
    {
        var httpPort = port == 80;
        var httpsPort = port == 443;
        var defaultPort = httpPort || httpsPort;
        var scheme = httpsPort ? "https" : "http";
        var uriBuilder = new UriBuilder(scheme, serverUrl, defaultPort ? - 1 : port, fileName);
        return uriBuilder.Uri;
    }

    private IEnumerator DownloadFiles()
    {
        int index = 0;
        
        var directoryInfo = new DirectoryInfo(pathToSaveFiles);
        if (directoryInfo.Exists == false)
        {
            directoryInfo.Create();
        }

        while (downloadingFilesList.Count > 0)
        {
            while (concurrentDownloadCounter < MAX_CONCURRENT_DOWNLOADS && downloadingFilesList.Count > 0)
            {
                var fileName = downloadingFilesList[0];
                downloadingFilesList.RemoveAt(0);
                downloadAttemptsPerFile[fileName] = 1;
                DownloadFile(fileName);
            }

            UpdateDownloadProgress();

            yield return null;
        }

        //Wait until final downloads finish
        while (concurrentDownloadCounter > 0)
        {
            UpdateDownloadProgress();
            yield return null;
        }

        serverConfiguration.AllFilesDownloaded = true;
        ServerConfigurationModel.SaveServerConfigurations();
        
        StateManager.GoToState<GameState>();
    }

    private void UpdateDownloadProgress()
    {
        foreach (var tuple in activeRequestAndFileNameTupleList)
        {
            downloadPresenter.SetDownloadProgress(tuple.Item2, tuple.Item1.downloadProgress);
        }
    }

    private void DownloadFile(string fileName)
    {
        var uri = GetUri(serverConfiguration.FileDownloadServerUrl, port, fileName);
        var request = UnityWebRequest.Get(uri);
        var filePath = Path.Combine(pathToSaveFiles, fileName);
        var fileDownloadHandler = new DownloadHandlerFile(filePath) {removeFileOnAbort = true};
        request.downloadHandler = fileDownloadHandler;
        request.SendWebRequest().completed += operation => SingleFileDownloadFinished(request, fileName);
        activeRequestAndFileNameTupleList.Add(new Tuple<UnityWebRequest, string>(request, fileName));
        ++concurrentDownloadCounter;
    }

    private void SingleFileDownloadFinished(UnityWebRequest request, string fileName)
    {
        --concurrentDownloadCounter;
        activeRequestAndFileNameTupleList.RemoveAll(x => x.Item1 == request);
        if (request.isHttpError || request.isNetworkError)
        {
            if(downloadAttemptsPerFile[fileName] >= MAX_DOWNLOAD_ATTEMPTS)
            {
                var error = $"Error while downloading {fileName}: {request.error}";
                StopAndShowError(error);
            }
            else
            {
                var attempt = downloadAttemptsPerFile[fileName] + 1;
                downloadAttemptsPerFile[fileName] = attempt;
                Debug.Log($"Re-downloading file, attempt:{attempt}");
                downloadingFilesList.Insert(0, fileName);
                downloadPresenter.SetDownloadProgress(request.uri.AbsolutePath, 0f);
            }
        }
        else
        {
            Debug.Log($"Download finished - {fileName}");
            ++numberOfFilesDownloaded;
            downloadPresenter.SetFileDownloaded(fileName);
            downloadPresenter.UpdateView(numberOfFilesDownloaded, numberOfFilesToDownload);
        }
        
    }

    private void StopAndShowError(string error)
    {
        Debug.LogError(error);
        //Stop downloads
        downloadPresenter.StopCoroutine(downloadCoroutine);
        downloadPresenter.ShowError(error);
        downloadPresenter.ClearFileList();
    }

    public void Exit()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        
        if (downloadCoroutine != null)
        {
            downloadPresenter.StopCoroutine(downloadCoroutine);
            downloadCoroutine = null;
        }
        downloadPresenter.ClearFileList();
        downloadPresenter.gameObject.SetActive(false);
        
        downloadAttemptsPerFile.Clear();
        downloadingFilesList = null;
        activeRequestAndFileNameTupleList.Clear();
        serverConfiguration = null;

        concurrentDownloadCounter = 0;
        numberOfFilesDownloaded = 0;
        numberOfFilesToDownload = 0;
    }
}