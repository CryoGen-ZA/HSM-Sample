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

        public FallingState(PlayerCharacter ctx)
        {
            _ctx = ctx;
        }

        protected override void OnEnter()
        {
            _cameraTransform ??= Camera.main.transform;
            _initialVelocity = _ctx.MovementComponent.GetCurrentVelocity();
        }

        protected override void OnExit()
        {
            _currentGravityMultiplier = 0;
        }

        protected override void OnUpdate()
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
    }
}