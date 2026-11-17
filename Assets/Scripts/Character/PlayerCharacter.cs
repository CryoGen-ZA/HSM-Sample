using HSM.Core;
using HSM.PlayerStates;
using UnityEngine;

namespace Character
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacter : HsmBehaviour
    {
        private CharacterController _characterController;
        private PlayerInputHandler _inputHandler;
        public MovementComponent MovementComponent {private set; get;}

        [SerializeField] public float leanStrength = 4;
        [SerializeField] public float leanBlendSpeed = 2;

        public bool JumpPressed {private set; get;}
        public Vector2 PlayerInput {private set; get;}
        public bool IsGrounded {private set; get;}
    
        public float gravity = 9f;
        public float groundedCheckRadius = .5f;
        public float maxStickyGroundedCheck;
        public float stickyGravity;

        [Header("Movement Variables")]
        public float runSpeed = 5f;
        public float acceleration = 10f;
        public float deceleration = 20f;
        public float jumpHeight = 10f;
        public float gravityMultiplier = 2f;
        public float airControl = 0.2f;
        public float alignmentStartBlend = 1f;
        public float alignmentStopBlend = 1f;
        public Transform meshTransform;
        public float rotationSpeed = 5f;

        public float maxDotThreshold;
        public float dotThresholdDeadZone;
        public float uphillMultiplier;
        public float downHillMultiplier;
        public LayerMask ignoreMask;
        public float hillDetectionThreshold = 0.05f;
        public float hillMultiplierDeceleration =  10f;
        public float hillMultiplierAcceleration =  50f;


        protected override void Awake()
        {
            base.Awake();
            _characterController = GetComponent<CharacterController>();
            MovementComponent = new MovementComponent(_characterController);
            _inputHandler = new PlayerInputHandler();
            _inputHandler.Initialize();
            SubscribeToInputs();
            Machine.Start<GroundedBaseState>();
        }

        protected override void RegisterStates()
        {
            var grounded = AddState(new GroundedBaseState(this));
            AddState(new IdleState(), grounded, isDefault: true);
            AddState(new RunState(this), grounded);
            AddState(new JumpState(this));
            AddState(new FallingState(this));

            // Evaluated parent-first, first match wins: Grounded's exits apply while in Idle or Run.
            AddTransition<GroundedBaseState, JumpState>(() => JumpPressed);
            AddTransition<GroundedBaseState, FallingState>(() => !IsGrounded);
            AddTransition<IdleState, RunState>(() =>
                PlayerInput.magnitude > 0 || MovementComponent.GetCurrentVelocityWithoutGravity().magnitude > 0);
            AddTransition<RunState, IdleState>(() =>
                MovementComponent.GetCurrentVelocityWithoutGravity() == Vector3.zero);
            AddTransition<JumpState, FallingState>(() => MovementComponent.GetCurrentVelocity().y < 0);
            AddTransition<FallingState, GroundedBaseState>(() => IsGrounded);
        }

        private void SubscribeToInputs()
        {
            _inputHandler.OnMove += _inputHandlerOnOnMove;
            _inputHandler.OnJump += _inputHandlerOnOnJump;
        }

        private void _inputHandlerOnOnJump(bool input)
        {
            JumpPressed = input;
        }

        private void _inputHandlerOnOnMove(Vector2 input)
        {
            PlayerInput = input;
        }

        private void Update()
        {
            IsGrounded = Physics.CheckSphere(_characterController.transform.position, groundedCheckRadius, ~ignoreMask);
        
            Machine.Update();
            MovementComponent.DoUpdate();
        }
    }
}
