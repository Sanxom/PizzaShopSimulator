using UnityEngine;
using PizzaShop.Player;
using PizzaShop.Utilities;

namespace PizzaShop.Interaction
{
    /// <summary>
    /// Base class for interactable objects.
    /// Provides default implementations and common functionality.
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Interactable Settings")]
        [SerializeField] protected string displayName = "Interactable";
        [SerializeField] protected InteractionType interactionType = InteractionType.Instant;
        [SerializeField] protected bool isInteractable = true;

        [Header("Visual Feedback")]
        [SerializeField] protected bool useOutline = true;
        [SerializeField] protected Color outlineColor = Color.yellow;
        [SerializeField] protected float outlineWidth = 5f;

        protected Outline outline;
        protected bool isHighlighted = false;

        // Interface Properties
        public virtual string DisplayName => displayName;
        public virtual InteractionType InteractionType => interactionType;

        protected virtual void Awake()
        {
            // Add outline component for highlighting
            if (useOutline)
            {
                outline = gameObject.AddComponent<Outline>();
                outline.OutlineColor = outlineColor;
                outline.OutlineWidth = outlineWidth;
                outline.enabled = false;
            }
        }

        // Interface Methods
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
            return 0f; // Override in hold-type interactions
        }

        public virtual void OnLookEnter(PlayerController player)
        {
            if (useOutline && outline != null)
            {
                outline.enabled = true;
                isHighlighted = true;
            }
        }

        public virtual void OnLookExit(PlayerController player)
        {
            if (useOutline && outline != null)
            {
                outline.enabled = false;
                isHighlighted = false;
            }
        }

        protected virtual void OnDestroy()
        {
            if (outline != null)
            {
                Destroy(outline);
            }
        }
    }
}