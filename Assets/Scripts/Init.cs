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
    }

    private void OnShowDebugConsoleChanged(int currentValue)
    {
        inGameDebugConsoleCanvas.enabled = currentValue == (int) PreferenceEnums.ShowDebugConsole.On;
    }
}
