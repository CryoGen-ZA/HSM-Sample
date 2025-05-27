using Character;
using HSM.Core;
using UnityEngine;

namespace HSM.PlayerStates
{
    public class GroundedBaseState : BaseState
    {
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private readonly PlayerCharacter _ctx;

        public GroundedBaseState(StateGenerator generator, PlayerCharacter ctx) : base(generator)
        {
            _ctx = ctx;
        }

        public override void EnterState()
        {
            _ctx.Animator.SetBool(IsGroundedHash, true);
            SetSubState(Generator.GetState(nameof(IdleState)));
        }

        public override void ExitState()
        {
            _ctx.Animator.SetBool(IsGroundedHash, false);
        }

        public override void UpdateState()
        {
            UpdateSubState();
            _ctx.MovementComponent.AddVelocity(Vector3.down * (_ctx.gravity * Time.deltaTime));
        }

        public override bool CheckStateSwitch(out string newState)
        {
            if (_ctx.JumpPressed)
            {
                newState = nameof(JumpState);
                return true;
            }

            if (!_ctx.IsGrounded)
            {
                newState = nameof(FallingState);
                return true;
            }
            
            newState = null;
            return false;
        }
    }
}