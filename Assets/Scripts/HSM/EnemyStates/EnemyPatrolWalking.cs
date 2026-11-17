using Enemies;
using HSM.Core;
using UnityEngine;

namespace HSM.EnemyStates
{
    public class EnemyPatrolWalking : BaseState
    {
        private static readonly int MovementVelocity = Animator.StringToHash("MovementVelocity");
        private readonly EnemyController _ctx;

        public EnemyPatrolWalking(EnemyController ctx)
        {
            _ctx = ctx;
        }

        protected override void OnUpdate()
        {
            _ctx.Animator.SetFloat(MovementVelocity, _ctx._agent.velocity.magnitude);
        }
    }
}
