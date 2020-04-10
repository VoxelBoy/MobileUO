using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public static class ServerConfigurationModel
{
    private const string serverConfigurationsKey = "server_configurations";
    private const string defaultConfigurationNameKey = "default_server_configuration";

    public static List<ServerConfiguration> ServerConfigurations;
    private static ServerConfiguration activeConfiguration;
    public static ServerConfiguration ActiveConfiguration
    {
        get => activeConfiguration;
        set
        {
            activeConfiguration = value;
            ActiveConfigurationChanged?.Invoke();
        }
    }
    public static ServerConfiguration DefaultConfiguration { get; set; }

    public static Action ActiveConfigurationChanged;

    public static void Initialize()
    {
        var json = PlayerPrefs.GetString(serverConfigurationsKey, string.Empty);
        ServerConfigurations = string.IsNullOrEmpty(json) == false ? JsonConvert.DeserializeObject<List<ServerConfiguration>>(json) : new List<ServerConfiguration>();
        DefaultConfiguration = GetDefaultConfiguration();
    }

    private static ServerConfiguration GetDefaultConfiguration()
    {
        var defaultConfigurationName = PlayerPrefs.GetString(defaultConfigurationNameKey, string.Empty);
        return ServerConfigurations.FirstOrDefault(x => x.Name == defaultConfigurationName);
    }

    public static void AddServerConfiguration(ServerConfiguration newConfiguration)
    {
        if (IsServerConfigurationNameValid(newConfiguration.Name) == false)
        {
            Debug.LogError($"Server configuration name {newConfiguration.Name} is not valid.");
            return;
        }
        ServerConfigurations.Add(newConfiguration);
        newConfiguration.CreateDirectoryToSaveFiles();
        SaveServerConfigurations();
    }

    private static bool IsServerConfigurationNameValid(string name)
    {
        return ServerConfigurations.All(x => x.Name != name);
    }

    public static void SaveServerConfigurations()
    {
        var json = JsonConvert.SerializeObject(ServerConfigurations);
        PlayerPrefs.SetString(serverConfigurationsKey, json);
        PlayerPrefs.Save();
    }

    private static string GetValidNewServerConfigurationName()
    {
        var name = "New Server Configuration";
        var validName = name;
        var index = 1;
        while (IsServerConfigurationNameValid(validName) == false && index < 1000)
        {
            validName = name + " " + index++;
        }

        return validName;
    }

    public static ServerConfiguration CreateNewServerConfiguration()
    {
        var newServerConfiguration = new ServerConfiguration
            {Name = GetValidNewServerConfigurationName()};
        return newServerConfiguration;
    }

    public static bool Contains(ServerConfiguration config)
    {
        return ServerConfigurations.Contains(config);
    }

    public static void DeleteConfiguration(ServerConfiguration config)
    {
        ServerConfigurations.Remove(config);
        DeleteConfigurationFiles(config, false);
    }

    public static void DeleteConfigurationFiles(ServerConfiguration config, bool createEmptyDirectory = true)
    {
        var directoryInfo = new DirectoryInfo(config.GetPathToSaveFiles());
        if (directoryInfo.Exists)
        {
            directoryInfo.Delete(true);
            if (createEmptyDirectory)
            {
                config.CreateDirectoryToSaveFiles();
            }
        }
        config.AllFilesDownloaded = false;
        SaveServerConfigurations();
    }
}