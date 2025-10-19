using UnityEngine;
using PizzaShop.Player;

namespace PizzaShop.Interaction
{
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Interactable Settings")]
        [SerializeField] protected string displayName = "Interactable";
        [SerializeField] protected InteractionType interactionType = InteractionType.Instant;
        [SerializeField] protected bool isInteractable = true;

        [Header("Visual Feedback")]
        [SerializeField] protected bool useOutline = true;
        [SerializeField] protected Color outlineColor = Color.yellow; // Not used with layer system
        [SerializeField] protected float outlineWidth = 5f; // Not used with layer system

        protected bool isHighlighted = false;
        private int originalLayer;
        private int highlightedLayer;

        // Interface Properties
        public virtual string DisplayName => displayName;
        public virtual InteractionType InteractionType => interactionType;

        protected virtual void Awake()
        {
            // Store original layer (should be "Interactable")
            originalLayer = gameObject.layer;

            // Get the "Highlighted" layer for outline rendering
            highlightedLayer = LayerMask.NameToLayer("Highlighted");

            if (highlightedLayer == -1)
            {
                Debug.LogError("[InteractableBase] 'Highlighted' layer not found! Please create it in Project Settings > Tags and Layers");
            }

            // Make sure this object is on the Interactable layer so raycast can find it
            if (gameObject.layer != LayerMask.NameToLayer("Interactable"))
            {
                Debug.LogWarning($"[InteractableBase] {gameObject.name} should be on 'Interactable' layer for raycasting!");
            }
        }

        public virtual bool CanInteract(PlayerController player)
        {
            return isInteractable;
        }

        public abstract void OnInteract(PlayerController player);

        public virtual void OnAlternateInteract(PlayerController player)
        {
            // Default: no alternate interaction
        }

        public abstract string GetInteractionPrompt(PlayerController player);

        public virtual float GetInteractionProgress()
        {
            return 0f;
        }

        public virtual void OnLookEnter(PlayerController player)
        {
            if (useOutline && highlightedLayer != -1)
            {
                // Switch to Highlighted layer to trigger outline
                SetLayerRecursively(gameObject, highlightedLayer);
                isHighlighted = true;
            }
        }

        public virtual void OnLookExit(PlayerController player)
        {
            if (useOutline)
            {
                // Restore original layer (Interactable)
                SetLayerRecursively(gameObject, originalLayer);
                isHighlighted = false;
            }
        }

        // Helper method to set layer on object and all children
        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        protected virtual void OnDestroy()
        {
            // Cleanup if needed
        }
    }
}