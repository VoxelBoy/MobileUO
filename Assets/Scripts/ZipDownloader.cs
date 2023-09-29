using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Networking;

public class ZipDownloader : DownloaderBase
{
    private string pathToSaveFiles;
    private int port;
    private string url;
    private string fileName;
    private Coroutine downloadCoroutine;
    private UnityWebRequest webRequest;
    private int downloadAttempts;
    private const int MAX_DOWNLOAD_ATTEMPTS = 3;
    
    public override void Initialize(DownloadState downloadState, ServerConfiguration serverConfiguration, DownloadPresenter downloadPresenter)
    {
        base.Initialize(downloadState, serverConfiguration, downloadPresenter);

        pathToSaveFiles = serverConfiguration.GetPathToSaveFiles();
        port = int.Parse(serverConfiguration.FileDownloadServerPort);
        url = serverConfiguration.FileDownloadServerUrl;
        fileName = GetFileNameFromUrl(url);
        downloadPresenter.SetFileList(new List<string> {fileName});
        downloadCoroutine = downloadPresenter.StartCoroutine(DownloadFiles());
    }
    
    private IEnumerator DownloadFiles()
    {
        var directoryInfo = new DirectoryInfo(pathToSaveFiles);
        if (directoryInfo.Exists == false)
        {
            directoryInfo.Create();
        }
        
        downloadPresenter.UpdateView(0,1);
        
        DownloadFile();

        while (webRequest.isDone == false)
        {
            downloadPresenter.SetDownloadProgress(fileName, webRequest.downloadProgress);
            yield return null;
        }
        
        var filePath = Path.Combine(pathToSaveFiles, fileName);
        try
        {
            ZipFile.ExtractToDirectory(filePath, pathToSaveFiles, true);
        }
        catch (Exception e)
        {
            var error = $"Error while extracting {fileName}: {e}";
            downloadState.StopAndShowError(error);
            yield break;
        }
        finally
        {
            downloadCoroutine = null;
            File.Delete(filePath);
        }

        serverConfiguration.AllFilesDownloaded = true;
        ServerConfigurationModel.SaveServerConfigurations();
        
        StateManager.GoToState<GameState>();
    }
    
    private void DownloadFile()
    {
        var uri = DownloadState.GetUri(url, port);
        var uriString = uri.ToString();
        if (uriString.EndsWith("/"))
        {
            uriString = uriString.Substring(0, uriString.Length - 1);
        }
        webRequest = UnityWebRequest.Get(uriString);
        var filePath = Path.Combine(pathToSaveFiles, fileName);
        var fileDownloadHandler = new DownloadHandlerFile(filePath) {removeFileOnAbort = true};
        webRequest.downloadHandler = fileDownloadHandler;
        webRequest.SendWebRequest().completed += _ => DownloadFinished(webRequest, fileName);
    }

    private static string GetFileNameFromUrl(string url)
    {
        if (url.Contains("http://") == false)
        {
            url = "http://" + url;
        }
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            uri = new Uri(url);
        var fileName = Path.GetFileName(uri.LocalPath);
        return fileName;
    }

    private void DownloadFinished(UnityWebRequest request, string fileName)
    {
        //If download coroutine was stopped, do nothing
        if (downloadCoroutine == null)
        {
            return;
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            downloadPresenter.SetFileDownloaded(fileName);
            downloadPresenter.UpdateView(1, 1);
        }
        else
        {
            if(downloadAttempts >= MAX_DOWNLOAD_ATTEMPTS)
            {
                var error = $"Error while downloading {fileName}: {request.error}";
                downloadPresenter.StopCoroutine(downloadCoroutine);
                downloadCoroutine = null;
                downloadState.StopAndShowError(error);
            }
            else
            {
                downloadAttempts++;
                Debug.Log($"Re-downloading file, attempt:{downloadAttempts}");
                DownloadFile();
                downloadPresenter.SetDownloadProgress(request.uri.AbsolutePath, 0f);
            }
        }
    }
    
    public override void Dispose()
    {
        if (downloadCoroutine != null)
        {
            downloadPresenter.StopCoroutine(downloadCoroutine);
            downloadCoroutine = null;
        }
        webRequest?.Abort();
        webRequest?.Dispose();
        base.Dispose();
    }
}