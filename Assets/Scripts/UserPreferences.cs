using System;
using UnityEngine;

public static class UserPreferences
{
    private const string customScaleSizePrefKey = "customScaleSize";
    private const string customScaleSizeDefaultKey = "Default";
    private static ScaleSizes customScaleSize;

    public static ScaleSizes CustomScaleSize
    {
        get => customScaleSize;
        set
        {
            customScaleSize = value;
            PlayerPrefs.SetString(customScaleSizePrefKey, customScaleSize.ToString());
        }
    }

    public static Action CustomScaleSizeChanged;

    public static void Initialize()
    {
        customScaleSize = (ScaleSizes) Enum.Parse(typeof(ScaleSizes), PlayerPrefs.GetString(customScaleSizePrefKey, customScaleSizeDefaultKey));
        CustomScaleSizeChanged?.Invoke();
    }
}