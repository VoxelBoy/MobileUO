using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Gumps;
using UnityEngine;
using UnityEngine.UI;

public class ToggleCloseButtonsPresenter : MonoBehaviour
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
        var onOff = Gump.CloseButtonsEnabled ? "On" : "Off";
        text.text = $"Close Buttons: " + onOff;
    }
    
    private void OnButtonClicked()
    {
        Gump.CloseButtonsEnabled = Gump.CloseButtonsEnabled == false;
        
        foreach (var control in UIManager.Gumps)
        {
            if (control is Gump gump)
            {
                gump.ToggleCloseButtonEnabled();
            }
        }
        
        UpdateButtonText();
    }
}
