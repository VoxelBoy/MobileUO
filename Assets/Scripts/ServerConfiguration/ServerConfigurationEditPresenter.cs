using System;
using System.Collections;
using System.IO;
using System.Linq;
using ClassicUO.Data;
using DG.Tweening;
using Rssdp;
using UnityEngine;
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
    private InputField uoServerPortInputField;
    
    [SerializeField]
    private InputField fileDownloadServerUrlInputField;
    
    [SerializeField]
    private InputField fileDownloadServerPortInputField;
    
    [SerializeField]
    private InputField clientVersionInputField;

    [SerializeField]
    private Toggle useEncryptionToggle;
    
    [SerializeField]
    private Toggle useExternalStorageToggle;
    
    [SerializeField]
    private GameObject useExternalStorageParent;
    
    [SerializeField]
    private InputField clientPathForUnityEditorInputField;
    
    [SerializeField]
    private GameObject clientPathForUnityEditorParent;

    [SerializeField]
    private Button saveButton;

    [SerializeField]
    private Transform saveButtonTransform;
    
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
    
    [SerializeField]
    private Button markFilesAsDownloadedButton;

    [SerializeField]
    private GameObject documentationButtonParent;

    [SerializeField]
    private Text validationErrorText;
    
    [SerializeField]
    private GameObject validationErrorTextParent;

    [SerializeField]
    private Button discoverButton;

    [SerializeField]
    private ServerDiscoveryButtonPresenter serverDiscoveryButtonPresenter;

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

    private Coroutine showErrorCoroutine;
    private Vector3 saveButtonOriginalLocalPosition;

    private const string deleteButtonConfirmText = "Click again to Delete!";
    private const float deleteButtonRevertDuration = 2f;
    
    private void UpdateInputFields()
    {
        serverNameInputField.text = serverConfigurationToEdit?.Name ?? "";
        uoServerUrlInputField.text = serverConfigurationToEdit?.UoServerUrl ?? "";
        uoServerPortInputField.text = serverConfigurationToEdit?.UoServerPort ?? "2593";
        fileDownloadServerUrlInputField.text = serverConfigurationToEdit?.FileDownloadServerUrl ?? "";
        fileDownloadServerPortInputField.text = serverConfigurationToEdit?.FileDownloadServerPort ?? DownloadState.DefaultFileDownloadPort;
        clientVersionInputField.text = serverConfigurationToEdit?.ClientVersion ?? "";
        useEncryptionToggle.isOn = serverConfigurationToEdit?.UseEncryption ?? false;
        useExternalStorageToggle.isOn = serverConfigurationToEdit?.PreferExternalStorage ?? false;
        clientPathForUnityEditorInputField.text = serverConfigurationToEdit?.ClientPathForUnityEditor ?? "";
        clientPathForUnityEditorParent.SetActive(Application.isMobilePlatform == false);
        
        documentationButtonParent.SetActive(true);
        validationErrorTextParent.SetActive(false);
        
        useExternalStorageParent.SetActive(Application.platform == RuntimePlatform.Android);
        
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
        markFilesAsDownloadedButton.onClick.AddListener(OnMarkFilesAsDownloadedButtonClicked);
        discoverButton.onClick.AddListener(SearchForDevices);

        saveButtonOriginalLocalPosition = saveButtonTransform.localPosition;
    }

    private async void SearchForDevices()
    {
        serverDiscoveryButtonPresenter.Toggle(true);
        using (var deviceLocator = new SsdpDeviceLocator())
        {
            try
            {
                var foundDevices = await deviceLocator.SearchAsync("urn:UOFileServer:device:UOFileServer:1");
                //Check isActiveAndEnabled in case the Edit Presenter exited already
                if (isActiveAndEnabled)
                {
                    var device = foundDevices.FirstOrDefault();
                    if (device == null)
                    {
                        Debug.Log("UOFileServer device was not found.");
                    }
                    else
                    {
                        var ip = device.DescriptionLocation.Authority;
                        Debug.Log($"UOFileServer device found at {ip}");
                        
                        var responseHeaders = device.ResponseHeaders;
                        if (responseHeaders.TryGetValues("LoginServer", out var loginServerValues))
                        {
                            uoServerUrlInputField.text = loginServerValues.FirstOrDefault();
                        }

                        if (responseHeaders.TryGetValues("LoginPort", out var loginPortValues))
                        {
                            uoServerPortInputField.text = loginPortValues.FirstOrDefault();
                        }

                        if (responseHeaders.TryGetValues("ClientVersion", out var clientVersionValues))
                        {
                            clientVersionInputField.text = clientVersionValues.FirstOrDefault();
                        }
                    
                        fileDownloadServerUrlInputField.text = ip;
                        fileDownloadServerPortInputField.text = DownloadState.DefaultFileDownloadPort;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        serverDiscoveryButtonPresenter.Toggle(false);
    }

    private void OnDisable()
    {
        saveButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        deleteServerConfigurationButton.onClick.RemoveAllListeners();
        deleteServerFilesButton.onClick.RemoveAllListeners();
        markFilesAsDownloadedButton.onClick.RemoveAllListeners();
        
        ResetDeleteServerConfigurationButton();
        ResetDeleteServerFilesButton();
    }
    
    private void OnSaveButtonClicked()
    {
        var valid = ValidateFields(out var validationError);
        if (valid == false)
        {
            ShowError(validationError);
            return;
        }
        
        if (ServerConfigurationToEdit.Name != serverNameInputField.text)
        {
            //Rename directory where client files are saved, if it exists
            var currentDirectoryPath = serverConfigurationToEdit.GetPathToSaveFiles();
            var directoryInfo = new DirectoryInfo(currentDirectoryPath);
            
            ServerConfigurationToEdit.Name = serverNameInputField.text;
            var newDirectoryPath = serverConfigurationToEdit.GetPathToSaveFiles();
            
            if (directoryInfo.Exists)
            {
                Directory.Move(currentDirectoryPath, newDirectoryPath);
            }
            else
            {
                Directory.CreateDirectory(newDirectoryPath);
            }
        }
        ServerConfigurationToEdit.UoServerUrl = uoServerUrlInputField.text;
        ServerConfigurationToEdit.UoServerPort = uoServerPortInputField.text;
        ServerConfigurationToEdit.FileDownloadServerUrl = fileDownloadServerUrlInputField.text;
        ServerConfigurationToEdit.FileDownloadServerPort = fileDownloadServerPortInputField.text;
        ServerConfigurationToEdit.ClientVersion = clientVersionInputField.text;
        ServerConfigurationToEdit.UseEncryption = useEncryptionToggle.isOn;
        ServerConfigurationToEdit.PreferExternalStorage = useExternalStorageToggle.isOn;
        ServerConfigurationToEdit.ClientPathForUnityEditor = clientPathForUnityEditorInputField.text;
        
        OnConfigurationEditSaved?.Invoke();
    }

    private void ShowError(string validationError)
    {
        if (showErrorCoroutine != null)
        {
            StopCoroutine(showErrorCoroutine);
        }
        
        documentationButtonParent.SetActive(false);
        
        validationErrorText.text = validationError;
        validationErrorTextParent.SetActive(true);

        saveButtonTransform.localPosition = saveButtonOriginalLocalPosition;
        DOTween.Kill(saveButtonTransform);
        saveButtonTransform.DOShakePosition(1f, 10f);

        showErrorCoroutine = StartCoroutine(HideValidationErrorText());
    }

    private IEnumerator HideValidationErrorText()
    {
        yield return new WaitForSeconds(4);
        documentationButtonParent.SetActive(true);
        validationErrorTextParent.SetActive(false);
        saveButtonTransform.localPosition = saveButtonOriginalLocalPosition;
    }

    private bool ValidateFields(out string validationError)
    {
        //Server Name validation
        var serverName = serverNameInputField.text;
        if (string.IsNullOrWhiteSpace(serverName))
        {
            validationError = "Server Name cannot be empty.";
            return false;
        }

        if (serverName.IndexOfAny(Path.GetInvalidPathChars()) != -1)
        {
            validationError = "Server Name contains illegal characters for filesystem path.";
            return false;
        }

        var configsWithSameName = ServerConfigurationModel.ServerConfigurations.Where(x => x.Name == serverName).ToList();
        configsWithSameName.Remove(serverConfigurationToEdit);
        if (configsWithSameName.Count > 0)
        {
            validationError = "Server Name cannot be the same as that of another configuration.";
            return false;
        }

        //UO Server Url validation
        var uoServerUrl = uoServerUrlInputField.text;
        if (string.IsNullOrWhiteSpace(uoServerUrl))
        {
            validationError = "UO Server Address cannot be empty.";
            return false;
        }

        //UO Server Port validation
        var uoServerPort = uoServerPortInputField.text;
        if (int.TryParse(uoServerPort, out var uoServerPortResult) == false)
        {
            validationError = "UO Server port is not valid.";
            return false;
        }

        if (uoServerPortResult < 0 || uoServerPortResult > 65535)
        {
            validationError = "UO Server port needs to be between 0 and 65535";
            return false;
        }

        //Skip file download server url and port validation if AllFilesDownloaded is set to true
        if (serverConfigurationToEdit.AllFilesDownloaded == false)
        {
            //File Download Server Url validation
            var fileDownloadServerUrl = fileDownloadServerUrlInputField.text;
            if (string.IsNullOrWhiteSpace(fileDownloadServerUrl))
            {
                validationError = "File Download Server Address cannot be empty.";
                return false;
            }

            try
            {
                var unused = new UriBuilder("http", fileDownloadServerUrl, -1, null).Uri;
            }
            catch (Exception e)
            {
                validationError = "File Download Server Address is not valid";
                return false;
            }

            //File Server Port validation
            var fileServerPort = fileDownloadServerPortInputField.text;
            if (int.TryParse(fileServerPort, out var fileServerPortResult) == false)
            {
                validationError = "File Download Server port is not valid.";
                return false;
            }

            if (fileServerPortResult < 0 || fileServerPortResult > 65535)
            {
                validationError = "File Download Server port needs to be between 0 and 65535";
                return false;
            }
        }

        //Client version validation
        var clientVersion = clientVersionInputField.text;
        if (ClientVersionHelper.IsClientVersionValid(clientVersion, out _) == false)
        {
            validationError = "Client Version is not valid";
            return false;
        }

        validationError = string.Empty;
        return true;
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
    
    private void OnMarkFilesAsDownloadedButtonClicked()
    {
        serverConfigurationToEdit.AllFilesDownloaded = true;
        ResetDeleteServerFilesButton();
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
        if (ServerConfigurationToEdit != null)
        {
            deleteServerFilesButton.gameObject.SetActive(ServerConfigurationToEdit.AllFilesDownloaded);
            markFilesAsDownloadedButton.gameObject.SetActive(deleteServerFilesButton.gameObject.activeSelf == false);
        }
    }
}