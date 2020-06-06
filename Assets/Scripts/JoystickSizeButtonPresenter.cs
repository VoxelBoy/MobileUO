using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JoystickSizeButtonPresenter : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text text;

    private int joystickSizesIndex;
    private Array joystickSizesValues;
    private int joystickSizesValuesLength;

    private void Awake()
    {
        UserPreferences.JoystickSizeChanged += UpdateButtonText;
        joystickSizesValues = Enum.GetValues(typeof(UserPreferences.JoystickSizes));
        joystickSizesValuesLength = joystickSizesValues.Length;
        joystickSizesIndex = Enum.GetNames(typeof(UserPreferences.JoystickSizes)).ToList().IndexOf(UserPreferences.JoystickSize.ToString());
        button.onClick.AddListener(OnButtonClicked);
        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        text.text = $"Joystick Size: {UserPreferences.JoystickSize}";
    }

    private void OnButtonClicked()
    {
        joystickSizesIndex++;
        if (joystickSizesIndex >= joystickSizesValuesLength)
        {
            joystickSizesIndex -= joystickSizesValuesLength;
        }
        var joystickSize = (UserPreferences.JoystickSizes) joystickSizesValues.GetValue(joystickSizesIndex);
        UserPreferences.JoystickSize = joystickSize;
        
        UpdateButtonText();
    }
}
