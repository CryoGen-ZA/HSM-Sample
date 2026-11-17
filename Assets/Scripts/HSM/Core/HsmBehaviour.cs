using System;
using UnityEngine;

namespace HSM.Core
{
    /// <summary>MonoBehaviour host for a <see cref="StateMachine"/>. Subclasses register states and transitions, then start it.</summary>
    public abstract class HsmBehaviour : MonoBehaviour
    {
        protected StateMachine Machine { get; private set; }
        public Animator Animator { private set; get; }

        public string GetStatesDebugText => Machine.GetActivePathDescription();

        /// <summary>Register states with <see cref="AddState{T}"/> and transitions with <see cref="AddTransition{TFrom,TTo}"/>.</summary>
        protected abstract void RegisterStates();

        protected virtual void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
            Machine = new StateMachine();
            RegisterStates();
        }

        protected T AddState<T>(T state, BaseState parent = null, bool isDefault = false) where T : BaseState
            => Machine.AddState(state, parent, isDefault);

        protected void AddTransition<TFrom, TTo>(Func<bool> condition, bool allowReenter = false)
            where TFrom : BaseState where TTo : BaseState
            => Machine.AddTransition<TFrom, TTo>(condition, allowReenter);

        protected void AddAnyTransition<TTo>(Func<bool> condition, bool allowReenter = false) where TTo : BaseState
            => Machine.AddAnyTransition<TTo>(condition, allowReenter);
    }
}
