using System.Text;

namespace HSM.Core
{
    public class StateMachine
    {
        private BaseState[] _states;
        private StateGenerator _generator;
        private BaseState _currentBaseState;
        private StringBuilder _stateDebugBuilder;
        public StateGenerator Generator => _generator;

        public void Initialize()
        {
            _generator = new StateGenerator();
        }

        public void DoUpdate()
        {
            CheckStateSwitching();
            _currentBaseState.UpdateState();
        }

        private void CheckStateSwitching()
        {
            if (!_currentBaseState.CheckStateSwitch(out var newState)) return;
            SetState(Generator.GetState(newState));
        }

        public void SetState(BaseState newState)
        {
            _currentBaseState?.ExitState();
            _currentBaseState = newState;
            _currentBaseState.EnterState();
        }

        public string GetStateAndSubStateNames()
        {
            _stateDebugBuilder ??= new StringBuilder();
            _stateDebugBuilder.Clear();
            _currentBaseState.BuildStatesNames(_stateDebugBuilder);
            return _stateDebugBuilder.ToString();
        }
    }
}
