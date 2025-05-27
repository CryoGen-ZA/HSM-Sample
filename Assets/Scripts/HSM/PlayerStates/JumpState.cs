using Character;
using HSM.Core;
using UnityEngine;

namespace HSM.PlayerStates
{
    public class JumpState : BaseState
    {
        private static readonly int JumpHash = Animator.StringToHash("isJumping");
        
        private float _currentMultiJumpCooldown;
        private float _calculatedJumpHeight;
        private float _currentGravityMultiplier;
        private Transform _cameraTransform;
        private Vector3 _initialVelocity;
        private readonly PlayerCharacter _ctx;
        

        public JumpState(StateGenerator generator, PlayerCharacter ctx) : base(generator)
        {
            _ctx = ctx;
        }
        
        public override void EnterState()
        {
            _cameraTransform ??= Camera.main.transform;
            _initialVelocity = _ctx.MovementComponent.GetCurrentVelocity();
            CalculateJumpHeight();
            DoJump();
        }
        public override void ExitState()
        {
            _ctx.Animator.SetBool(JumpHash, false);
        }

        public override void UpdateState()
        {
            if (_currentMultiJumpCooldown > 0)
                _currentMultiJumpCooldown -= Time.deltaTime;

            var airControlVelocity = StateHelpers.CreateAirControllerVelocity(_cameraTransform, _ctx.PlayerInput, 
                _initialVelocity, _ctx.MovementComponent.GetCurrentVelocity(), _ctx.runSpeed, _ctx.airControl);
            
            airControlVelocity += Vector3.down * (_ctx.gravity * Time.deltaTime);
            
            _ctx.MovementComponent.ReplaceVelocity(airControlVelocity);
            
            var directionVelocity = _ctx.MovementComponent.GetCurrentVelocityWithoutGravity();
            if (directionVelocity != Vector3.zero)
                _ctx.transform.rotation = Quaternion.RotateTowards(_ctx.transform.rotation,
                    Quaternion.LookRotation(directionVelocity.normalized), 
                    _ctx.rotationSpeed);
        }

       

        public override bool CheckStateSwitch(out string newState)
        {
            if (_ctx.MovementComponent.GetCurrentVelocity().y < 0)
            {
                newState = nameof(FallingState);
                return true;
            }

            newState = null;
            return false;
        }
        
        private void DoJump()
        {
            var jumpVelocity = Vector3.zero;
            jumpVelocity.y = - _ctx.MovementComponent.GetCurrentVelocity().y; //Negate the current y velocity to counteract the gravity
            jumpVelocity.y += _calculatedJumpHeight;
            _ctx.MovementComponent.AddVelocity(jumpVelocity); //May need to render current velocity to zer to offset gravity when I implement double jumping
            _currentGravityMultiplier = 0;
            _ctx.Animator.SetBool(JumpHash, true);
        }

        private void CalculateJumpHeight()
        {
            _calculatedJumpHeight = Mathf.Sqrt(2 * (_ctx.gravity + _currentGravityMultiplier) * _ctx.jumpHeight);
        }
    }
}