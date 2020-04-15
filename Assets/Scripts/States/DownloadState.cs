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
    private int port;
    private const int maxConcurrentDownloads = 4;
    private int concurrentDownloadCounter = 0;
    private int numberOfFilesDownloaded = 0;
    private int numberOfFilesToDownload = 0;

    private Coroutine downloadCoroutine;
    
    public DownloadState(DownloadPresenter downloadPresenter)
    {
        this.downloadPresenter = downloadPresenter;
        downloadPresenter.backButtonPressed += OnBackButtonPressed;
    }

    private void OnBackButtonPressed()
    {
        StateManager.GoToState<ServerConfigurationState>();
    }

    public void Enter()
    {
        serverConfiguration = ServerConfigurationModel.ActiveConfiguration;
        port = int.Parse(serverConfiguration.FileDownloadServerPort);
        if (serverConfiguration.AllFilesDownloaded || Application.isEditor)
        {
            StateManager.GoToState<GameState>();
        }
        else
        {
            downloadPresenter.gameObject.SetActive(true);
            //Get list of files to download from server
            var uriBuilder = new UriBuilder("http",serverConfiguration.FileDownloadServerUrl, port);
            var request = UnityWebRequest.Get(uriBuilder.Uri);
            request.SendWebRequest().completed += operation =>
            {
                if (request.isHttpError || request.isNetworkError)
                {
                    var error = $"Error while getting list of files from server: {request.error}";
                    Debug.LogError(error);
                    downloadPresenter.ShowError(error);
                    return;
                }
                string hRefPattern = @"href\s*=\s*(?:[""'](?<1>[^""']*)[""']|(?<1>\S+))";
                var filesList = new List<string>(Regex.Matches(request.downloadHandler.text, hRefPattern, RegexOptions.IgnoreCase).
                    Cast<Match>().Select(match => match.Groups[1].Value)
                    .Where(text => text.Contains("."))
                    .Where(text => text.Contains(".exe") == false));
                numberOfFilesToDownload = filesList.Count;
                downloadCoroutine = downloadPresenter.StartCoroutine(DownloadFiles(filesList));
            };
        }
    }

    private IEnumerator DownloadFiles(List<string> filesList)
    {
        int index = 0;

        var pathToSaveFiles = serverConfiguration.GetPathToSaveFiles();
        var directoryInfo = new DirectoryInfo(pathToSaveFiles);
        if (directoryInfo.Exists == false)
        {
            directoryInfo.Create();
        }

        while (index < filesList.Count)
        {
            while (concurrentDownloadCounter < maxConcurrentDownloads && index < filesList.Count)
            {
                var fileName = filesList[index++];
                var uriBuilder = new UriBuilder("http",serverConfiguration.FileDownloadServerUrl,port, fileName);
                var request = UnityWebRequest.Get(uriBuilder.Uri);
                var filePath = Path.Combine(pathToSaveFiles, fileName);
                var fileDownloadHandler = new DownloadHandlerFile(filePath);
                fileDownloadHandler.removeFileOnAbort = true;
                request.downloadHandler = fileDownloadHandler;
                request.SendWebRequest().completed += operation => SingleFileDownloadFinished(request, fileName);
                ++concurrentDownloadCounter;
            }

            yield return new WaitUntil(() => concurrentDownloadCounter < maxConcurrentDownloads);
        }

        //Wait until final downloads finish
        yield return new WaitUntil(() => concurrentDownloadCounter == 0);

        serverConfiguration.AllFilesDownloaded = true;
        ServerConfigurationModel.SaveServerConfigurations();
        
        StateManager.GoToState<GameState>();
    }

    private void SingleFileDownloadFinished(UnityWebRequest request, string fileName)
    {
        if (request.isHttpError || request.isNetworkError)
        {
            var error = $"Error while downloading {fileName}: {request.error}";
            Debug.LogError(error);
            downloadPresenter.ShowError(error);
            //Stop downloads
            downloadPresenter.StopCoroutine(downloadCoroutine);
            return;
        }
        Debug.Log($"Download finished - {fileName}");
        --concurrentDownloadCounter;
        ++numberOfFilesDownloaded;
        downloadPresenter.UpdateCounter(numberOfFilesDownloaded, numberOfFilesToDownload);
    }

    public void Exit()
    {
        downloadPresenter.gameObject.SetActive(false);
    }
}