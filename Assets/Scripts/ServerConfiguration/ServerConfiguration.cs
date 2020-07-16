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
    public string FileDownloadServerPort = DownloadState.DefaultFileDownloadPort;
    public string ClientVersion;
    public bool UseEncryption;
    public string ClientPathForUnityEditor;
    public bool AllFilesDownloaded;
    public bool PreferExternalStorage;

    public string GetPathToSaveFiles()
    {
        var dataPath = Application.persistentDataPath;
        
        if (PreferExternalStorage && string.IsNullOrEmpty(Init.ExternalStoragePath) == false)
        {
            dataPath = Init.ExternalStoragePath;
        }
        
        return Path.Combine(dataPath, Name);
    }

    public void CreateDirectoryToSaveFiles()
    {
        Directory.CreateDirectory(GetPathToSaveFiles());
    }

    public ServerConfiguration Clone()
    {
        return new ServerConfiguration
        {
            Name = this.Name,
            UoServerUrl = this.UoServerUrl,
            UoServerPort = this.UoServerPort,
            FileDownloadServerUrl = this.FileDownloadServerUrl,
            FileDownloadServerPort = this.FileDownloadServerPort,
            ClientVersion = this.ClientVersion,
            UseEncryption = this.UseEncryption,
            ClientPathForUnityEditor = this.ClientPathForUnityEditor,
            AllFilesDownloaded = this.AllFilesDownloaded,
            PreferExternalStorage = this.PreferExternalStorage
        };
    }
}