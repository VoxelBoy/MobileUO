using System;
using System.IO;
using UnityEngine;

[Serializable]
public class ServerConfiguration
{
    public string Name;
    public string UoServerUrl;
    public string UoServerPort = "2593";
    public string FileDownloadServerUrl;
    public string FileDownloadServerPort = "8080";
    public string ClientVersion;
    public string ClientPathForUnityEditor;
    public bool AllFilesDownloaded;

    public string GetPathToSaveFiles()
    {
        return Path.Combine(Application.persistentDataPath, Name);
    }

    public void CreateDirectoryToSaveFiles()
    {
        Directory.CreateDirectory(GetPathToSaveFiles());
    }
}