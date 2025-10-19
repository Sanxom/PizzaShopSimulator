using PizzaShop.Core;
using PizzaShop.Input;
using PizzaShop.Inventory;
using PizzaShop.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PizzaShop.Interaction
{
    /// <summary>
    /// Main interaction system controller.
    /// Uses event-driven architecture instead of polling.
    /// </summary>
    [RequireComponent(typeof(InteractionRaycaster))]
    public class InteractionManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InteractionUI interactionUI;

        [Header("Settings")]
        [SerializeField] private bool enableInteraction = true;

        private InteractionRaycaster raycaster;
        private IInputService inputService;
        private PlayerController playerController;
        private PlayerInputActions inputActions;

        private IInteractable currentTarget;
        private IInteractable lastTarget; // Track changes

        private void Awake()
        {
            raycaster = GetComponent<InteractionRaycaster>();
            playerController = GetComponent<PlayerController>();
            inputActions = new PlayerInputActions();
        }

        private void Start()
        {
            inputService = ServiceLocator.Get<IInputService>();

            if (interactionUI == null)
            {
                interactionUI = FindFirstObjectByType<InteractionUI>();
            }

            // Subscribe to input events
            inputActions.Player.Interact.performed += OnInteractPerformed;
            inputActions.Player.AlternateInteract.performed += OnAlternateInteractPerformed;
            inputActions.Player.Cancel.performed += OnCancelPerformed;

            inputActions.Player.Enable();
        }

        private void Update()
        {
            if (!enableInteraction) return;

            // Only raycast (necessary for targeting)
            currentTarget = raycaster.PerformRaycast();

            // Only update UI when target changes
            if (currentTarget != lastTarget)
            {
                UpdateInteractionUI();
                lastTarget = currentTarget;
            }
        }

        private void UpdateInteractionUI()
        {
            if (interactionUI == null) return;

            if (currentTarget != null && currentTarget.CanInteract(playerController))
            {
                string prompt = currentTarget.GetInteractionPrompt(playerController);
                float progress = currentTarget.GetInteractionProgress();

                interactionUI.Show(prompt, currentTarget.InteractionType == InteractionType.Hold);

                // Only update progress if it's a hold interaction
                if (currentTarget.InteractionType == InteractionType.Hold)
                {
                    interactionUI.UpdateProgress(progress);
                }
            }
            else
            {
                interactionUI.Hide();
            }
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (!enableInteraction || currentTarget == null) return;
            if (!currentTarget.CanInteract(playerController)) return;

            currentTarget.OnInteract(playerController);
            EventBus.RaisePlayerInteracted();
        }

        private void OnAlternateInteractPerformed(InputAction.CallbackContext context)
        {
            if (!enableInteraction || currentTarget == null) return;
            if (!currentTarget.CanInteract(playerController)) return;

            currentTarget.OnAlternateInteract(playerController);
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            if (!enableInteraction) return;

            // Try to drop item if holding one
            PlayerInventory inventory = playerController.GetComponent<PlayerInventory>();
            if (inventory != null && inventory.IsHoldingItem)
            {
                inventory.DropItem();
            }
        }

        public void SetInteractionEnabled(bool enabled)
        {
            enableInteraction = enabled;

            if (!enabled)
            {
                raycaster.ClearInteractable();
                if (interactionUI != null)
                {
                    interactionUI.Hide();
                }
            }
        }

        public IInteractable GetCurrentTarget() => currentTarget;

        private void OnDisable()
        {
            raycaster.ClearInteractable();

            // Unsubscribe from input events
            inputActions.Player.Interact.performed -= OnInteractPerformed;
            inputActions.Player.AlternateInteract.performed -= OnAlternateInteractPerformed;
            inputActions.Player.Cancel.performed -= OnCancelPerformed;
        }

        private void OnDestroy()
        {
            inputActions?.Dispose();
        }
    }
}