using System;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ServerConfigurationEditPresenter : MonoBehaviour
{
    private ServerConfiguration serverConfigurationToEdit;
    public ServerConfiguration ServerConfigurationToEdit
    {
        get => serverConfigurationToEdit;
        set
        {
            serverConfigurationToEdit = value;
            UpdateInputFields();
        }
    }

    [SerializeField]
    private InputField serverNameInputField;
    
    [SerializeField]
    private InputField uoServerUrlInputField;
    
    [SerializeField]
    private InputField fileDownloadServerUrlInputField;
    
    [SerializeField]
    private InputField clientVersionInputField;
    
    [SerializeField]
    private InputField clientPathForUnityEditorInputField;
    
    [SerializeField]
    private GameObject clientPathForUnityEditorParent;

    [SerializeField]
    private Button saveButton;
    
    [SerializeField]
    private Button cancelButton;
    
    [SerializeField]
    private Button deleteServerConfigurationButton;
    
    [SerializeField]
    private Text deleteServerConfigurationButtonText;
    
    [SerializeField]
    private Button deleteServerFilesButton;

    [SerializeField]
    private Text deleteServerFilesButtonText;

    public Action OnConfigurationEditSaved;
    public Action OnConfigurationEditCanceled;
    public Action OnConfigurationDeleted;
    public Action OnConfigurationFilesDeleted;

    private int deleteServerConfigurationButtonClickCount;
    private float deleteServerConfigurationButtonClickTime;
    private string deleteServerConfigurationButtonOriginalText;
    
    private int deleteServerFilesButtonClickCount;
    private float deleteServerFilesButtonClickTime;
    private string deleteServerFilesButtonOriginalText;
    
    private const string deleteButtonConfirmText = "Click again to Delete!";
    private const float deleteButtonRevertDuration = 2f;
    
    private void UpdateInputFields()
    {
        serverNameInputField.text = serverConfigurationToEdit?.Name ?? "";
        uoServerUrlInputField.text = serverConfigurationToEdit?.UoServerUrl ?? "";
        fileDownloadServerUrlInputField.text = serverConfigurationToEdit?.FileDownloadServerUrl ?? "";
        clientVersionInputField.text = serverConfigurationToEdit?.ClientVersion ?? "";
        clientPathForUnityEditorInputField.text = serverConfigurationToEdit?.ClientPathForUnityEditor ?? "";
        clientPathForUnityEditorParent.SetActive(Application.isEditor);
        
        deleteServerConfigurationButtonOriginalText = deleteServerConfigurationButtonText.text;
        deleteServerFilesButtonOriginalText = deleteServerFilesButtonText.text;
        
        ResetDeleteServerConfigurationButton();
        ResetDeleteServerFilesButton();
    }

    private void OnEnable()
    {
        saveButton.onClick.AddListener(OnSaveButtonClicked);
        cancelButton.onClick.AddListener(() => OnConfigurationEditCanceled?.Invoke());
        deleteServerConfigurationButton.onClick.AddListener(OnDeleteServerConfigurationButtonClicked);
        deleteServerFilesButton.onClick.AddListener(OnDeleteServerFilesButtonClicked);
    }

    private void OnDisable()
    {
        saveButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        deleteServerConfigurationButton.onClick.RemoveAllListeners();
        deleteServerFilesButton.onClick.RemoveAllListeners();
        
        ResetDeleteServerConfigurationButton();
        ResetDeleteServerFilesButton();
    }
    
    private void OnSaveButtonClicked()
    {
        //TODO: Implement validation
        if (ServerConfigurationToEdit.Name != serverNameInputField.text)
        {
            //Rename directory where client files are saved, if it exists
            var currentDirectoryPath = serverConfigurationToEdit.GetPathToSaveFiles();
            var directoryInfo = new DirectoryInfo(currentDirectoryPath);
            if (directoryInfo.Exists)
            {
                var newDirectoryPath = Path.Combine(Application.persistentDataPath, serverNameInputField.text);
                Directory.Move(currentDirectoryPath, newDirectoryPath);
            }
            ServerConfigurationToEdit.Name = serverNameInputField.text;
        }
        ServerConfigurationToEdit.UoServerUrl = uoServerUrlInputField.text;
        ServerConfigurationToEdit.FileDownloadServerUrl = fileDownloadServerUrlInputField.text;
        ServerConfigurationToEdit.ClientVersion = clientVersionInputField.text;
        ServerConfigurationToEdit.ClientPathForUnityEditor = clientPathForUnityEditorInputField.text;
        
        OnConfigurationEditSaved?.Invoke();
    }

    private void OnDeleteServerConfigurationButtonClicked()
    {
        if (deleteServerConfigurationButtonClickCount == 0)
        {
            deleteServerConfigurationButtonText.text = deleteButtonConfirmText;
            deleteServerConfigurationButtonClickCount++;
            deleteServerConfigurationButtonClickTime = Time.time;
        }
        else
        {
            OnConfigurationDeleted?.Invoke();
            serverConfigurationToEdit = null;
            ResetDeleteServerConfigurationButton();
        }
    }
    
    private void OnDeleteServerFilesButtonClicked()
    {
        if (deleteServerFilesButtonClickCount == 0)
        {
            deleteServerFilesButtonText.text = deleteButtonConfirmText;
            deleteServerFilesButtonClickCount++;
            deleteServerFilesButtonClickTime = Time.time;
        }
        else
        {
            OnConfigurationFilesDeleted?.Invoke();
            ResetDeleteServerFilesButton();
        }
    }

    private void Update()
    {
        if (deleteServerConfigurationButtonClickCount > 0 && Time.time > deleteServerConfigurationButtonClickTime + deleteButtonRevertDuration)
        {
            ResetDeleteServerConfigurationButton();
        }
        
        if (deleteServerFilesButtonClickCount > 0 && Time.time > deleteServerFilesButtonClickTime + deleteButtonRevertDuration)
        {
            ResetDeleteServerFilesButton();
        }
    }

    private void ResetDeleteServerConfigurationButton()
    {
        deleteServerConfigurationButtonText.text = deleteServerConfigurationButtonOriginalText;
        deleteServerConfigurationButtonClickCount = 0;
    }
    
    private void ResetDeleteServerFilesButton()
    {
        deleteServerFilesButtonText.text = deleteServerFilesButtonOriginalText;
        deleteServerFilesButtonClickCount = 0;
    }
}