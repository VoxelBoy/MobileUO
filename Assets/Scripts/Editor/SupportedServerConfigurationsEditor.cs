using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SupportedServerConfigurations))]
public class SupportedServerConfigurationsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Write out to JSON"))
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject((target as SupportedServerConfigurations).ServerConfigurations);
            var path = EditorUtility.SaveFilePanel("Save Supported Server Configurations", Application.dataPath, "supportedServerConfigurations", "json");
            if (string.IsNullOrEmpty(path) == false)
            {
                File.WriteAllText(path, json);
            }
        }
    }
}