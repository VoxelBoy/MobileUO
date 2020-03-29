public class GameState : IState
{
    private UnityMain unityMain;

    public GameState(UnityMain unityMain)
    {
        this.unityMain = unityMain;
    }
    public void Enter()
    {
        unityMain.StartGame(ServerConfigurationModel.ActiveConfiguration);
    }

    public void Exit()
    {
        
    }
}