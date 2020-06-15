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
    
    private void Awake()
    {
        ConsoleRedirect.Redirect();
        
        UserPreferences.Initialize();

        StateManager.AddState(new BootState());
        StateManager.AddState(new ServerConfigurationState(serverConfigurationUiParent));
        StateManager.AddState(new DownloadState(downloadPresenter, forceDownloadsInEditor, inGameDebugConsoleCanvas));
        StateManager.AddState(new GameState(clientRunner, errorPresenter, inGameDebugConsoleCanvas));

        inGameDebugConsoleCanvas.enabled = false;
        
        ServerConfigurationModel.Initialize();
        
        StateManager.GoToState<BootState>();
    }
}
