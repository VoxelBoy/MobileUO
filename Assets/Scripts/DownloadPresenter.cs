using System;
using UnityEngine;
using UnityEngine.UI;

public class DownloadPresenter : MonoBehaviour
{
    [SerializeField]
    private Text counterText;
    
    [SerializeField]
    private Text errorText;

    [SerializeField]
    private Button backButton;

    public Action backButtonPressed;

    private void OnEnable()
    {
        backButton.onClick.AddListener(() => backButtonPressed?.Invoke());
        backButton.gameObject.SetActive(false);
        errorText.gameObject.SetActive(false);
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

    public void UpdateCounter(int numberOfFilesDownloaded, int numberOfFilesToDownload)
    {
        counterText.text = $"Files downloaded: {numberOfFilesDownloaded}/{numberOfFilesToDownload}";
    }
}