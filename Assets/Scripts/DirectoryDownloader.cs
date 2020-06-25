using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DirectoryDownloader : DownloaderBase
{
    private List<string> filesToDownload;
    private string resourcePathForFilesToDownload;
    private int concurrentDownloadCounter;
    private int numberOfFilesDownloaded;
    private int numberOfFilesToDownload;
    private Dictionary<string, int> downloadAttemptsPerFile = new Dictionary<string, int>();
    private List<Tuple<UnityWebRequest, string>> activeRequestAndFileNameTupleList = new List<Tuple<UnityWebRequest, string>>();
    private Coroutine downloadCoroutine;
    private string pathToSaveFiles;
    private int port;
    
    private const int MAX_CONCURRENT_DOWNLOADS = 2;
    private const int MAX_DOWNLOAD_ATTEMPTS = 3;

    public override void Initialize(DownloadState downloadState, ServerConfiguration serverConfiguration, DownloadPresenter downloadPresenter)
    {
        base.Initialize(downloadState, serverConfiguration, downloadPresenter);

        pathToSaveFiles = serverConfiguration.GetPathToSaveFiles();
        port = int.Parse(serverConfiguration.FileDownloadServerPort);
        filesToDownload = downloadState.FilesToDownload;
        resourcePathForFilesToDownload = downloadState.ResourcePathForFilesToDownload ?? "";
        numberOfFilesToDownload = filesToDownload.Count;
        downloadPresenter.SetFileList(filesToDownload);
        downloadCoroutine = downloadPresenter.StartCoroutine(DownloadFiles());
    }
    
    private IEnumerator DownloadFiles()
    {
        var directoryInfo = new DirectoryInfo(pathToSaveFiles);
        if (directoryInfo.Exists == false)
        {
            directoryInfo.Create();
        }

        while (filesToDownload.Count > 0)
        {
            while (concurrentDownloadCounter < MAX_CONCURRENT_DOWNLOADS && filesToDownload.Count > 0)
            {
                var fileName = filesToDownload[0];
                filesToDownload.RemoveAt(0);
                if (downloadAttemptsPerFile.TryGetValue(fileName, out _) == false)
                {
                    downloadAttemptsPerFile[fileName] = 1;
                }

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
        var uri = DownloadState.GetUri(serverConfiguration.FileDownloadServerUrl, port, resourcePathForFilesToDownload + fileName);
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
                downloadPresenter.StopCoroutine(downloadCoroutine);
                downloadCoroutine = null;
                downloadState.StopAndShowError(error);
            }
            else
            {
                var attempt = downloadAttemptsPerFile[fileName] + 1;
                downloadAttemptsPerFile[fileName] = attempt;
                Debug.Log($"Re-downloading file, attempt:{attempt}");
                filesToDownload.Insert(0, fileName);
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

    public override void Dispose()
    {
        if (downloadCoroutine != null)
        {
            downloadPresenter.StopCoroutine(downloadCoroutine);
            downloadCoroutine = null;
        }
        filesToDownload = null;
        downloadAttemptsPerFile?.Clear();
        downloadAttemptsPerFile = null;
        activeRequestAndFileNameTupleList?.ForEach(kvp => kvp.Item1?.Dispose());
        activeRequestAndFileNameTupleList?.Clear();
        activeRequestAndFileNameTupleList = null;
        concurrentDownloadCounter = 0;
        numberOfFilesDownloaded = 0;
        numberOfFilesToDownload = 0;
        base.Dispose();
    }
}