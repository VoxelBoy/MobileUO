using UnityEngine;
using UnityEngine.UI;

public class TextureFilteringButtonPresenter : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text text;

    private void Awake()
    {
        button.onClick.AddListener(OnButtonClicked);
        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        var filterModeText = UserPreferences.TextureFiltering == FilterMode.Point ? "Sharp" : "Smooth";
        text.text = "Texture Filtering: " + filterModeText;
    }

    private void OnButtonClicked()
    {
        UserPreferences.TextureFiltering = UserPreferences.TextureFiltering == FilterMode.Point ? FilterMode.Bilinear : FilterMode.Point;
        UpdateButtonText();
    }
}
