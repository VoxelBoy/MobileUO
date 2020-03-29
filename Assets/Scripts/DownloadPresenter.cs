using UnityEngine;
using UnityEngine.UI;

public class DownloadPresenter : MonoBehaviour
{
    [SerializeField] private Text counterText;
    public void UpdateCounter(int numberOfFilesDownloaded, int numberOfFilesToDownload)
    {
        counterText.text = $"Files downloaded: {numberOfFilesDownloaded}/{numberOfFilesToDownload}";
    }
}