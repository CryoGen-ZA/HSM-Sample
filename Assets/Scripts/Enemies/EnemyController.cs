using HSM.EnemyStates;
using UnityEngine;
using UnityEngine.AI;
using StateMachineBehaviour = HSM.Core.StateMachineBehaviour;

namespace Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : StateMachineBehaviour
    {
        public NavMeshAgent _agent { private set; get; }
        public Transform[] patrolPoints;

        protected override void Awake()
        {
            base.Awake();
            _agent = GetComponent<NavMeshAgent>();
            _stateMachine.SetState(_stateMachine.Generator.GetState(nameof(EnemyPatrolBaseState)));
        }
        protected override void RegisterStates()
        {
            RegisterState(new EnemyPatrolBaseState(_stateMachine.Generator, this));
            RegisterState(new EnemyPatrolWalking(_stateMachine.Generator, this));
            RegisterState(new EnemyIdle(_stateMachine.Generator, this));
        }
        private void Update()
        {
            _stateMachine.DoUpdate();
        }
    }
}