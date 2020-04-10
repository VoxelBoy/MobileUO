using System;
using System.IO;
using UnityEngine;

[Serializable]
public class ServerConfiguration
{
    public string Name;
    public string UoServerUrl;
    public string FileDownloadServerUrl;
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

    public bool SaveDirectoryContainsFiles()
    {
        var directoryInfo = new DirectoryInfo(GetPathToSaveFiles());
        return directoryInfo.Exists && directoryInfo.GetFiles().Length > 0;
    }
}