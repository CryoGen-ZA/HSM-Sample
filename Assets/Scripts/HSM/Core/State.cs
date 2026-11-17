using System;
using System.Collections.Generic;

namespace HSM.Core
{
    /// <summary>
    /// A node in the state hierarchy. The tree (Parent / DefaultChild) is fixed at registration time
    /// by <see cref="StateMachine.AddState{T}"/>; the machine owns which states are active and drives
    /// the lifecycle. Override only the hooks you need.
    /// </summary>
    public abstract class BaseState
    {
        public StateMachine Machine { get; private set; }
        public BaseState Parent { get; private set; }
        public BaseState DefaultChild { get; private set; }
        public int Depth { get; private set; }
        public bool IsActive { get; private set; }
        public bool HasChildren { get; private set; }
        public string Name => GetType().Name;

        internal readonly List<Transition> Transitions = new();

        /// <summary>Called when the state becomes active, before its default child is entered.</summary>
        protected virtual void OnEnter() { }

        /// <summary>Called when the state is left, after its active child has been exited.</summary>
        protected virtual void OnExit() { }

        /// <summary>Called every tick, parents before children.</summary>
        protected virtual void OnUpdate() { }

        /// <summary>Called every tick after all OnUpdate calls, children before parents.</summary>
        protected virtual void OnLateUpdate() { }

        /// <summary>Imperatively request a transition. It is applied at the start of the next machine tick.</summary>
        protected void RequestTransition<T>(bool allowReenter = false) where T : BaseState
        {
            Machine.RequestTransition<T>(allowReenter);
        }

        internal void Attach(StateMachine machine, BaseState parent)
        {
            if (Machine != null)
                throw new InvalidOperationException($"State '{Name}' is already registered with a state machine.");

            Machine = machine;
            Parent = parent;
            Depth = parent == null ? 0 : parent.Depth + 1;
            if (parent != null) parent.HasChildren = true;
        }

        internal void SetDefaultChild(BaseState child)
        {
            if (DefaultChild != null)
                throw new InvalidOperationException(
                    $"State '{Name}' already has default child '{DefaultChild.Name}', cannot also set '{child.Name}'.");
            DefaultChild = child;
        }

        internal void Enter()
        {
            IsActive = true;
            OnEnter();
        }

        internal void Exit()
        {
            OnExit();
            IsActive = false;
        }

        internal void Update() => OnUpdate();
        internal void LateUpdate() => OnLateUpdate();
    }
}
