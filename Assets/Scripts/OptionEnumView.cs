using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OptionEnumView : MonoBehaviour
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Text labelText;
    [SerializeField] private Text enumText;
    
    private UserPreferences.IntPreference intPreference;
    
    private string[] enumNames;
    private List<int> enumValues;
    private bool useValuesInsteadOfNames;
    private bool usePercentage;

    public void Initialize(Type enumType, UserPreferences.IntPreference intPreference, string labelText, bool useValuesInsteadOfNames, bool usePercentage)
    {
        this.intPreference = intPreference;
        this.useValuesInsteadOfNames = useValuesInsteadOfNames;
        this.usePercentage = usePercentage;
        this.labelText.text = labelText;

        intPreference.ValueChanged += OnValueChanged;

        enumNames = Enum.GetNames(enumType);
        enumValues = Enum.GetValues(enumType).Cast<int>().ToList();

        UpdateText();

        leftButton.onClick.AddListener(OnLeftButtonClicked);
        rightButton.onClick.AddListener(OnRightButtonClicked);
    }

    private void OnValueChanged(int value)
    {
        UpdateText();
    }

    public void SetInteractable(bool interactable)
    {
        leftButton.interactable = interactable;
        rightButton.interactable = interactable;
        enumText.color = interactable ? Color.black : Color.gray;
    }

    private void UpdateText()
    {
        string text;
        if (useValuesInsteadOfNames)
        {
            var value = intPreference.CurrentValue;
            if (usePercentage)
            {
                var floatValue = value / 100f;
                text = floatValue.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                text = value.ToString();
            }
        }
        else
        {
            var index = enumValues.IndexOf(intPreference.CurrentValue);
            if (index > -1)
            {
                text = enumNames[index];
            }
            else
            {
                text = "Invalid";
                Debug.LogWarning($"Could not find valid enum value for {intPreference.CurrentValue}");
            }
        }

        enumText.text = text;
    }

    private void OnLeftButtonClicked()
    {
        UpdateValue(-1);
    }
    
    private void OnRightButtonClicked()
    {
        UpdateValue(1);
    }

    private void UpdateValue(int direction)
    {
        var index = enumValues.IndexOf(intPreference.CurrentValue);
        index += direction;
        
        //Wrap around
        if (index < 0)
        {
            index += enumNames.Length;
        }
        else if (index >= enumNames.Length)
        {
            index -= enumNames.Length;
        }
        
        intPreference.CurrentValue = enumValues[index];
    }
}