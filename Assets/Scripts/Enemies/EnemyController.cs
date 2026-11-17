using HSM.Core;
using HSM.EnemyStates;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : HsmBehaviour
    {
        public NavMeshAgent _agent { private set; get; }
        public Transform[] patrolPoints;

        protected override void Awake()
        {
            base.Awake();
            _agent = GetComponent<NavMeshAgent>();
            Machine.Start<EnemyPatrolBaseState>();
        }

        protected override void RegisterStates()
        {
            var patrol = AddState(new EnemyPatrolBaseState(this));
            AddState(new EnemyIdle(this), patrol, isDefault: true);
            AddState(new EnemyPatrolWalking(this), patrol);

            AddTransition<EnemyIdle, EnemyPatrolWalking>(() => _agent.velocity.magnitude > 0.1f);
            AddTransition<EnemyPatrolWalking, EnemyIdle>(() => _agent.velocity.magnitude <= 0.1f);
        }

        private void Update()
        {
            Machine.Update();
        }
    }
}
