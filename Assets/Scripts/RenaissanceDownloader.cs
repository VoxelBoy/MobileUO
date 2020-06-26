using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

public class RenaissanceDownloader : DownloaderBase
{
    public override void Initialize(DownloadState downloadState, ServerConfiguration serverConfiguration,
        DownloadPresenter downloadPresenter)
    {
        base.Initialize(downloadState, serverConfiguration, downloadPresenter);
        
        var port = int.Parse(serverConfiguration.FileDownloadServerPort);
        
        //Make payload.json request
        var uri = DownloadState.GetUri(serverConfiguration.FileDownloadServerUrl, port, "downloads/launcher/payload.json");
        var request = UnityWebRequest.Get(uri);
        request.SendWebRequest().completed += operation =>
        {
            if (request.isHttpError || request.isNetworkError)
            {
                var error = $"Error while making initial request to server: {request.error}";
                downloadState.StopAndShowError(error);
                return;
            }
            
            var payloadDictionary = JToken.Parse(request.downloadHandler.text);
            var files = payloadDictionary["Files"].Select(x => x["Name"].Value<string>());
            
            //Find the files with the right extension
            var filesToDownload = files.Where(x => downloadState.NeededUoFileExtensions.Any(x.Contains)).ToList();
            
            //Get rid of paths with backslash in them now to prevent downloading files from subdirectories
            filesToDownload.RemoveAll(x => x.Contains("\\"));

            downloadState.SetFileListAndDownload(filesToDownload, "downloads/launcher/client/");
        };
    }
}