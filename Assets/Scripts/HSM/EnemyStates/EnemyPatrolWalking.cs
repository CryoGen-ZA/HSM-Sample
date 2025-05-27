using Enemies;
using HSM.Core;
using UnityEngine;

namespace HSM.EnemyStates
{
    public class EnemyPatrolWalking : BaseState
    {
        private static readonly int MovementVelocity = Animator.StringToHash("MovementVelocity");
        private EnemyController _ctx;

        public EnemyPatrolWalking(StateGenerator generator, EnemyController ctx) : base(generator)
        {
            _ctx = ctx;
        }

        public override void EnterState()
        {
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        { 
            _ctx.Animator.SetFloat(MovementVelocity, _ctx._agent.velocity.magnitude);
        }

        public override bool CheckStateSwitch(out string newState)
        {
            if (_ctx._agent.velocity.magnitude <= 0.1f)
            {
                newState = nameof(EnemyIdle);
                return true;
            }

            newState = null;
            return false;
        }
    }
}