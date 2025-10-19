using UnityEngine;
using PizzaShop.Data;
using PizzaShop.Core;
using PizzaShop.Interaction;
using PizzaShop.Player;
using DG.Tweening;

namespace PizzaShop.Equipment
{
    /// <summary>
    /// Generic, reusable container that dynamically assigns to ingredients.
    /// Uses State pattern for behavior, Observer pattern for communication.
    /// Follows Single Responsibility Principle - only manages container logic.
    /// </summary>
    public class Container : InteractableBase
    {
        [Header("Container Configuration")]
        [SerializeField] private ContainerData containerData;

        [Header("Container Components")]
        [SerializeField] private ContainerVisualizer visualizer;
        [SerializeField] private ContainerSlot currentSlot;

        // State
        private IContainerState currentState;
        private Data.IngredientData assignedIngredient;
        private int currentServings;
        private bool isBeingCarried;

        // State instances (reused, not recreated)
        public EmptyContainerState EmptyState { get; private set; }
        public FillingContainerState FillingState { get; private set; }
        public FullContainerState FullState { get; private set; }

        // Properties
        public ContainerData Data => containerData;
        public Data.IngredientData AssignedIngredient => assignedIngredient;
        public int CurrentServings => currentServings;
        public int MaxCapacity => containerData?.MaxCapacity ?? 20;
        public bool IsAssigned => assignedIngredient != null;
        public bool IsEmpty => currentServings == 0;
        public bool IsFull => currentServings >= MaxCapacity;
        public bool IsBeingCarried => isBeingCarried;
        public ContainerSlot CurrentSlot => currentSlot;

        protected override void Awake()
        {
            base.Awake();

            // Initialize states (State pattern)
            EmptyState = new EmptyContainerState();
            FillingState = new FillingContainerState();
            FullState = new FullContainerState();

            // Get or add visualizer
            if (visualizer == null)
            {
                visualizer = GetComponent<ContainerVisualizer>();
                if (visualizer == null)
                {
                    visualizer = gameObject.AddComponent<ContainerVisualizer>();
                }
            }

            // Initialize
            Initialize(containerData);
        }

        /// <summary>
        /// Initialize container with data (Dependency Injection).
        /// </summary>
        public void Initialize(ContainerData data)
        {
            containerData = data;
            currentServings = data?.InitialServings ?? 0;
            assignedIngredient = null;

            // Start in appropriate state
            if (currentServings > 0)
            {
                // TODO: Need to assign ingredient for pre-filled containers
                TransitionToState(FillingState);
            }
            else
            {
                TransitionToState(EmptyState);
            }

            UpdateVisuals();
        }

        /// <summary>
        /// Transition to new state (State pattern).
        /// </summary>
        public void TransitionToState(IContainerState newState)
        {
            currentState?.Exit(this);
            currentState = newState;
            currentState?.Enter(this);

            UpdateDisplayName();
        }

        /// <summary>
        /// Assign this container to a specific ingredient.
        /// Called when first ingredient is placed.
        /// </summary>
        public void AssignIngredient(Data.IngredientData ingredient)
        {
            if (assignedIngredient != null)
            {
                Debug.LogWarning($"[Container] Already assigned to {assignedIngredient.DisplayName}!");
                return;
            }

            assignedIngredient = ingredient;

            // Raise event (Observer pattern)
            //EventBus.OnContainerAssigned?.Invoke(this, ingredient);

            UpdateDisplayName();
            UpdateVisuals();
        }

        /// <summary>
        /// Reset container to generic state.
        /// </summary>
        public void Reset()
        {
            assignedIngredient = null;
            currentServings = 0;
            TransitionToState(EmptyState);

            // Raise event
            //EventBus.OnContainerReset?.Invoke(this);

            UpdateVisuals();
        }

        /// <summary>
        /// Add one serving to the container.
        /// </summary>
        public void AddServing()
        {
            if (currentServings >= MaxCapacity)
            {
                Debug.LogWarning($"[Container] Cannot add serving - container full!");
                return;
            }

            currentServings++;
            UpdateVisuals();

            // Raise event
            //EventBus.OnContainerRefilled?.Invoke(this, 1);
        }

        /// <summary>
        /// Remove one serving from the container.
        /// </summary>
        public void RemoveServing()
        {
            if (currentServings <= 0)
            {
                Debug.LogWarning($"[Container] Cannot remove serving - container empty!");
                return;
            }

            currentServings--;

            // Reset if empty
            if (currentServings == 0)
            {
                Reset();
            }
            else
            {
                UpdateVisuals();
            }

            // Raise event
            //EventBus.OnServingTaken?.Invoke(this, assignedIngredient);
        }

        /// <summary>
        /// Update visual representation.
        /// </summary>
        public void UpdateVisuals()
        {
            if (visualizer == null) return;

            if (IsAssigned)
            {
                visualizer.ShowAssigned(assignedIngredient, currentServings, MaxCapacity);
            }
            else
            {
                visualizer.ShowEmpty();
            }
        }

        private void UpdateDisplayName()
        {
            if (IsAssigned)
            {
                displayName = $"{assignedIngredient.DisplayName} Container";
            }
            else
            {
                displayName = "Empty Container";
            }
        }

        // ==================== INTERACTION INTERFACE ====================

        public override void OnInteract(PlayerController player)
        {
            var inventory = player.GetComponent<PizzaShop.Inventory.PlayerInventory>();
            if (inventory == null) return;

            // If holding ingredient, try to add to container
            if (inventory.IsHoldingItem)
            {
                // TODO: Get ingredient data from held item
                // For now, just log
                Debug.Log($"[Container] Player trying to add ingredient to container");
            }
            // If not holding anything and container has servings, take one
            else if (currentState.CanTakeServing(this))
            {
                TakeServingToInventory(inventory);
            }
            // If empty container, pick it up
            else if (IsEmpty && !isBeingCarried)
            {
                PickupContainer(inventory);
            }
        }

        public override void OnAlternateInteract(PlayerController player)
        {
            // Q key - Pick up container regardless of state
            var inventory = player.GetComponent<PizzaShop.Inventory.PlayerInventory>();
            if (inventory == null || isBeingCarried) return;

            if (!inventory.IsHoldingItem)
            {
                PickupContainer(inventory);
            }
        }

        public override string GetInteractionPrompt(PlayerController player)
        {
            var inventory = player.GetComponent<PizzaShop.Inventory.PlayerInventory>();
            if (inventory == null) return "";

            if (inventory.IsHoldingItem)
            {
                if (currentState.CanAddServing(this))
                {
                    return $"Press E to add ingredient ({currentServings}/{MaxCapacity})";
                }
                else if (IsFull)
                {
                    return "Container is full!";
                }
            }
            else
            {
                if (currentState.CanTakeServing(this))
                {
                    return $"Press E to take {assignedIngredient.DisplayName} ({currentServings} left) | Press Q to pick up container";
                }
                else if (IsEmpty)
                {
                    return "Press E to pick up empty container";
                }
            }

            return "";
        }

        private void TakeServingToInventory(PizzaShop.Inventory.PlayerInventory inventory)
        {
            if (!currentState.CanTakeServing(this)) return;

            // Create inventory item for the serving
            var item = new PizzaShop.Inventory.InventoryItem(
                assignedIngredient.IngredientID,
                assignedIngredient.DisplayName,
                PizzaShop.Inventory.ItemType.Ingredient
            )
            {
                visualPrefab = assignedIngredient.VisualPrefab
            };

            if (inventory.PickupItem(item, null))
            {
                currentState.OnTakeServing(this);
            }
        }

        private void PickupContainer(PizzaShop.Inventory.PlayerInventory inventory)
        {
            // Create inventory item for container
            var item = new PizzaShop.Inventory.InventoryItem(
                containerData.ContainerID,
                displayName,
                PizzaShop.Inventory.ItemType.Container
            );

            if (inventory.PickupItem(item, gameObject))
            {
                isBeingCarried = true;

                // Remove from slot
                if (currentSlot != null)
                {
                    //currentSlot.RemoveContainer();
                    currentSlot = null;
                }

                // Disable collider
                var col = GetComponent<Collider>();
                if (col != null) col.enabled = false;

                // Raise event
                //EventBus.OnContainerPickedUp?.Invoke(this);
            }
        }

        /// <summary>
        /// Called when container is placed back on table.
        /// </summary>
        public void PlaceOnSlot(ContainerSlot slot)
        {
            isBeingCarried = false;
            currentSlot = slot;

            // Re-enable collider
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = true;

            // Animate to slot position
            //transform.DOMove(slot.GetPlacementPosition(), 0.3f).SetEase(Ease.OutBack);
            //transform.DORotateQuaternion(slot.transform.rotation, 0.3f);

            // Register with slot
            //slot.PlaceContainer(this);

            // Raise event
            //EventBus.OnContainerPlaced?.Invoke(this, slot);
        }

        public void SetSlot(ContainerSlot slot)
        {
            currentSlot = slot;
        }
    }
}