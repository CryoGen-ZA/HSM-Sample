using System.Collections.Generic;
using HSM.PlayerStates;

namespace HSM.Core
{
    public class StateGenerator
    {
        private BaseState _groundedState;
        private BaseState _jumpState;
        private FallingState _fallState;
        private RunState _runState;
        private IdleState _idleState;
        
        private Dictionary<string, BaseState> _states = new();

        public BaseState GetState(string key)
        {
            return _states.GetValueOrDefault(key);
        }
        public void AddState(string key, BaseState state)
        {
            _states[key] = state;
        }
    }
}