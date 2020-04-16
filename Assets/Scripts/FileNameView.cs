using UnityEngine;
using UnityEngine.UI;

public class FileNameView : MonoBehaviour
{
    [SerializeField]
    private Text text;

    [SerializeField]
    private Image fillImage;

    public void SetText(string text)
    {
        this.text.text = text;
    }

    public void SetDownloaded()
    {
        text.color = Color.green;
        fillImage.enabled = false;
    }

    public void SetProgress(float progress)
    {
        fillImage.enabled = true;
        fillImage.fillAmount = progress;
    }
}
