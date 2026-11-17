using System;
using System.Collections.Generic;
using System.Text;

namespace HSM.Core
{
    /// <summary>
    /// Hierarchical state machine.
    /// <para>States are registered into a static tree and looked up by type. The machine keeps the
    /// active path (root → leaf) and performs transitions via the lowest common ancestor: exit from
    /// the active leaf up to (not including) the LCA, then enter down to the target and on through
    /// each DefaultChild until a leaf is reached.</para>
    /// <para>Per tick: any-transitions are checked first, then each active state's transitions from
    /// root to leaf; the first match wins and at most one transition happens per tick. Then OnUpdate
    /// runs root → leaf and OnLateUpdate runs leaf → root.</para>
    /// </summary>
    public sealed class StateMachine
    {
        private const int MaxChainedTransitions = 8;

        private readonly Dictionary<Type, BaseState> _states = new();
        private readonly List<BaseState> _activePath = new();
        private readonly List<Transition> _anyTransitions = new();
        private readonly List<BaseState> _targetPathBuffer = new();
        private readonly StringBuilder _debugBuilder = new();

        private Transition _pending;
        private bool _isTransitioning;

        /// <summary>Raised after each transition with the previous and new active leaf (previous is null on start).</summary>
        public event Action<BaseState, BaseState> OnTransition;

        public IReadOnlyList<BaseState> ActivePath => _activePath;
        public BaseState ActiveLeaf => _activePath.Count > 0 ? _activePath[_activePath.Count - 1] : null;
        public bool IsStarted { get; private set; }

        #region Registration

        public T AddState<T>(T state, BaseState parent = null, bool isDefault = false) where T : BaseState
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (IsStarted) throw new InvalidOperationException("Cannot add states after the machine has started.");

            var type = state.GetType();
            if (_states.ContainsKey(type))
                throw new InvalidOperationException($"A state of type '{type.Name}' is already registered.");
            if (parent != null && parent.Machine != this)
                throw new InvalidOperationException(
                    $"Parent '{parent.Name}' of '{type.Name}' must be registered with this machine first.");
            if (isDefault && parent == null)
                throw new InvalidOperationException($"Root state '{type.Name}' cannot be a default child.");

            state.Attach(this, parent);
            if (isDefault) parent.SetDefaultChild(state);
            _states[type] = state;
            return state;
        }

        public void AddTransition<TFrom, TTo>(Func<bool> condition, bool allowReenter = false)
            where TFrom : BaseState where TTo : BaseState
        {
            GetState<TFrom>().Transitions.Add(new Transition(GetState<TTo>(), condition, allowReenter));
        }

        /// <summary>A transition evaluated from every state. Ignored while the target is already active unless allowReenter.</summary>
        public void AddAnyTransition<TTo>(Func<bool> condition, bool allowReenter = false) where TTo : BaseState
        {
            _anyTransitions.Add(new Transition(GetState<TTo>(), condition, allowReenter));
        }

        public T GetState<T>() where T : BaseState => (T)GetState(typeof(T));

        public BaseState GetState(Type type)
        {
            if (_states.TryGetValue(type, out var state)) return state;
            throw new KeyNotFoundException($"State '{type.Name}' has not been registered with this state machine.");
        }

        #endregion

        #region Running

        public void Start<T>() where T : BaseState
        {
            if (IsStarted) throw new InvalidOperationException("State machine has already been started.");
            Validate();
            IsStarted = true;
            _pending = new Transition(GetState<T>(), static () => true);
            ProcessPending();
        }

        public void Update()
        {
            if (!IsStarted) throw new InvalidOperationException("Call Start<T>() before updating the state machine.");

            // A transition requested imperatively last tick takes this tick's transition slot.
            if (_pending == null) EvaluateTransitions();
            ProcessPending();

            for (var i = 0; i < _activePath.Count; i++)
                _activePath[i].Update();
            for (var i = _activePath.Count - 1; i >= 0; i--)
                _activePath[i].LateUpdate();
        }

        /// <summary>
        /// Queue a transition. During a transition (from OnEnter/OnExit) it runs as soon as the current one
        /// finishes; otherwise it runs at the start of the next Update. If one is already pending, the first wins.
        /// </summary>
        public void RequestTransition<T>(bool allowReenter = false) where T : BaseState
        {
            // While transitioning, the ProcessPending loop already running picks this up.
            _pending ??= new Transition(GetState<T>(), static () => true, allowReenter);
        }

        #endregion

        #region Transition internals

        private void EvaluateTransitions()
        {
            foreach (var transition in _anyTransitions)
            {
                if (IsNoOp(transition)) continue;
                if (!transition.Condition()) continue;
                _pending = transition;
                return;
            }

            foreach (var state in _activePath)
            {
                foreach (var transition in state.Transitions)
                {
                    if (IsNoOp(transition)) continue;
                    if (!transition.Condition()) continue;
                    _pending = transition;
                    return;
                }
            }
        }

        private static bool IsNoOp(Transition transition) => transition.Target.IsActive && !transition.AllowReenter;

        private void ProcessPending()
        {
            if (_isTransitioning) return;

            var chained = 0;
            while (_pending != null)
            {
                if (++chained > MaxChainedTransitions)
                {
                    var target = _pending.Target.Name;
                    _pending = null;
                    throw new InvalidOperationException(
                        $"More than {MaxChainedTransitions} chained transitions (last target '{target}'). Possible transition loop.");
                }

                var transition = _pending;
                _pending = null;

                _isTransitioning = true;
                try
                {
                    PerformTransition(transition.Target, transition.AllowReenter);
                }
                finally
                {
                    _isTransitioning = false;
                }
            }
        }

        private void PerformTransition(BaseState target, bool allowReenter)
        {
            if (target.IsActive && !allowReenter) return;

            var previousLeaf = ActiveLeaf;

            // Build root → target path.
            _targetPathBuffer.Clear();
            for (var s = target; s != null; s = s.Parent)
                _targetPathBuffer.Add(s);
            _targetPathBuffer.Reverse();

            // Length of the shared prefix = index of the first state that has to be entered.
            var lca = 0;
            while (lca < _activePath.Count && lca < _targetPathBuffer.Count &&
                   _activePath[lca] == _targetPathBuffer[lca])
                lca++;

            // Re-entering an already active target: exit and re-enter it (and everything below it).
            if (lca == _targetPathBuffer.Count)
                lca = target.Depth;

            // Exit innermost first, up to (not including) the LCA.
            for (var i = _activePath.Count - 1; i >= lca; i--)
            {
                var exiting = _activePath[i];
                _activePath.RemoveAt(i);
                exiting.Exit();
            }

            // Enter down to the target, then through default children to a leaf.
            for (var i = lca; i < _targetPathBuffer.Count; i++)
                EnterAndPush(_targetPathBuffer[i]);
            for (var s = target.DefaultChild; s != null; s = s.DefaultChild)
                EnterAndPush(s);

            OnTransition?.Invoke(previousLeaf, ActiveLeaf);
        }

        private void EnterAndPush(BaseState state)
        {
            _activePath.Add(state);
            state.Enter();
        }

        private void Validate()
        {
            foreach (var state in _states.Values)
            {
                if (state.HasChildren && state.DefaultChild == null)
                    throw new InvalidOperationException(
                        $"State '{state.Name}' has child states but no default child. Register one with isDefault: true.");
            }
        }

        #endregion

        public string GetActivePathDescription()
        {
            _debugBuilder.Clear();
            foreach (var state in _activePath)
            {
                _debugBuilder.Append(state.Parent == null ? "Base State: " : "Sub State: ")
                    .Append(state.Name)
                    .Append('\n');
            }
            return _debugBuilder.ToString();
        }
    }
}
