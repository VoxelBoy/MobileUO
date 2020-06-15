using UnityEngine;

public class GameState : IState
{
    private readonly ClientRunner clientRunner;
    private readonly ErrorPresenter errorPresenter;
    private readonly Canvas inGameDebugConsoleCanvas;

    public GameState(ClientRunner clientRunner, ErrorPresenter errorPresenter, Canvas inGameDebugConsoleCanvas)
    {
        this.clientRunner = clientRunner;
        this.errorPresenter = errorPresenter;
        this.inGameDebugConsoleCanvas = inGameDebugConsoleCanvas;
    }
    public void Enter()
    {
        errorPresenter.BackButtonClicked += GoBackToServerConfigurationState;
        clientRunner.OnExiting += GoBackToServerConfigurationState;
        clientRunner.OnError += OnError;
        
        clientRunner.enabled = true;
        clientRunner.StartGame(ServerConfigurationModel.ActiveConfiguration);
    }

    private void GoBackToServerConfigurationState()
    {
        inGameDebugConsoleCanvas.enabled = false;
        StateManager.GoToState<ServerConfigurationState>();
    }

    private void OnError(string error)
    {
        clientRunner.enabled = false;
        errorPresenter.gameObject.SetActive(true);
        errorPresenter.SetErrorText(error);
        inGameDebugConsoleCanvas.enabled = true;
    }

    public void Exit()
    {
        clientRunner.enabled = false;
        errorPresenter.gameObject.SetActive(false);
        
        errorPresenter.BackButtonClicked -= GoBackToServerConfigurationState;
        clientRunner.OnExiting -= GoBackToServerConfigurationState;
        clientRunner.OnError -= OnError;
    }
}