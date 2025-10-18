using UnityEngine;
using PizzaShop.Core;
using PizzaShop.Input;
using PizzaShop.Player;

namespace PizzaShop.Interaction
{
    /// <summary>
    /// Main interaction system controller.
    /// Coordinates raycasting, input handling, and UI updates.
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

        private IInteractable currentTarget;
        private bool isInteracting = false;

        private void Awake()
        {
            raycaster = GetComponent<InteractionRaycaster>();
            playerController = GetComponent<PlayerController>();
        }

        private void Start()
        {
            inputService = ServiceLocator.Get<IInputService>();

            if (interactionUI == null)
            {
                interactionUI = FindObjectOfType<InteractionUI>();
            }
        }

        private void Update()
        {
            if (!enableInteraction) return;

            // Raycast to find interactable
            currentTarget = raycaster.PerformRaycast();

            // Update UI
            UpdateInteractionUI();

            // Handle input
            HandleInteractionInput();
        }

        private void UpdateInteractionUI()
        {
            if (interactionUI == null) return;

            if (currentTarget != null && currentTarget.CanInteract(playerController))
            {
                string prompt = currentTarget.GetInteractionPrompt(playerController);
                float progress = currentTarget.GetInteractionProgress();

                interactionUI.Show(prompt, currentTarget.InteractionType == InteractionType.Hold);
                interactionUI.UpdateProgress(progress);
            }
            else
            {
                interactionUI.Hide();
            }
        }

        private void HandleInteractionInput()
        {
            if (currentTarget == null || !currentTarget.CanInteract(playerController))
            {
                return;
            }

            // Primary interaction (E key)
            if (inputService.IsInteracting)
            {
                currentTarget.OnInteract(playerController);
                EventBus.RaisePlayerInteracted();
            }

            // Alternate interaction (Q key)
            if (inputService.IsAlternateInteracting)
            {
                currentTarget.OnAlternateInteract(playerController);
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
        }
    }
}