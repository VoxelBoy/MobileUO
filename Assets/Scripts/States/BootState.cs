using States;

public class BootState : IState
{
    public void Enter()
    {
        if (ServerConfigurationModel.DefaultConfiguration != null)
        {
            ServerConfigurationModel.ActiveConfiguration = ServerConfigurationModel.DefaultConfiguration;
            StateManager.GoToState<DownloadState>();
        }
        else
        {
            StateManager.GoToState<ServerConfigurationState>();
        }
    }

    public void Exit()
    {
        
    }
}