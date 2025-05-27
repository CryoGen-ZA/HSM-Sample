using Enemies;
using HSM.Core;
using UnityEngine;

namespace HSM.EnemyStates
{
    public class EnemyIdle : BaseState
    {
        private readonly EnemyController _ctx;
        private static readonly int MovementVelocity = Animator.StringToHash("MovementVelocity");
        
        public EnemyIdle(StateGenerator generator, EnemyController ctx) : base(generator)
        {
            _ctx = ctx;
        }

        public override void EnterState()
        {
            _ctx.Animator.SetFloat(MovementVelocity, 0);
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
        }

        public override bool CheckStateSwitch(out string newState)
        {
            if (_ctx._agent.velocity.magnitude > 0.1f)
            {
                newState = nameof(EnemyPatrolWalking);
                return true;
            }

            newState = null;
            return false;
        }
    }
}