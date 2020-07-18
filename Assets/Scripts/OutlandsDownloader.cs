using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class OutlandsDownloader : DownloaderBase
{
    private int port;
    private const string OUTLANDS_VERSION_PREF_KEY = "OUTLANDS_VERSION";
    
    public override void Initialize(DownloadState downloadState, ServerConfiguration serverConfiguration,
        DownloadPresenter downloadPresenter)
    {
        base.Initialize(downloadState, serverConfiguration, downloadPresenter);
        
        port = int.Parse(serverConfiguration.FileDownloadServerPort);
        
        //Make version request
        var uri = DownloadState.GetUri(serverConfiguration.FileDownloadServerUrl, port, "Version");
        var request = UnityWebRequest.Get(uri);
        request.SendWebRequest().completed += operation =>
        {
            if (request.isHttpError || request.isNetworkError)
            {
                var error = $"Error while making initial request to server: {request.error}";
                downloadState.StopAndShowError(error);
                return;
            }

            var outlandsVersion = request.downloadHandler.text;
            Debug.Log($"Request result text (outlands version): {outlandsVersion}");
            
            var previousOutlandsVersion = PlayerPrefs.GetString(OUTLANDS_VERSION_PREF_KEY, string.Empty);
            if (string.IsNullOrEmpty(previousOutlandsVersion))
            {
                //First time reaching to outlands servers
                PlayerPrefs.SetString(OUTLANDS_VERSION_PREF_KEY, outlandsVersion);
            }
            else if(previousOutlandsVersion == outlandsVersion)
            {
                //Same as before
            }
            else
            {
                //New version
            }

            GetManifest();
        };
    }

    private void GetManifest()
    {
        //Make version request
        var uri = DownloadState.GetUri(serverConfiguration.FileDownloadServerUrl, port, "Manifest");
        var request = UnityWebRequest.Get(uri);
        request.SendWebRequest().completed += operation =>
        {
            if (request.isHttpError || request.isNetworkError)
            {
                var error = $"Error while making initial request to server: {request.error}";
                downloadState.StopAndShowError(error);
                return;
            }

            var manifestContents = request.downloadHandler.text;
            Debug.Log($"Request result text (manifest contents): {manifestContents}");
            
            var lines = manifestContents.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            
            //All valid lines start with one backslash
            lines.RemoveAll(x => x.StartsWith("\\") == false);
            //I think we can ignore the stuff that begins with a backslash-minus, which probably means they should be deleted?
            lines.RemoveAll(x => x.StartsWith("\\-"));

            //For files with the right extension, remove first character (backslash)
            var filesToDownload = lines.Where(x => DownloadState.NeededUoFileExtensions.Any(x.Contains)).Select(y => y.Substring(1)).ToList();
            //For files with a plus at the beginning, remove first character
            filesToDownload = filesToDownload.Select(x => x.StartsWith("+") ? x.Substring(1) : x).ToList();
            
            //Get rid of paths with backslash in them now to prevent downloading files from subdirectories
            filesToDownload.RemoveAll(x => x.Contains("\\"));

            downloadState.SetFileListAndDownload(filesToDownload);
        };
    }
}