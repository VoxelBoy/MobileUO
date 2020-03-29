using System.Collections.Generic;
using System.Linq;

namespace States
{
    public class StateManager
    {
        private static readonly List<IState> states = new List<IState>();
        private static IState currentState;
        
        public static void GoToState<T>() where T : IState
        {
            currentState?.Exit();
            currentState = states.FirstOrDefault(x => x is T);
            currentState?.Enter();
        }

        public static void AddState<T>(T state) where T : IState
        {
            states.Add(state);
        }
    }
}