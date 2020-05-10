using UnityEngine;
using UnityEngine.UI;

public class OpenUrlButtonView : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private string url;

    void Start()
    {
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        Application.OpenURL(url);
    }
}
