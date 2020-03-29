using System.IO;
using UnityEngine;

public class ServerConfigurationPresenter : MonoBehaviour
{
    [SerializeField] private ServerConfigurationListPresenter serverConfigurationListPresenter;
    [SerializeField] private ServerConfigurationEditPresenter serverConfigurationEditPresenter;

    private void Start()
    {
        SwitchToList();

        serverConfigurationListPresenter.AddNewConfigurationButtonClicked += OnAddNewConfigurationButtonClicked;
        serverConfigurationListPresenter.EditButtonClicked += OnEditButtonClicked;
        serverConfigurationEditPresenter.OnConfigurationEditSaved += OnConfigurationEditSaved;
        serverConfigurationEditPresenter.OnConfigurationEditCanceled += OnConfigurationEditCanceled;
        serverConfigurationEditPresenter.OnConfigurationDeleted += OnConfigurationDeleted;
        serverConfigurationEditPresenter.OnConfigurationFilesDeleted += OnConfigurationFilesDeleted;
    }

    private void OnEditButtonClicked(ServerConfiguration config)
    {
        SetConfigurationAndSwitchToEdit(config);
    }

    private void OnAddNewConfigurationButtonClicked()
    {
        var serverConfiguration = ServerConfigurationModel.CreateNewServerConfiguration();
        SetConfigurationAndSwitchToEdit(serverConfiguration);
    }

    private void SetConfigurationAndSwitchToEdit(ServerConfiguration config)
    {
        serverConfigurationEditPresenter.ServerConfigurationToEdit = config;
        serverConfigurationListPresenter.gameObject.SetActive(false);
        serverConfigurationEditPresenter.gameObject.SetActive(true);
    }

    private void SwitchToList()
    {
        serverConfigurationListPresenter.gameObject.SetActive(true);
        serverConfigurationEditPresenter.gameObject.SetActive(false);
    }
    
    private void OnConfigurationEditSaved()
    {
        var config = serverConfigurationEditPresenter.ServerConfigurationToEdit;
        if (ServerConfigurationModel.Contains(config) == false)
        {
            //TODO: Check that a config with the same name doesn't exist, or do this in input field validation in ServerConfigurationEditPresenter
            ServerConfigurationModel.AddServerConfiguration(config);
        }
        else
        {
            //Save changes
            ServerConfigurationModel.SaveServerConfigurations();
        }

        SwitchToList();
    }
    
    private void OnConfigurationEditCanceled()
    {
        serverConfigurationEditPresenter.ServerConfigurationToEdit = null;
        SwitchToList();
    }
    
    private void OnConfigurationDeleted()
    {
        ServerConfigurationModel.DeleteConfiguration(serverConfigurationEditPresenter.ServerConfigurationToEdit);
        SwitchToList();
    }
    
    private void OnConfigurationFilesDeleted()
    {
        var directoryInfo = new DirectoryInfo(serverConfigurationEditPresenter.ServerConfigurationToEdit.GetPathToSaveFiles());
        if (directoryInfo.Exists)
        {
            directoryInfo.Delete(true);
        }
        serverConfigurationEditPresenter.ServerConfigurationToEdit.AllFilesDownloaded = false;
        
        ServerConfigurationModel.SaveServerConfigurations();
    }
}
