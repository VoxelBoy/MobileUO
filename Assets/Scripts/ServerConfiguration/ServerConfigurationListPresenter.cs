using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerConfigurationListPresenter : MonoBehaviour
{
    [SerializeField] private ServerConfigurationListItemView serverConfigurationViewInstance;
    [SerializeField] private Button addNewConfigurationButton;
    [SerializeField] private Button supportedServersButton;
    [SerializeField] private Button backButton;

    public Action AddNewConfigurationButtonClicked;
    public Action<ServerConfiguration> EditButtonClicked;

    private List<ServerConfigurationListItemView> viewsCreated = new List<ServerConfigurationListItemView>();

    private bool showingSupportedServerConfigurations;
    
    private void OnEnable()
    {
        serverConfigurationViewInstance.gameObject.SetActive(false);
        RecreateViews();
        addNewConfigurationButton.onClick.AddListener(OnAddNewConfigurationButtonClicked);
        supportedServersButton.onClick.AddListener(OnSupportedServersClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnBackButtonClicked()
    {
        showingSupportedServerConfigurations = false;
        RecreateViews();
    }

    private void OnSupportedServersClicked()
    {
        showingSupportedServerConfigurations = true;
        RecreateViews();
    }

    private void OnDisable()
    {
        addNewConfigurationButton.onClick.RemoveAllListeners();
        DestroyViews();
    }

    private void RecreateViews()
    {
        DestroyViews();
        if (showingSupportedServerConfigurations)
        {
            addNewConfigurationButton.gameObject.SetActive(false);
            supportedServersButton.gameObject.SetActive(false);
            backButton.gameObject.SetActive(true);
            CreateServerConfigurationItemViews(ServerConfigurationModel.SupportedServerConfigurations, true);
            backButton.transform.SetAsLastSibling();
        }
        else
        {
            addNewConfigurationButton.gameObject.SetActive(true);
            supportedServersButton.gameObject.SetActive(true);
            backButton.gameObject.SetActive(false);
            CreateServerConfigurationItemViews(ServerConfigurationModel.ServerConfigurations, false);
            addNewConfigurationButton.transform.SetAsLastSibling();
            supportedServersButton.transform.SetAsLastSibling();
        }
    }

    private void CreateServerConfigurationItemViews(List<ServerConfiguration> configs, bool addInsteadOfEdit)
    {
        configs.ForEach(config =>
        {
            var view = Instantiate(serverConfigurationViewInstance.gameObject, serverConfigurationViewInstance.transform.parent).GetComponent<ServerConfigurationListItemView>();
            view.SetServerConfiguration(config);
            view.ShowAddButtonInsteadOfEdit(addInsteadOfEdit);
            view.SelectCallback = ServerConfigurationListItemSelect;
            view.AddOrEditCallback = ServerConfigurationListItemEdit;
            view.gameObject.SetActive(true);
            viewsCreated.Add(view);
        });
    }

    private void DestroyViews()
    {
        viewsCreated.ForEach(x => Destroy(x.gameObject));
        viewsCreated.Clear();
    }

    private void ServerConfigurationListItemEdit(ServerConfiguration config)
    {
        if (showingSupportedServerConfigurations)
        {
            AddSupportedConfigAndGoBack(config);
        }
        else
        {
            EditButtonClicked?.Invoke(config);
        }
    }

    private void ServerConfigurationListItemSelect(ServerConfiguration config)
    {
        if (showingSupportedServerConfigurations)
        {
            AddSupportedConfigAndGoBack(config);
        }
        else
        {
            ServerConfigurationModel.ActiveConfiguration = config;
        }
    }

    private void AddSupportedConfigAndGoBack(ServerConfiguration config)
    {
        if (ServerConfigurationModel.IsServerConfigurationNameValid(config.Name) == false)
        {
            //TODO: Show error saying there already is a config with this name, or something
            return;
        }
        var configClone = config.Clone();
        configClone.SupportedServer = true;
        ServerConfigurationModel.AddServerConfiguration(configClone);
        OnBackButtonClicked();
    }

    private void OnAddNewConfigurationButtonClicked()
    {
        AddNewConfigurationButtonClicked?.Invoke();
        RecreateViews();
    }
}