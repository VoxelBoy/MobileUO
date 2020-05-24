using System.Collections.Generic;
using System.Linq;

public class StateManager
{
    private static readonly List<IState> states = new List<IState>();
    private static IState currentState;
    
    public static void GoToState<T>() where T : IState
    {
        currentState?.Exit();
        var newState = states.FirstOrDefault(x => x is T);
        currentState = newState;
        currentState?.Enter();
    }

    public static void AddState<T>(T state) where T : IState
    {
        states.Add(state);
    }
}