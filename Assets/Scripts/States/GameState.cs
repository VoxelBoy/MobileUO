using System.IO;
using System.Linq;
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

        var config = ServerConfigurationModel.ActiveConfiguration;
        
        //Check that some of the essential UO files exist
        if (Application.isMobilePlatform || string.IsNullOrEmpty(config.ClientPathForUnityEditor))
        {
            var configPath = config.GetPathToSaveFiles();
            var configurationDirectory = new DirectoryInfo(configPath);
            var files = configurationDirectory.GetFiles().Select(x => x.Name).ToList();
            var hasAnimationFiles = UtilityMethods.EssentialUoFilesExist(files);
            if (hasAnimationFiles == false)
            {
                var error = $"Server configuration directory does not contain UO files such as anim.mul or animationFrame1.uop. Make sure that the UO files have been downloaded or transferred properly.\nPath: {configPath}";
                OnError(error);
                return;
            }
        }

        clientRunner.enabled = true;
        clientRunner.StartGame(config);
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