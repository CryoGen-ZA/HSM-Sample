using Enemies;
using HSM.Core;

namespace HSM.EnemyStates
{
    public class EnemyPatrolBaseState : BaseState
    {
        private readonly EnemyController _ctx;
        private int currentPatrolIndex;

        public EnemyPatrolBaseState(EnemyController ctx)
        {
            _ctx = ctx;
        }

        protected override void OnEnter()
        {
            _ctx._agent.SetDestination(_ctx.patrolPoints[0].position);
        }

        // LateUpdate keeps the previous "update sub state, then patrol" order.
        protected override void OnLateUpdate()
        {
            if (_ctx._agent.remainingDistance <= _ctx._agent.stoppingDistance)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % _ctx.patrolPoints.Length;
                _ctx._agent.SetDestination(_ctx.patrolPoints[currentPatrolIndex].position);
            }
        }
    }
}
