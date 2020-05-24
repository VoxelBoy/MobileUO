using UnityEngine;

public class GameState : IState
{
    private readonly UnityMain unityMain;
    private readonly ErrorPresenter errorPresenter;
    private readonly Canvas inGameDebugConsoleCanvas;

    public GameState(UnityMain unityMain, ErrorPresenter errorPresenter, Canvas inGameDebugConsoleCanvas)
    {
        this.unityMain = unityMain;
        this.errorPresenter = errorPresenter;
        this.inGameDebugConsoleCanvas = inGameDebugConsoleCanvas;
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
        inGameDebugConsoleCanvas.enabled = false;
        StateManager.GoToState<ServerConfigurationState>();
    }

    private void OnError(string error)
    {
        unityMain.enabled = false;
        errorPresenter.gameObject.SetActive(true);
        errorPresenter.SetErrorText(error);
        inGameDebugConsoleCanvas.enabled = true;
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