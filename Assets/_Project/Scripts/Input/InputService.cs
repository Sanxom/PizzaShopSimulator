using UnityEngine;
using UnityEngine.InputSystem;
using PizzaShop.Core;

namespace PizzaShop.Input
{
    public class InputService : IInputService
    {
        private PlayerInputActions inputActions;

        public Vector2 MovementInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsInteracting { get; private set; }
        public bool IsAlternateInteracting { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsCancelPressed { get; private set; }

        public void Initialize()
        {
            inputActions = new PlayerInputActions();

            // Subscribe to Player action map
            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;

            inputActions.Player.Look.performed += OnLook;
            inputActions.Player.Look.canceled += OnLook;

            inputActions.Player.Interact.performed += OnInteract;
            inputActions.Player.Interact.canceled += OnInteract;

            inputActions.Player.AlternateInteract.performed += OnAlternateInteract;
            inputActions.Player.AlternateInteract.canceled += OnAlternateInteract;

            inputActions.Player.Sprint.performed += OnSprint;
            inputActions.Player.Sprint.canceled += OnSprint;

            inputActions.Player.Cancel.performed += OnCancel;

            EnablePlayerInput();

            Debug.Log("[InputService] Initialized");
        }

        public void Shutdown()
        {
            DisablePlayerInput();
            DisableUIInput();

            inputActions?.Dispose();

            Debug.Log("[InputService] Shutdown");
        }

        public void EnablePlayerInput()
        {
            inputActions.Player.Enable();
        }

        public void DisablePlayerInput()
        {
            inputActions.Player.Disable();
        }

        public void EnableUIInput()
        {
            inputActions.UI.Enable();
        }

        public void DisableUIInput()
        {
            inputActions.UI.Disable();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            IsInteracting = context.performed;
            if (context.performed)
            {
                EventBus.RaisePlayerInteracted();
            }
        }

        private void OnAlternateInteract(InputAction.CallbackContext context)
        {
            IsAlternateInteracting = context.performed;
        }

        private void OnSprint(InputAction.CallbackContext context)
        {
            IsSprinting = context.performed;
        }

        private void OnCancel(InputAction.CallbackContext context)
        {
            IsCancelPressed = context.performed;
        }
    }
}