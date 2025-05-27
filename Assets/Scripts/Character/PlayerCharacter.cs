using HSM.PlayerStates;
using UnityEngine;
using StateMachineBehaviour = HSM.Core.StateMachineBehaviour;

namespace Character
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacter : StateMachineBehaviour
    {
        private CharacterController _characterController;
        private PlayerInputHandler _inputHandler;
        public MovementComponent MovementComponent {private set; get;}

        public string GetStatesDebugText => _stateMachine.GetStateAndSubStateNames();
    
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
            _stateMachine.SetState(_stateMachine.Generator.GetState(nameof(GroundedBaseState)));
        }
        
        protected override void RegisterStates()
        {
            RegisterState(new GroundedBaseState(_stateMachine.Generator, this));
            RegisterState(new IdleState(_stateMachine.Generator, this));
            RegisterState(new RunState(_stateMachine.Generator, this));
            RegisterState(new JumpState(_stateMachine.Generator, this));
            RegisterState(new FallingState(_stateMachine.Generator, this));
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
        
            _stateMachine.DoUpdate();
            MovementComponent.DoUpdate();
        }
    }
}
