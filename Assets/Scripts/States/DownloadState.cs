using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadState : IState
{
    private readonly DownloadPresenter downloadPresenter;
    private readonly Canvas inGameDebugConsoleCanvas;
    
    private ServerConfiguration serverConfiguration;

    public List<string> filesToDownload;
    private DownloaderBase downloader;

    private readonly bool forceDownloadsInEditor;
    
    private const string H_REF_PATTERN = @"<a\shref=[^>]*>([^<]*)<\/a>";
    
    public readonly List<string> NeededUoFileExtensions = new List<string>{".def", ".mul", ".idx", ".uop", ".enu"};

    public const string DefaultFileDownloadPort = "8080";

    public DownloadState(DownloadPresenter downloadPresenter, bool forceDownloadsInEditor, Canvas inGameDebugConsoleCanvas)
    {
        this.downloadPresenter = downloadPresenter;
        this.inGameDebugConsoleCanvas = inGameDebugConsoleCanvas;
        
        downloadPresenter.backButtonPressed += OnBackButtonPressed;
        this.forceDownloadsInEditor = forceDownloadsInEditor;
    }

    private void OnBackButtonPressed()
    {
        inGameDebugConsoleCanvas.enabled = false;
        StateManager.GoToState<ServerConfigurationState>();
    }

    public void Enter()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        serverConfiguration = ServerConfigurationModel.ActiveConfiguration;
        Debug.Log($"Downloading files to {serverConfiguration.GetPathToSaveFiles()}");
        var port = int.Parse(serverConfiguration.FileDownloadServerPort);
        
        if (serverConfiguration.AllFilesDownloaded || (Application.isEditor && forceDownloadsInEditor == false))
        {
            StateManager.GoToState<GameState>();
        }
        else
        {
            downloadPresenter.gameObject.SetActive(true);

            //Figure out what kind of downloader we should use
            if (serverConfiguration.FileDownloadServerUrl.ToLowerInvariant().Contains("uooutlands.com"))
            {
                downloader = new OutlandsDownloader();
                downloader.Initialize(this, serverConfiguration, downloadPresenter);
            }
            else
            {
                //Get list of files to download from server
                var uri = GetUri(serverConfiguration.FileDownloadServerUrl, port);
                var request = UnityWebRequest.Get(uri);
                request.SendWebRequest().completed += operation =>
                {
                    if (request.isHttpError || request.isNetworkError)
                    {
                        var error = $"Error while making initial request to server: {request.error}";
                        StopAndShowError(error);
                        return;
                    }

                    var headers = request.GetResponseHeaders();

                    if (headers.TryGetValue("Content-Type", out var contentType))
                    {
                        if (contentType.Contains("application/json"))
                        {
                            //Parse json response to get list of files
                            Debug.Log($"Json response: {request.downloadHandler.text}");
                            filesToDownload = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(request.downloadHandler.text);
                        }
                        else if (contentType.Contains("text/html"))
                        {
                            filesToDownload = new List<string>(Regex
                                .Matches(request.downloadHandler.text, H_REF_PATTERN, RegexOptions.IgnoreCase)
                                .Cast<Match>()
                                .Select(match => match.Groups[1].Value));
                        }
                    }

                    if (filesToDownload != null)
                    {
                        filesToDownload.RemoveAll(file => NeededUoFileExtensions.Any(file.Contains) == false);
                        SetFileListAndDownload(filesToDownload);
                    }
                    else
                    {
                        StopAndShowError("Could not determine file list to download");
                    }
                };
            }
        }
    }

    public void SetFileListAndDownload(List<string> filesList)
    {
        filesToDownload = filesList;

        var hasAnimationFiles = filesToDownload.Any(x =>
        {
            var fileNameLowerCase = x.ToLowerInvariant();
            return fileNameLowerCase.Contains("anim.mul") || fileNameLowerCase.Contains("animationframe1.uop");
        });
                    
        if (filesToDownload.Count == 0 || hasAnimationFiles == false)
        {
            var error = "Download directory does not contain UO files such as anim.mul or AnimationFrame1.uop";
            StopAndShowError(error);
            return;
        }

        if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        {
            ShowCellularWarning();
        }
        else
        {
            downloader = new DirectoryDownloader();
            downloader.Initialize(this, serverConfiguration, downloadPresenter);
        }
    }

    private void ShowCellularWarning()
    {
        //TODO: Implement a dialog with Back and Continue buttons
        var userAllowedDownloads = true;
        if (userAllowedDownloads)
        {
            downloader = new DirectoryDownloader();
            downloader.Initialize(this, serverConfiguration, downloadPresenter);
        }
    }

    public static Uri GetUri(string serverUrl, int port, string fileName = null)
    {
        var httpPort = port == 80;
        var httpsPort = port == 443;
        var defaultPort = httpPort || httpsPort;
        var scheme = httpsPort ? "https" : "http";
        var uriBuilder = new UriBuilder(scheme, serverUrl, defaultPort ? - 1 : port, fileName);
        return uriBuilder.Uri;
    }

    public void StopAndShowError(string error)
    {
        Debug.LogError(error);
        //Stop downloads
        downloadPresenter.ShowError(error);
        downloadPresenter.ClearFileList();
        inGameDebugConsoleCanvas.enabled = true;
    }

    public void Exit()
    {
        Screen.sleepTimeout = SleepTimeout.SystemSetting;
        
        downloadPresenter.ClearFileList();
        downloadPresenter.gameObject.SetActive(false);
        downloader?.Dispose();
        
        filesToDownload = null;
        serverConfiguration = null;
    }
}