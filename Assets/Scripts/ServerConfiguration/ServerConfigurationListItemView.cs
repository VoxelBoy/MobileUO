using System;
using UnityEngine;
using UnityEngine.UI;

public class ServerConfigurationListItemView : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button addOrEditButton;
    [SerializeField] private Text addOrEditButtonText;

    private ServerConfiguration config;

    public Action<ServerConfiguration> SelectCallback;
    public Action<ServerConfiguration> AddOrEditCallback;

    private void OnEnable()
    {
        selectButton.onClick.AddListener(() => SelectCallback?.Invoke(config));
        addOrEditButton.onClick.AddListener(() => AddOrEditCallback?.Invoke(config));
    }

    private void OnDisable()
    {
        selectButton.onClick.RemoveAllListeners();
        addOrEditButton.onClick.RemoveAllListeners();
    }
    
    public void SetServerConfiguration(ServerConfiguration config)
    {
        this.config = config;
        nameText.text = config.Name;
    }

    public void ShowAddButtonInsteadOfEdit(bool addInsteadOfEdit)
    {
        addOrEditButtonText.text = addInsteadOfEdit ? "Add" : "Edit";
    }
}