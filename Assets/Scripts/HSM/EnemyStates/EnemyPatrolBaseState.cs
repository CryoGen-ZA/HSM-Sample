using Enemies;
using HSM.Core;

namespace HSM.EnemyStates
{
    public class EnemyPatrolBaseState : BaseState
    {
        private readonly EnemyController _ctx;
        private int currentPatrolIndex;

        public EnemyPatrolBaseState(StateGenerator generator, EnemyController ctx) : base(generator)
        {
            _ctx = ctx;
        }

        public override void EnterState()
        {
            SetSubState(Generator.GetState(nameof(EnemyIdle)));
            _ctx._agent.SetDestination(_ctx.patrolPoints[0].position);
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
            UpdateSubState();
            if (_ctx._agent.remainingDistance <= _ctx._agent.stoppingDistance)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % _ctx.patrolPoints.Length;
                _ctx._agent.SetDestination(_ctx.patrolPoints[currentPatrolIndex].position);
            }
        }

        public override bool CheckStateSwitch(out string newState)
        {
            newState = "Undefined"; //We don't have other base states for AI at the moment
            return false;
        }
    }
}