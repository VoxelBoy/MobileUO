using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using States;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadState : IState
{
    private readonly DownloadPresenter downloadPresenter;
    private ServerConfiguration serverConfiguration;
    private const int maxConcurrentDownloads = 4;
    private int concurrentDownloadCounter = 0;
    private int numberOfFilesDownloaded = 0;
    private int numberOfFilesToDownload = 0;
    
    public DownloadState(DownloadPresenter downloadPresenter)
    {
        this.downloadPresenter = downloadPresenter;
    }
    
    public void Enter()
    {
        serverConfiguration = ServerConfigurationModel.ActiveConfiguration;
        if (serverConfiguration.AllFilesDownloaded)
        {
            StateManager.GoToState<GameState>();
        }
        else
        {
            //Get list of files to download from server
            var uriBuilder = new UriBuilder("http",serverConfiguration.FileDownloadServerUrl,8080);
            var request = UnityWebRequest.Get(uriBuilder.Uri);
            request.SendWebRequest().completed += operation =>
            {
                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.LogError($"Error while getting list of files from server: {request.error}");
                }
                string hRefPattern = @"href\s*=\s*(?:[""'](?<1>[^""']*)[""']|(?<1>\S+))";
                var filesList = new List<string>(Regex.Matches(request.downloadHandler.text, hRefPattern, RegexOptions.IgnoreCase).
                    Cast<Match>().Select(match => match.Groups[1].Value)
                    .Where(text => text.Contains("."))
                    .Where(text => text.Contains(".exe") == false));
                numberOfFilesToDownload = filesList.Count;
                downloadPresenter.gameObject.SetActive(true);
                downloadPresenter.StartCoroutine(DownloadFiles(filesList));
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
                var uriBuilder = new UriBuilder("http",serverConfiguration.FileDownloadServerUrl,8080, fileName);
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
            Debug.LogError($"Error while downloading {fileName}: {request.error}");
        }
        Debug.Log($"Download finished - {fileName}");
        --concurrentDownloadCounter;
        ++numberOfFilesDownloaded;
        downloadPresenter.UpdateCounter(numberOfFilesDownloaded, numberOfFilesToDownload);
    }
    
    //Taken from Flurl (https://github.com/tmenier/Flurl/)
    private static string CombineUrl(params string[] parts) {
        if (parts == null)
            throw new ArgumentNullException(nameof(parts));

        string result = "";
        bool inQuery = false, inFragment = false;

        string CombineEnsureSingleSeparator(string a, string b, char separator) {
            if (string.IsNullOrEmpty(a)) return b;
            if (string.IsNullOrEmpty(b)) return a;
            return a.TrimEnd(separator) + separator + b.TrimStart(separator);
        }

        foreach (var part in parts) {
            if (string.IsNullOrEmpty(part))
                continue;

            if (result.EndsWith("?") || part.StartsWith("?"))
                result = CombineEnsureSingleSeparator(result, part, '?');
            else if (result.EndsWith("#") || part.StartsWith("#"))
                result = CombineEnsureSingleSeparator(result, part, '#');
            else if (inFragment)
                result += part;
            else if (inQuery)
                result = CombineEnsureSingleSeparator(result, part, '&');
            else
                result = CombineEnsureSingleSeparator(result, part, '/');

            if (part.Contains("#")) {
                inQuery = false;
                inFragment = true;
            }
            else if (!inFragment && part.Contains("?")) {
                inQuery = true;
            }
        }
        return EncodeIllegalCharacters(result);
    }
    
    //Taken from Flurl (https://github.com/tmenier/Flurl/)
    private static string EncodeIllegalCharacters(string s, bool encodeSpaceAsPlus = false) {
        if (string.IsNullOrEmpty(s))
            return s;

        if (encodeSpaceAsPlus)
            s = s.Replace(" ", "+");

        // Uri.EscapeUriString mostly does what we want - encodes illegal characters only - but it has a quirk
        // in that % isn't illegal if it's the start of a %-encoded sequence https://stackoverflow.com/a/47636037/62600

        // no % characters, so avoid the regex overhead
        if (!s.Contains("%"))
            return Uri.EscapeUriString(s);

        // pick out all %-hex-hex matches and avoid double-encoding 
        return Regex.Replace(s, "(.*?)((%[0-9A-Fa-f]{2})|$)", c => {
            var a = c.Groups[1].Value; // group 1 is a sequence with no %-encoding - encode illegal characters
            var b = c.Groups[2].Value; // group 2 is a valid 3-character %-encoded sequence - leave it alone!
            return Uri.EscapeUriString(a) + b;
        });
    }

    public void Exit()
    {
        downloadPresenter.gameObject.SetActive(false);
    }
}