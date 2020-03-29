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
}