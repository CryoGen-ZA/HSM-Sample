using System;

namespace HSM.Core
{
    /// <summary>A guarded edge to <see cref="Target"/>. Owned by its source state (or the machine, for "any" transitions).</summary>
    public sealed class Transition
    {
        public BaseState Target { get; }
        public Func<bool> Condition { get; }

        /// <summary>If the target is already active, exit and re-enter it instead of ignoring the transition.</summary>
        public bool AllowReenter { get; }

        public Transition(BaseState target, Func<bool> condition, bool allowReenter = false)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            AllowReenter = allowReenter;
        }
    }
}
