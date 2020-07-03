using UnityEngine;
using UnityEngine.UI;

public class ModifierKeyButtonPresenter : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image image;

    private readonly Color activeColor = new Color(155 / 255f, 1f, 1f, 1f);
    private readonly Color inactiveColor = Color.white;
    
    public bool ToggledOn { get; private set; }

    private void Awake()
    {
        button.onClick.AddListener(OnButtonClicked);
    }
    
    private void OnButtonClicked()
    {
        ToggledOn = !ToggledOn;
        image.color = ToggledOn ? activeColor : inactiveColor;
    }
}