using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    public class PlayerInputHandler
    {
        private PlayerInputActions _inputActions;
        private bool _isInitialized = false;
    
        public event Action<Vector2> OnMove;
        public event Action<bool> OnJump;

        public void Initialize()
        {
            if (_isInitialized) return;
        
            _inputActions ??= new PlayerInputActions();
            _inputActions.Enable();
            SubscribeInputEvents();
        }

        private void SubscribeInputEvents()
        {
            _inputActions.Movement.Get().actionTriggered += OnMovementTriggered;
            _inputActions.Jump.Get().actionTriggered += OnJumpTriggered;
        }

        private void OnJumpTriggered(InputAction.CallbackContext ctx)
        {
            OnJump?.Invoke(ctx.ReadValueAsButton());
        }
        private void OnMovementTriggered(InputAction.CallbackContext ctx)
        {
            OnMove?.Invoke(ctx.ReadValue<Vector2>());
        }

        public void Deinitialize()
        {
            UnSubscribeInputEvents();
            _inputActions.Disable();
        }

        private void UnSubscribeInputEvents()
        {
            _inputActions.Movement.Get().actionTriggered -= OnMovementTriggered;
            _inputActions.Jump.Get().actionTriggered -= OnJumpTriggered;
        }
    }
}
