using System;
using UnityEngine;

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
    private Canvas inGameDebugConsoleCanvas;

    [SerializeField]
    private SupportedServerConfigurations supportedServerConfigurations;

    public static string ExternalStoragePath { get; private set; }

    private void Awake()
    {
        ConsoleRedirect.Redirect();
        
        UserPreferences.Initialize();
        UserPreferences.ShowDebugConsole.ValueChanged += OnShowDebugConsoleChanged;
        OnShowDebugConsoleChanged(UserPreferences.ShowDebugConsole.CurrentValue);

        StateManager.AddState(new BootState());
        StateManager.AddState(new ServerConfigurationState(serverConfigurationUiParent));
        StateManager.AddState(new DownloadState(downloadPresenter, inGameDebugConsoleCanvas));
        StateManager.AddState(new GameState(clientRunner, errorPresenter, inGameDebugConsoleCanvas));

        Input.simulateMouseWithTouches = false;
        
        ServerConfigurationModel.Initialize(supportedServerConfigurations);
        
        StateManager.GoToState<BootState>();
        
        ExternalStoragePath = GetAndroidExternalFilesDir();
    }

    private void OnShowDebugConsoleChanged(int currentValue)
    {
        inGameDebugConsoleCanvas.enabled = currentValue == (int) PreferenceEnums.ShowDebugConsole.On;
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
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
        #endif
        return null;
    }
}
