using UnityEngine;
using UnityEngine.UI;

public class JoystickOpacityController : MonoBehaviour
{
    [SerializeField]
    private Image backgroundImage;
    
    [SerializeField]
    private Image handleImage;

    [SerializeField]
    private float backgroundMinimumAlpha;
    
    [SerializeField]
    private float backgroundAlphaStep;
    
    [SerializeField]
    private float handleMinimumAlpha;
    
    [SerializeField]
    private float handleAlphaStep;

    private void Awake()
    {
        UserPreferences.JoystickOpacity.ValueChanged += OnJoystickOpacityChanged;
        OnJoystickOpacityChanged(UserPreferences.JoystickOpacity.CurrentValue);
    }

    private void OnJoystickOpacityChanged(int opacity)
    {
        var backgroundAlpha = backgroundMinimumAlpha / 255f + opacity * (backgroundAlphaStep / 255f);
        var handleAlpha = handleMinimumAlpha / 255f + opacity * (handleAlphaStep / 255f);

        var color = backgroundImage.color;
        color.a = backgroundAlpha;
        backgroundImage.color = color;

        color = handleImage.color;
        color.a = handleAlpha;
        handleImage.color = color;
    }
}
