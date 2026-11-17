using Enemies;
using HSM.Core;
using UnityEngine;

namespace HSM.EnemyStates
{
    public class EnemyIdle : BaseState
    {
        private readonly EnemyController _ctx;
        private static readonly int MovementVelocity = Animator.StringToHash("MovementVelocity");

        public EnemyIdle(EnemyController ctx)
        {
            _ctx = ctx;
        }

        protected override void OnEnter()
        {
            _ctx.Animator.SetFloat(MovementVelocity, 0);
        }
    }
}
