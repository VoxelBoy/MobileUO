using UnityEngine;
using UnityEngine.UI;

public class ToggleButtonPresenter : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image image;

    private readonly Color activeColor = new Color(155 / 255f, 1f, 1f, 1f);
    private readonly Color inactiveColor = Color.white;    

    private bool active;

    private void Awake()
    {
        button.onClick.AddListener(OnButtonClicked);
    }
    
    private void OnButtonClicked()
    {
        active = !active;
        image.color = active ? activeColor : inactiveColor;
    }
}