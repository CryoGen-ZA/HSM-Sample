using Character;
using HSM.Core;
using UnityEngine;

namespace HSM.PlayerStates
{
    public class RunState : BaseState
    {
        private static readonly int MovementSpeedHash = Animator.StringToHash("MovementSpeed");
        private static readonly int LeanValueHash = Animator.StringToHash("LeanValue");

        private Transform _cameraTransform;
        private float _currentSpeed;
        private Vector3 _lastDirection;
        private float _previousYaw;
        private float _blendedLeanAmount;
        private float _dampedHillMultiplier;
        private Vector3 _lastPosition;
        private readonly PlayerCharacter _ctx;

        public RunState(StateGenerator generator, PlayerCharacter ctx) : base(generator)
        {
            _ctx = ctx;
        }

        public override void EnterState()
        {
            _cameraTransform ??= Camera.main.transform;
        }

        public override void ExitState()
        {
            _ctx.Animator.SetFloat(MovementSpeedHash, 0);
        }

        public override void UpdateState()
        { 
            CalculateMovementVelocity();

            var directionVelocity = _ctx.MovementComponent.GetCurrentVelocityWithoutGravity();
            if (directionVelocity != Vector3.zero)
                _ctx.transform.rotation = Quaternion.RotateTowards(_ctx.transform.rotation,
                    Quaternion.LookRotation(directionVelocity.normalized), 
                    _ctx.rotationSpeed);

            ApplyAlignment();
            
            _ctx.Animator.SetFloat(MovementSpeedHash, directionVelocity.magnitude);
            _ctx.Animator.SetFloat(LeanValueHash, CalculateLeanValue());
        }
        
        private void ApplyAlignment()
        {
            if (_currentSpeed > 0)
            {
                if (Physics.Raycast(_ctx.transform.position + Vector3.up * 10, Vector3.down, out var hit, 200,
                        ~_ctx.ignoreMask))
                {
                    var targetRotation = Quaternion.FromToRotation(_ctx.meshTransform.up, hit.normal) *
                                         _ctx.transform.rotation;
                    _ctx.meshTransform.rotation = Quaternion.RotateTowards(_ctx.meshTransform.rotation, targetRotation,
                        _ctx.alignmentStartBlend * Time.deltaTime);
                }
            }
            else
            {
                _ctx.meshTransform.localRotation =  Quaternion.RotateTowards(_ctx.meshTransform.localRotation, Quaternion.Euler(Vector3.zero), _ctx.alignmentStopBlend * Time.deltaTime);
            }
        }
        
        private float GetHillMultiplier()
        {
            var surfaceDotProduct = CalculateSurfaceDotProduct();

            var remappedValue = StateHelpers.Remap(surfaceDotProduct, _ctx.dotThresholdDeadZone, _ctx.maxDotThreshold, 0, 1);
            
            var hillMultiplier = CalculateHillMultiplier();
            var targetValue = hillMultiplier * remappedValue;
            var delta = remappedValue > 0 ? _ctx.hillMultiplierAcceleration : _ctx.hillMultiplierDeceleration;
            _dampedHillMultiplier = Mathf.MoveTowards(_dampedHillMultiplier, targetValue, delta * Time.deltaTime);
        
            return _dampedHillMultiplier;
        }
        
        private float CalculateHillMultiplier()
        {
            if (Mathf.Abs(_lastPosition.y - _ctx.transform.position.y) < _ctx.hillDetectionThreshold) return 0;

            return _lastPosition.y < _ctx.transform.position.y ? _ctx.uphillMultiplier : _ctx.downHillMultiplier;
        }

        private float CalculateSurfaceDotProduct()
        {
            if (Physics.Raycast(_ctx.transform.position + Vector3.up * 10, Vector3.down, out RaycastHit hit,200, ~_ctx.ignoreMask))
            {
                return Vector3.Dot(hit.normal, Vector3.up);
            }
            return 0;
        }
        
        private float CalculateLeanValue()
        {
            var currentYaw = _ctx.transform.rotation.eulerAngles.y;
            var deltaYaw = currentYaw - _previousYaw;
            _previousYaw = currentYaw;
            var turnPerSecond = deltaYaw / Time.deltaTime;
            _blendedLeanAmount = Mathf.MoveTowards(_blendedLeanAmount, turnPerSecond/_ctx.leanStrength, _ctx.leanBlendSpeed * Time.deltaTime);
            return _blendedLeanAmount;
        }

        private void CalculateMovementVelocity()
        {
            var movementDirection = _cameraTransform.right * _ctx.PlayerInput.x + _cameraTransform.forward * _ctx.PlayerInput.y;
            movementDirection.y = -CalculateStickyGravity() * Time.deltaTime;
            
            if (_ctx.PlayerInput != Vector2.zero)
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, _ctx.runSpeed + _ctx.runSpeed * GetHillMultiplier(), _ctx.acceleration * Time.deltaTime);
                _lastDirection = movementDirection;
                _ctx.MovementComponent.ReplaceVelocity(_lastDirection.normalized * _currentSpeed);
            }
            else
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, _ctx.deceleration * Time.deltaTime); 
                if (Mathf.Approximately(_ctx.MovementComponent.GetCurrentVelocityWithoutGravity().magnitude, 0))
                {
                    _ctx.MovementComponent.ReplaceVelocity(Vector3.zero);
                }
                else
                { var decelerationVelocity =
                        Vector3.MoveTowards(_ctx.MovementComponent.GetCurrentVelocityWithoutGravity(), Vector3.zero, _ctx.deceleration * Time.deltaTime);
                    _ctx.MovementComponent.ReplaceVelocity(decelerationVelocity); //Current speed and the velocity itself needs to deceleration applied to it, It's the closest I was able to get to a "fake acceleration and deceleration" feel like in Mario
                }
            }
            _lastPosition = _ctx.transform.position;
        }

        private float CalculateStickyGravity()
        {
            if (!Physics.Raycast(_ctx.transform.position + Vector3.up, Vector3.down, out var hit,
                    _ctx.maxStickyGroundedCheck, ~_ctx.ignoreMask)) return _ctx.gravity;
            
            var distance = hit.distance - 1;
            if (!(distance > 0)) return _ctx.gravity;
            
            var stickyAlpha = StateHelpers.Remap(distance, 0, _ctx.maxStickyGroundedCheck, 0, 1);
            return _ctx.gravity + _ctx.stickyGravity * stickyAlpha;
        }

        public override bool CheckStateSwitch(out string newState)
        {
            if (_ctx.MovementComponent.GetCurrentVelocityWithoutGravity() == Vector3.zero)
            {
                newState = nameof(IdleState);
                return true;
            }

            newState = null;
            return false;
        }
    }
}