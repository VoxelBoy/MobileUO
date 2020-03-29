using System;
using UnityEngine;
using UnityEngine.UI;

public class ServerConfigurationListItemView : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button editButton;

    private ServerConfiguration config;

    public Action<ServerConfiguration> SelectCallback;
    public Action<ServerConfiguration> EditCallback;

    private void OnEnable()
    {
        selectButton.onClick.AddListener(() => SelectCallback?.Invoke(config));
        editButton.onClick.AddListener(() => EditCallback?.Invoke(config));
    }

    private void OnDisable()
    {
        selectButton.onClick.RemoveAllListeners();
        editButton.onClick.RemoveAllListeners();
    }
    
    public void SetServerConfiguration(ServerConfiguration config)
    {
        this.config = config;
        nameText.text = config.Name;
    }
}