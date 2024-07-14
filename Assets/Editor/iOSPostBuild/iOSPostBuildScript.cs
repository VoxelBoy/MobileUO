#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class iOSPostProcess
{
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            PlistElementDict rootDict = plist.root;

            // Add your custom key-value pairs here
            rootDict.SetBoolean("UISupportsDocumentBrowser", true);
            rootDict.SetBoolean("UIFileSharingEnabled", true);

            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
#endif