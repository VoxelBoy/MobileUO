using System;
using PreferenceEnums;
using UnityEngine;

public static class UserPreferences
{
    public class IntPreference
    {
        private readonly string PrefKey;
        private int currentValue;
        public Action<int> ValueChanged;

        public int CurrentValue
        {
            get => currentValue;
            set
            {
                if (currentValue != value)
                {
                    currentValue = value;
                    PlayerPrefs.SetInt(PrefKey, currentValue);
                    PlayerPrefs.Save();
                    ValueChanged?.Invoke(currentValue);
                }
            }
        }

        public IntPreference(string prefKey, int defaultValue)
        {
            PrefKey = prefKey;
            currentValue = PlayerPrefs.GetInt(prefKey, defaultValue);
        }
    }
    
    public class Vector3Preference
    {
        private readonly string PrefKey;
        private Vector3 currentValue;
        public Action<Vector3> ValueChanged;

        public Vector3 CurrentValue
        {
            get => currentValue;
            set
            {
                if (currentValue != value)
                {
                    currentValue = value;
                    PlayerPrefs.SetFloat(PrefKey + "X", currentValue.x);
                    PlayerPrefs.SetFloat(PrefKey + "Y", currentValue.y);
                    PlayerPrefs.SetFloat(PrefKey + "Z", currentValue.z);
                    PlayerPrefs.Save();
                    ValueChanged?.Invoke(currentValue);
                }
            }
        }

        public Vector3Preference(string prefKey, Vector3 defaultValue)
        {
            PrefKey = prefKey;
            currentValue.x = PlayerPrefs.GetFloat(prefKey + "X", defaultValue.x);
            currentValue.y = PlayerPrefs.GetFloat(prefKey + "Y", defaultValue.y);
            currentValue.z = PlayerPrefs.GetFloat(prefKey + "Z", defaultValue.z);
        }
    }
    
    public static IntPreference ShowCloseButtons;
    public static IntPreference UseMouseOnMobile;
    public static IntPreference ScaleSize;
    public static IntPreference TextureFiltering;
    public static IntPreference TargetFrameRate;
    public static IntPreference JoystickSize;
    public static IntPreference JoystickOpacity;
    public static Vector3Preference CustomJoystickPositionAndSize;
    public static IntPreference JoystickDeadZone;
    public static IntPreference JoystickRunThreshold;
    public static IntPreference ContainerItemSelection;
    public static IntPreference ForceUseXbr;
    public static IntPreference VisualizeFingerInput;

    public static void Initialize()
    {
        ShowCloseButtons = new IntPreference(nameof(ShowCloseButtons), (int) PreferenceEnums.ShowCloseButtons.Off);
        UseMouseOnMobile = new IntPreference(nameof(UseMouseOnMobile), (int) PreferenceEnums.UseMouseOnMobile.Off);
        ScaleSize = new IntPreference(nameof(ScaleSize), (int) ScaleSizes.Default);
        TextureFiltering = new IntPreference(nameof(TextureFiltering), (int) TextureFilterMode.Sharp);
        TargetFrameRate = new IntPreference(nameof(TargetFrameRate), (int) TargetFrameRates.Sixty);
        JoystickSize = new IntPreference(nameof(JoystickSize), (int) JoystickSizes.Normal);
        JoystickOpacity = new IntPreference(nameof(JoystickOpacity), (int) PreferenceEnums.JoystickOpacity.Normal);
        CustomJoystickPositionAndSize = new Vector3Preference("customJoystickSizeAndPosition", new Vector3(-1,-1,-1));
        JoystickDeadZone = new IntPreference(nameof(JoystickDeadZone), (int) PreferenceEnums.JoystickDeadZone.Low);
        JoystickRunThreshold = new IntPreference(nameof(JoystickRunThreshold), (int) PreferenceEnums.JoystickRunThreshold.Low);
        ContainerItemSelection = new IntPreference(nameof(ContainerItemSelection), (int) PreferenceEnums.ContainerItemSelection.Coarse);
        ForceUseXbr = new IntPreference(nameof(ForceUseXbr), (int) PreferenceEnums.ForceUseXbr.Off);
        VisualizeFingerInput = new IntPreference(nameof(VisualizeFingerInput), (int) PreferenceEnums.VisualizeFingerInput.Off);
    }
}