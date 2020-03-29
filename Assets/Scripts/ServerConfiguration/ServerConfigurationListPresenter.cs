using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerConfigurationListPresenter : MonoBehaviour
{
    [SerializeField] private ServerConfigurationListItemView serverConfigurationViewInstance;
    [SerializeField] private Button addNewConfigurationButton;

    public Action AddNewConfigurationButtonClicked;
    public Action<ServerConfiguration> EditButtonClicked;

    private List<ServerConfigurationListItemView> viewsCreated = new List<ServerConfigurationListItemView>();
    
    private void OnEnable()
    {
        serverConfigurationViewInstance.gameObject.SetActive(false);
        RecreateViews();
        addNewConfigurationButton.onClick.AddListener(OnAddNewConfigurationButtonClicked);
    }

    private void OnDisable()
    {
        addNewConfigurationButton.onClick.RemoveAllListeners();
        DestroyViews();
    }

    private void RecreateViews()
    {
        DestroyViews();
        ServerConfigurationModel.ServerConfigurations.ForEach(config =>
        {
            var view = Instantiate(serverConfigurationViewInstance.gameObject, serverConfigurationViewInstance.transform.parent).GetComponent<ServerConfigurationListItemView>();
            view.SetServerConfiguration(config);
            view.SelectCallback = ServerConfigurationListItemSelect;
            view.EditCallback = ServerConfigurationListItemEdit;
            view.transform.SetSiblingIndex(addNewConfigurationButton.transform.GetSiblingIndex() - 1);
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
        EditButtonClicked?.Invoke(config);
    }

    private void ServerConfigurationListItemSelect(ServerConfiguration config)
    {
        ServerConfigurationModel.ActiveConfiguration = config;
    }
    
    private void OnAddNewConfigurationButtonClicked()
    {
        AddNewConfigurationButtonClicked?.Invoke();
        RecreateViews();
    }
}