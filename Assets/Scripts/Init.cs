using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Init : MonoBehaviour
{
    [SerializeField]
    private GameObject serverConfigurationUiParent;
    
    [SerializeField]
    private DownloadPresenter downloadPresenter;

    [SerializeField]
    private ClientRunner clientRunner;

    [SerializeField]
    private ErrorPresenter errorPresenter;

    [SerializeField]
    private SupportedServerConfigurations supportedServerConfigurations;

    private const string supportedServerConfigurationsUrl = "https://www.dropbox.com/scl/fi/dvmthuazhp6rlmaklu1xj/supportedServerConfigurations.json?rlkey=lcczmc36r39uovp0w7sy9ssa5&dl=1";
    private const string downloadedSupportedServerConfigurationsPrefKey = "downloadedSupportedServerConfigurations";

    public static string ExternalStoragePath { get; private set; }

    private void Awake()
    {
        ConsoleRedirect.Redirect();
        
        UserPreferences.Initialize();

        StateManager.AddState(new BootState());
        StateManager.AddState(new ServerConfigurationState(serverConfigurationUiParent));
        StateManager.AddState(new DownloadState(downloadPresenter));
        StateManager.AddState(new GameState(clientRunner, errorPresenter));

        Input.simulateMouseWithTouches = false;

        var supportedServerConfigurationsList = supportedServerConfigurations.ServerConfigurations;
        var json = PlayerPrefs.GetString(downloadedSupportedServerConfigurationsPrefKey, string.Empty);
        if (string.IsNullOrEmpty(json) == false)
        {
            try
            {
                var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ServerConfiguration>>(json);
                if (list != null && list.Count > 0)
                {
                    supportedServerConfigurationsList = list;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        
        ServerConfigurationModel.Initialize(supportedServerConfigurationsList);
        StartCoroutine(FetchUpdatedSupportedServerConfigurations());
        
        StateManager.GoToState<BootState>();
        
        ExternalStoragePath = GetAndroidExternalFilesDir();
    }

    private IEnumerator FetchUpdatedSupportedServerConfigurations()
    {
        var request = new UnityWebRequest(supportedServerConfigurationsUrl);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        if (string.IsNullOrEmpty(request.downloadHandler.error) == false)
        {
            Debug.LogError(request.downloadHandler.error);
            yield break;
        }
        
        try
        {
            var json = request.downloadHandler.text;
            var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ServerConfiguration>>(json);
            if (list != null && list.Count > 0)
            {
                ServerConfigurationModel.SupportedServerConfigurations = list;
                PlayerPrefs.SetString(downloadedSupportedServerConfigurationsPrefKey, json);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private static string GetAndroidExternalFilesDir()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    // Get all available external file directories (emulated and sdCards)
                    AndroidJavaObject[] externalFilesDirectories =
                        context.Call<AndroidJavaObject[], string>("getExternalFilesDirs", null);
                    AndroidJavaObject emulated = null;
                    AndroidJavaObject sdCard = null;

                    for (int i = 0; i < externalFilesDirectories.Length; i++)
                    {
                        AndroidJavaObject directory = externalFilesDirectories[i];
                        using (AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment"))
                        {
                            // Check which one is the emulated and which the sdCard.
                            bool isRemovable = environment.CallStatic<bool>("isExternalStorageRemovable", directory);
                            bool isEmulated = environment.CallStatic<bool>("isExternalStorageEmulated", directory);
                            if (isEmulated)
                                emulated = directory;
                            else if (isRemovable)
                                sdCard = directory;
                        }
                    }

                    // Return the sdCard if available
                    return sdCard?.Call<string>("getAbsolutePath");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            return null;
        }
        #endif
        return null;
    }
}
