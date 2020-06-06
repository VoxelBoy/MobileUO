using UnityEngine;
using UnityEngine.UI;

public class CustomizeJoystickButtonPresenter : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image image;
    [SerializeField] private JoystickScaler joystickScaler;

    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;

    private void Awake()
    {
        button.onClick.AddListener(OnButtonClicked);
    }
    
    private void OnButtonClicked()
    {
        joystickScaler.gameObject.SetActive(joystickScaler.gameObject.activeSelf == false);
        image.color = joystickScaler.gameObject.activeSelf ? activeColor : inactiveColor;
    }
}
