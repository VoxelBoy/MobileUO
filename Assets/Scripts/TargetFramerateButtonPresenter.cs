using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TargetFramerateButtonPresenter : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Text text;

    private int targetFrameRateIndex;
    private Array targetFrameRatesValues;
    private int targetFrameRatesValuesLength;

    private void Awake()
    {
        targetFrameRatesValues = Enum.GetValues(typeof(UserPreferences.TargetFrameRates));
        targetFrameRatesValuesLength = targetFrameRatesValues.Length;
        targetFrameRateIndex = Enum.GetNames(typeof(UserPreferences.TargetFrameRates)).ToList().IndexOf(UserPreferences.TargetFrameRate.ToString());
        button.onClick.AddListener(OnButtonClicked);
        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        text.text = $"Target FPS: {(int) UserPreferences.TargetFrameRate}";
    }

    private void OnButtonClicked()
    {
        targetFrameRateIndex++;
        if (targetFrameRateIndex >= targetFrameRatesValuesLength)
        {
            targetFrameRateIndex -= targetFrameRatesValuesLength;
        }
        var targetFrameRate = (UserPreferences.TargetFrameRates) targetFrameRatesValues.GetValue(targetFrameRateIndex);
        UserPreferences.TargetFrameRate = targetFrameRate;
        
        UpdateButtonText();
    }
}
