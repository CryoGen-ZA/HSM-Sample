using System.Text;

namespace HSM.Core
{
    public abstract class BaseState
    {
        protected readonly StateGenerator Generator;

        private BaseState _subState;
        private BaseState _superState;

        protected BaseState(StateGenerator generator)
        {
            Generator = generator;
        }

        public abstract void EnterState();
        public abstract void ExitState();
        public abstract void UpdateState();
        public abstract bool CheckStateSwitch(out string newState);

        protected void SetSubState(BaseState newSubState)
        {
            _subState?.ExitState();
            _subState = newSubState;
            _subState.SetSuperState(_superState);
            _subState.EnterState();
        }

        private void SetSuperState(BaseState newSuperState)
        {
            _superState = newSuperState;
        }

        protected void UpdateSubState()
        {
            if (_subState == null) return;
            
            CheckSubStateSwitch();
            _subState.UpdateState();
        }

        private void CheckSubStateSwitch()
        {
            if (_subState.CheckStateSwitch(out var newSubState))
            {
                SetSubState(Generator.GetState(newSubState));
            }
        }

        public void BuildStatesNames(StringBuilder stringBuilder)
        {
            if (_subState != null)
                _subState.BuildStatesNames(stringBuilder);
            stringBuilder.Insert(0, _superState == null ? $"Base State: {GetType().Name} \n" : $"Sub State: {GetType().Name} \n");
        }
    }
}
