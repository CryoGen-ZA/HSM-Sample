using Character;
using HSM.Core;
using UnityEngine;

namespace HSM.PlayerStates
{
    public class FallingState : BaseState
    {
        private float _currentGravityMultiplier;
        private Transform _cameraTransform;
        private Vector3 _initialVelocity;
        private readonly PlayerCharacter _ctx;

        public FallingState(StateGenerator generator, PlayerCharacter ctx) : base(generator)
        {
            _ctx = ctx;
        }

        public override void EnterState()
        {
            _cameraTransform ??= Camera.main.transform;
            _initialVelocity = _ctx.MovementComponent.GetCurrentVelocity();
        }

        public override void ExitState()
        {
            _currentGravityMultiplier = 0;
        }

        public override void UpdateState()
        {
            var airControlVelocity = StateHelpers.CreateAirControllerVelocity(_cameraTransform, _ctx.PlayerInput, 
                _initialVelocity, _ctx.MovementComponent.GetCurrentVelocity(), _ctx.runSpeed, _ctx.airControl);

            airControlVelocity += Vector3.down * (_ctx.gravity * Time.deltaTime) + Vector3.down * _currentGravityMultiplier;
            _currentGravityMultiplier += _ctx.gravityMultiplier * Time.deltaTime;
            
            _ctx.MovementComponent.ReplaceVelocity(airControlVelocity);

            var directionVelocity = _ctx.MovementComponent.GetCurrentVelocityWithoutGravity();
            if (directionVelocity != Vector3.zero)
                _ctx.transform.rotation = Quaternion.RotateTowards(_ctx.transform.rotation,
                    Quaternion.LookRotation(directionVelocity.normalized), 
                    _ctx.rotationSpeed);
        }

        public override bool CheckStateSwitch(out string newState)
        {
            if (_ctx.IsGrounded)
            {
                newState = nameof(GroundedBaseState);
                return true;
            }

            newState = null;
            return false;
        }
    }
}