using UnityEngine;
using UnityEngine.UI;

public class CustomizeJoystickButtonPresenter : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image image;
    [SerializeField] private JoystickScaler joystickScaler;

    private readonly Color activeColor = Color.white;
    private readonly Color inactiveColor = new Color(155 / 255f, 1f, 1f, 1f);

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
