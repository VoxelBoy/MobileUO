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
            if (customScaleSize != value)
            {
                customScaleSize = value;
                PlayerPrefs.SetString(customScaleSizePrefKey, customScaleSize.ToString());
                CustomScaleSizeChanged?.Invoke();
            }
        }
    }

    public static Action CustomScaleSizeChanged;

    public static void Initialize()
    {
        CustomScaleSize = (ScaleSizes) Enum.Parse(typeof(ScaleSizes), PlayerPrefs.GetString(customScaleSizePrefKey, customScaleSizeDefaultKey));
    }
}