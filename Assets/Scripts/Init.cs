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
    
    private void Awake()
    {
        ConsoleRedirect.Redirect();
        Application.targetFrameRate = 60;
        
        UserPreferences.Initialize();

        StateManager.AddState(new BootState());
        StateManager.AddState(new ServerConfigurationState(serverConfigurationUiParent));
        StateManager.AddState(new DownloadState(downloadPresenter, forceDownloadsInEditor));
        StateManager.AddState(new GameState(unityMain, errorPresenter));
        
        ServerConfigurationModel.Initialize();
        
        StateManager.GoToState<BootState>();
    }
}
