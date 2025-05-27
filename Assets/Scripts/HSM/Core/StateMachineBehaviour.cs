using UnityEngine;

namespace HSM.Core
{
    public abstract class StateMachineBehaviour : MonoBehaviour
    {
        protected StateMachine _stateMachine;
        public Animator Animator {private set; get;}
        
        protected abstract void RegisterStates();

        protected virtual void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
            _stateMachine = new StateMachine();
            _stateMachine.Initialize();
            RegisterStates();
        }

        protected void RegisterState<T>(string name, T state) where T : BaseState
        {
            _stateMachine.Generator.AddState(name, state);
        }
        
        protected void RegisterState<T>(T state) where T : BaseState
        {
            _stateMachine.Generator.AddState(typeof(T).Name, state);
        }
    }
}