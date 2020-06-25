using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownloadPresenter : MonoBehaviour
{
    [SerializeField]
    private Text counterText;
    
    [SerializeField]
    private Text errorText;

    [SerializeField]
    private GameObject filesScrollView;
    
    [SerializeField]
    private FileNameView fileNameViewInstance;

    [SerializeField]
    private Button backButton;

    [SerializeField]
    private GameObject cellularWarningParent;

    [SerializeField]
    private Button cellularWarningYesButton;

    [SerializeField]
    private Button cellularWarningNoButton;

    public Action BackButtonPressed;
    public Action CellularWarningYesButtonPressed;
    public Action CellularWarningNoButtonPressed;

    private readonly Dictionary<string, FileNameView> fileNameToFileNameView = new Dictionary<string, FileNameView>();

    private void OnEnable()
    {
        backButton.onClick.AddListener(() => BackButtonPressed?.Invoke());
        cellularWarningYesButton.onClick.AddListener(() => CellularWarningYesButtonPressed?.Invoke());
        cellularWarningNoButton.onClick.AddListener(() => CellularWarningNoButtonPressed?.Invoke());
        
        cellularWarningParent.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        errorText.gameObject.SetActive(false);
        fileNameViewInstance.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        backButton.onClick.RemoveAllListeners();
    }

    public void ShowError(string error)
    {
        backButton.gameObject.SetActive(true);
        errorText.gameObject.SetActive(true);
        errorText.text = error;
    }

    public void UpdateView(int numberOfFilesDownloaded, int numberOfFilesToDownload)
    {
        if (counterText != null)
        {
            counterText.text = $"Files downloaded: {numberOfFilesDownloaded}/{numberOfFilesToDownload}";
        }
    }

    public void SetFileDownloaded(string file)
    {
        var fileNameView = fileNameToFileNameView[file];
        if (fileNameView != null)
        {
            fileNameView.SetDownloaded();
        }
    }

    public void SetFileList(List<string> filesList)
    {
        var filenameTextInstanceGameObject = fileNameViewInstance.gameObject;
        var filenameTextInstanceTransformParent = fileNameViewInstance.transform.parent;
        filesList.ForEach(file =>
        {
            var newFileNameText = Instantiate(filenameTextInstanceGameObject, filenameTextInstanceTransformParent).GetComponent<FileNameView>();
            newFileNameText.SetText(file);
            fileNameToFileNameView.Add(file, newFileNameText);
            newFileNameText.gameObject.SetActive(true);
        });
        filesScrollView.SetActive(true);
    }

    public void ClearFileList()
    {
        foreach (var keyValuePair in fileNameToFileNameView)
        {
            if (keyValuePair.Value != null)
            {
                Destroy(keyValuePair.Value.gameObject);
            }
        }
        
        fileNameToFileNameView.Clear();
        filesScrollView.SetActive(false);
    }

    public void SetDownloadProgress(string file, float progress)
    {
        if (fileNameToFileNameView.TryGetValue(file, out var fileNameView))
        {
            fileNameView?.SetProgress(progress);
        }
    }

    public void ToggleCellularWarning(bool enabled)
    {
        cellularWarningParent.SetActive(enabled);
    }
}