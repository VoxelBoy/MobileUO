using UnityEngine;

public class Init : MonoBehaviour
{
    [SerializeField]
    private GameObject serverConfigurationUiParent;
    
    [SerializeField]
    private DownloadPresenter downloadPresenter;

    [SerializeField]
    private bool forceDownloadsInEditor;
    
    [SerializeField]
    private UnityMain unityMain;

    [SerializeField]
    private ErrorPresenter errorPresenter;

    [SerializeField]
    private Canvas inGameDebugConsoleCanvas;
    
    private void Awake()
    {
        ConsoleRedirect.Redirect();
        
        UserPreferences.TargetFrameRateChanged += OnTargetFrameRateChanged;
        UserPreferences.Initialize();

        StateManager.AddState(new BootState());
        StateManager.AddState(new ServerConfigurationState(serverConfigurationUiParent));
        StateManager.AddState(new DownloadState(downloadPresenter, forceDownloadsInEditor, inGameDebugConsoleCanvas));
        StateManager.AddState(new GameState(unityMain, errorPresenter, inGameDebugConsoleCanvas));

        inGameDebugConsoleCanvas.enabled = false;
        
        ServerConfigurationModel.Initialize();
        
        StateManager.GoToState<BootState>();
    }

    private static void OnTargetFrameRateChanged()
    {
        Application.targetFrameRate = (int) UserPreferences.TargetFrameRate;
    }
}
