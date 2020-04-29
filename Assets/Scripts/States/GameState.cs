public class GameState : IState
{
    private UnityMain unityMain;
    private ErrorPresenter errorPresenter;

    public GameState(UnityMain unityMain, ErrorPresenter errorPresenter)
    {
        this.unityMain = unityMain;
        this.errorPresenter = errorPresenter;
    }
    public void Enter()
    {
        errorPresenter.BackButtonClicked += GoBackToServerConfigurationState;
        unityMain.OnExiting += GoBackToServerConfigurationState;
        unityMain.OnError += OnError;
        
        unityMain.enabled = true;
        unityMain.StartGame(ServerConfigurationModel.ActiveConfiguration);
    }

    private void GoBackToServerConfigurationState()
    {
        StateManager.GoToState<ServerConfigurationState>();
    }

    private void OnError(string error)
    {
        unityMain.enabled = false;
        errorPresenter.gameObject.SetActive(true);
        errorPresenter.SetErrorText(error);
    }

    public void Exit()
    {
        unityMain.enabled = false;
        errorPresenter.gameObject.SetActive(false);
        
        errorPresenter.BackButtonClicked -= GoBackToServerConfigurationState;
        unityMain.OnExiting -= GoBackToServerConfigurationState;
        unityMain.OnError -= OnError;
    }
}