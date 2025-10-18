using UnityEngine;
using PizzaShop.Player;

namespace PizzaShop.Interaction
{
    /// <summary>
    /// Interactable that requires holding the interact button.
    /// Example: Opening a drawer, charging something, etc.
    /// </summary>
    public class HoldInteractable : InteractableBase
    {
        [Header("Hold Settings")]
        [SerializeField] private float holdDuration = 2f;
        [SerializeField] private bool resetOnRelease = true;

        private float currentHoldTime = 0f;
        private bool isHolding = false;
        private bool hasCompleted = false;

        protected override void Awake()
        {
            base.Awake();
            interactionType = InteractionType.Hold;
        }

        public override void OnInteract(PlayerController player)
        {
            if (!isHolding)
            {
                StartHolding();
            }
        }

        public override string GetInteractionPrompt(PlayerController player)
        {
            if (hasCompleted)
            {
                return $"{displayName} - Complete!";
            }
            return $"Hold E to interact with {displayName}";
        }

        public override float GetInteractionProgress()
        {
            return Mathf.Clamp01(currentHoldTime / holdDuration);
        }

        private void Update()
        {
            if (isHolding)
            {
                currentHoldTime += Time.deltaTime;

                if (currentHoldTime >= holdDuration)
                {
                    CompleteHold();
                }
            }
            else if (resetOnRelease && currentHoldTime > 0f && currentHoldTime < holdDuration)
            {
                // Reset if released early
                currentHoldTime = Mathf.Max(0f, currentHoldTime - Time.deltaTime * 2f);
            }
        }

        private void StartHolding()
        {
            isHolding = true;
            Debug.Log($"[HoldInteractable] Started holding: {displayName}");
        }

        private void CompleteHold()
        {
            isHolding = false;
            hasCompleted = true;
            Debug.Log($"[HoldInteractable] Completed: {displayName}");
            OnHoldComplete();
        }

        protected virtual void OnHoldComplete()
        {
            // Override in derived classes for custom behavior
        }

        public void ResetHold()
        {
            currentHoldTime = 0f;
            isHolding = false;
            hasCompleted = false;
        }

        public override void OnLookExit(PlayerController player)
        {
            base.OnLookExit(player);

            if (isHolding)
            {
                isHolding = false;
            }
        }
    }
}