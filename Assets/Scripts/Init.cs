using UnityEngine;

public class Init : MonoBehaviour
{
    [SerializeField]
    private GameObject serverConfigurationUiParent;
    
    [SerializeField]
    private DownloadPresenter downloadPresenter;
    
    [SerializeField]
    private UnityMain unityMain;
    
    private void Awake()
    {
        ConsoleRedirect.Redirect();
        Application.targetFrameRate = 60;
        
        StateManager.AddState(new BootState());
        StateManager.AddState(new ServerConfigurationState(serverConfigurationUiParent));
        StateManager.AddState(new DownloadState(downloadPresenter));
        StateManager.AddState(new GameState(unityMain));
        
        ServerConfigurationModel.Initialize();
        
        StateManager.GoToState<BootState>();
    }
}
