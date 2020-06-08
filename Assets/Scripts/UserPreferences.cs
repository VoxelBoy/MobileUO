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

    public enum JoystickSizes
    {
        Small = 0,
        Normal = 1,
        Large = 2,
        Custom = 3
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

    private const string customJoystickPositionAndSizePrefKey = "customJoystickSizeAndPosition";
    private static Vector3 customJoystickPositionAndSize;
    public static Vector3 CustomJoystickPositionAndSize
    {
        get => customJoystickPositionAndSize;
        set
        {
            if (customJoystickPositionAndSize != value)
            {
                customJoystickPositionAndSize = value;
                PlayerPrefs.SetFloat(customJoystickPositionAndSizePrefKey + "X", customJoystickPositionAndSize.x);
                PlayerPrefs.SetFloat(customJoystickPositionAndSizePrefKey + "Y", customJoystickPositionAndSize.y);
                PlayerPrefs.SetFloat(customJoystickPositionAndSizePrefKey + "Z", customJoystickPositionAndSize.z);
            }
        }
    }

    private const string joystickSizePrefKey = "joystickSize";
    private const string joystickSizeDefaultValue = "Normal";
    private static JoystickSizes joystickSize;
    public static Action JoystickSizeChanged;
    public static JoystickSizes JoystickSize
    {
        get => joystickSize;
        set
        {
            if (joystickSize != value)
            {
                joystickSize = value;
                PlayerPrefs.SetString(joystickSizePrefKey, joystickSize.ToString());
                JoystickSizeChanged?.Invoke();
            }
        }
    }

    private const string textureFilteringPrefKey = "textureFiltering";
    private const string textureFilteringDefaultValue = "Point";
    private static FilterMode textureFiltering;
    public static Action TextureFilteringChanged;

    public static FilterMode TextureFiltering
    {
        get => textureFiltering;
        set
        {
            if (textureFiltering != value)
            {
                textureFiltering = value;
                PlayerPrefs.SetString(textureFilteringPrefKey, textureFiltering.ToString());
                TextureFilteringChanged?.Invoke();
            }
        }
    }

    public static void Initialize()
    {
        CustomScaleSize = (ScaleSizes) Enum.Parse(typeof(ScaleSizes), PlayerPrefs.GetString(customScaleSizePrefKey, customScaleSizeDefaultValue));
        TargetFrameRate = (TargetFrameRates) PlayerPrefs.GetInt(targetFrameRatePrefKey, targetFrameRateDefaultValue);
        JoystickSize = (JoystickSizes) Enum.Parse(typeof(JoystickSizes), PlayerPrefs.GetString(joystickSizePrefKey, joystickSizeDefaultValue));
        TextureFiltering = (FilterMode) Enum.Parse(typeof(FilterMode), PlayerPrefs.GetString(textureFilteringPrefKey, textureFilteringDefaultValue));
        
        customJoystickPositionAndSize.x = PlayerPrefs.GetFloat(customJoystickPositionAndSizePrefKey + "X", -1f);
        customJoystickPositionAndSize.y = PlayerPrefs.GetFloat(customJoystickPositionAndSizePrefKey + "Y", -1f);
        customJoystickPositionAndSize.z = PlayerPrefs.GetFloat(customJoystickPositionAndSizePrefKey + "Z", -1f);
    }
}