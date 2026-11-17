using Character;
using HSM.Core;
using UnityEngine;

namespace HSM.PlayerStates
{
    public class GroundedBaseState : BaseState
    {
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private readonly PlayerCharacter _ctx;

        public GroundedBaseState(PlayerCharacter ctx)
        {
            _ctx = ctx;
        }

        protected override void OnEnter()
        {
            _ctx.Animator.SetBool(IsGroundedHash, true);
        }

        protected override void OnExit()
        {
            _ctx.Animator.SetBool(IsGroundedHash, false);
        }

        // LateUpdate so gravity is added after the child state (e.g. RunState) has set its velocity,
        // matching the previous "UpdateSubState() then add gravity" order.
        protected override void OnLateUpdate()
        {
            _ctx.MovementComponent.AddVelocity(Vector3.down * (_ctx.gravity * Time.deltaTime));
        }
    }
}
