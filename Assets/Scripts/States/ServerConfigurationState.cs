using States;
using UnityEngine;

public class ServerConfigurationState : IState
{
    private GameObject serverConfigurationUiParent;
    public ServerConfigurationState(GameObject serverConfigurationUiParent)
    {
        this.serverConfigurationUiParent = serverConfigurationUiParent;
    }
    
    public void Enter()
    {
        serverConfigurationUiParent.SetActive(true);
        ServerConfigurationModel.ActiveConfigurationChanged += OnActiveServerConfigurationChanged;
    }

    public void Exit()
    {
        serverConfigurationUiParent.SetActive(false);
        ServerConfigurationModel.ActiveConfigurationChanged -= OnActiveServerConfigurationChanged;
    }
    
    private void OnActiveServerConfigurationChanged()
    {
        if (ServerConfigurationModel.ActiveConfiguration != null)
        {
            StateManager.GoToState<DownloadState>();
        }
    }
}