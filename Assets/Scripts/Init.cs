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
    private ClientRunner clientRunner;

    [SerializeField]
    private ErrorPresenter errorPresenter;

    [SerializeField]
    private Canvas inGameDebugConsoleCanvas;

    [SerializeField]
    private bool hideInGameDebugConsoleOnAwake;

    [SerializeField]
    private SupportedServerConfigurations supportedServerConfigurations;

    private void Awake()
    {
        ConsoleRedirect.Redirect();
        
        UserPreferences.Initialize();

        StateManager.AddState(new BootState());
        StateManager.AddState(new ServerConfigurationState(serverConfigurationUiParent));
        StateManager.AddState(new DownloadState(downloadPresenter, forceDownloadsInEditor, inGameDebugConsoleCanvas));
        StateManager.AddState(new GameState(clientRunner, errorPresenter, inGameDebugConsoleCanvas));

        if (hideInGameDebugConsoleOnAwake)
        {
            inGameDebugConsoleCanvas.enabled = false;
        }

        Input.simulateMouseWithTouches = false;
        
        ServerConfigurationModel.Initialize(supportedServerConfigurations);
        
        StateManager.GoToState<BootState>();
    }
}
