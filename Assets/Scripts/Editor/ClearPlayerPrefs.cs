using UnityEditor;
using UnityEngine;

public static class ClearPlayerPrefs
{
    [MenuItem("Tools/Clear Player Prefs")]
    public static void Clear()
    {
        PlayerPrefs.DeleteAll();
    }
}
