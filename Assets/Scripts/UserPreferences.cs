using System;
using UnityEngine;

public static class UserPreferences
{
    public enum ScaleSizes
    {
        Default = 100,
        OneTwentyFive = 125,
        OneHalf = 150,
        OneSeventyFive = 175,
        Two = 200
    }

    public enum TargetFrameRates
    {
        Thirty = 30,
        Sixty = 60
    }
    
    private const string customScaleSizePrefKey = "customScaleSize";
    private const string customScaleSizeDefaultValue = "Default";
    private static ScaleSizes customScaleSize;
    public static Action CustomScaleSizeChanged;

    public static ScaleSizes CustomScaleSize
    {
        get => customScaleSize;
        set
        {
            if (customScaleSize != value)
            {
                customScaleSize = value;
                PlayerPrefs.SetString(customScaleSizePrefKey, customScaleSize.ToString());
                CustomScaleSizeChanged?.Invoke();
            }
        }
    }

    
    private const string targetFrameRatePrefKey = "targetFrameRate";
    private const int targetFrameRateDefaultValue = 60;
    private static TargetFrameRates targetFrameRate;
    public static Action TargetFrameRateChanged;

    public static TargetFrameRates TargetFrameRate
    {
        get => targetFrameRate;
        set
        {
            if (targetFrameRate != value)
            {
                targetFrameRate = value;
                PlayerPrefs.SetInt(targetFrameRatePrefKey, (int) targetFrameRate);
                TargetFrameRateChanged?.Invoke();
            }
        }
    }

    public static void Initialize()
    {
        CustomScaleSize = (ScaleSizes) Enum.Parse(typeof(ScaleSizes), PlayerPrefs.GetString(customScaleSizePrefKey, customScaleSizeDefaultValue));
        TargetFrameRate = (TargetFrameRates) PlayerPrefs.GetInt(targetFrameRatePrefKey, targetFrameRateDefaultValue);
    }
}