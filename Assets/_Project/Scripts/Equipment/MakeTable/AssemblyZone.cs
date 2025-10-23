using UnityEngine;
using PizzaShop.Core;
using PizzaShop.Data;
using PizzaShop.Food;
using PizzaShop.Interaction;
using PizzaShop.Player;

namespace PizzaShop.Equipment
{
    /// <summary>
    /// Zone on make table where pizzas are assembled.
    /// Uses state machine for assembly workflow.
    /// </summary>
    public class AssemblyZone : InteractableBase
    {
        [Header("Zone Configuration")]
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private Orders.PizzaSize supportedSize = Orders.PizzaSize.Large;

        [Header("Visual Components")]
        [SerializeField] private Transform pizzaAnchor;
        [SerializeField] private MeshRenderer zoneRenderer;

        [Header("State Colors")]
        [SerializeField] private Color emptyColor = new Color(0.2f, 0.6f, 0.2f);
        [SerializeField] private Color assemblingColor = new Color(0.6f, 0.6f, 0.2f);
        [SerializeField] private Color completeColor = new Color(0.2f, 0.2f, 0.8f);

        private Pizza currentPizza;
        private AssemblyZoneState currentState;
        private MaterialPropertyBlock propertyBlock;

        // State instances
        private EmptyZoneState emptyState;
        private AssemblingZoneState assemblingState;
        private CompleteZoneState completeState;

        public Vector2Int GridPosition => gridPosition;
        public bool HasPizza => currentPizza != null;
        public Pizza CurrentPizza => currentPizza;
        public Orders.PizzaSize SupportedSize => supportedSize;

        protected override void Awake()
        {
            base.Awake();

            propertyBlock = new MaterialPropertyBlock();

            if (zoneRenderer == null)
                zoneRenderer = GetComponent<MeshRenderer>();

            if (pizzaAnchor == null)
                pizzaAnchor = transform;

            // Initialize states
            emptyState = new EmptyZoneState(this);
            assemblingState = new AssemblingZoneState(this);
            completeState = new CompleteZoneState(this);

            TransitionToState(emptyState);
        }

        public void Initialize(Vector2Int position, Orders.PizzaSize size)
        {
            gridPosition = position;
            supportedSize = size;
            gameObject.name = $"AssemblyZone_{position.x}_{position.y}_{size}";
        }

        /// <summary>
        /// Start a new pizza in this zone.
        /// </summary>
        public void StartPizza(Orders.PizzaSize size)
        {
            if (currentPizza != null)
            {
                Debug.LogWarning("[AssemblyZone] Zone already has a pizza!");
                return;
            }

            if (size != supportedSize)
            {
                Debug.LogWarning($"[AssemblyZone] Zone supports {supportedSize}, not {size}!");
                return;
            }

            // Create pizza GameObject
            GameObject pizzaObj = new GameObject($"Pizza_{size}");
            pizzaObj.transform.SetParent(pizzaAnchor);
            pizzaObj.transform.localPosition = Vector3.zero;
            pizzaObj.transform.localRotation = Quaternion.identity;

            // Add Pizza component and initialize
            currentPizza = pizzaObj.AddComponent<Pizza>();
            currentPizza.Initialize(size, this);

            TransitionToState(assemblingState);

            EventBus.RaisePizzaStarted(currentPizza);

            Debug.Log($"[AssemblyZone] Started {size} pizza at {gridPosition}");
        }

        /// <summary>
        /// Try to add ingredient to current pizza.
        /// </summary>
        public bool TryAddIngredient(IngredientData ingredient)
        {
            if (currentPizza == null)
            {
                Debug.LogWarning("[AssemblyZone] No pizza to add ingredient to!");
                return false;
            }

            return currentPizza.TryAddIngredient(ingredient);
        }

        /// <summary>
        /// Mark pizza as complete and ready for cooking.
        /// </summary>
        public void CompletePizza()
        {
            if (currentPizza == null) return;

            TransitionToState(completeState);
            EventBus.RaisePizzaCompleted(currentPizza);

            Debug.Log($"[AssemblyZone] Pizza completed at {gridPosition}");
        }

        /// <summary>
        /// Remove pizza from zone (for pickup or cancellation).
        /// </summary>
        public Pizza RemovePizza()
        {
            if (currentPizza == null) return null;

            Pizza pizza = currentPizza;
            currentPizza = null;

            TransitionToState(emptyState);

            Debug.Log($"[AssemblyZone] Pizza removed from {gridPosition}");

            return pizza;
        }

        /// <summary>
        /// Cancel current pizza assembly.
        /// </summary>
        public void CancelPizza()
        {
            if (currentPizza == null) return;

            EventBus.RaisePizzaCancelled(currentPizza);

            Destroy(currentPizza.gameObject);
            currentPizza = null;

            TransitionToState(emptyState);

            Debug.Log($"[AssemblyZone] Pizza cancelled at {gridPosition}");
        }

        private void TransitionToState(AssemblyZoneState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();
        }

        public void SetColor(Color color)
        {
            if (zoneRenderer == null) return;

            propertyBlock.SetColor("_BaseColor", color);
            zoneRenderer.SetPropertyBlock(propertyBlock);
        }

        // ==================== INTERACTION INTERFACE ====================

        public override void OnInteract(PlayerController player)
        {
            currentState?.OnInteract(player);
        }

        public override void OnAlternateInteract(PlayerController player)
        {
            currentState?.OnAlternateInteract(player);
        }

        public override string GetInteractionPrompt(PlayerController player)
        {
            return currentState?.GetPrompt(player) ?? "";
        }

        #region Assembly Zone States

        private abstract class AssemblyZoneState
        {
            protected AssemblyZone zone;

            public AssemblyZoneState(AssemblyZone zone)
            {
                this.zone = zone;
            }

            public virtual void Enter() { }
            public virtual void Exit() { }
            public abstract void OnInteract(PlayerController player);
            public virtual void OnAlternateInteract(PlayerController player) { }
            public abstract string GetPrompt(PlayerController player);
        }

        private class EmptyZoneState : AssemblyZoneState
        {
            public EmptyZoneState(AssemblyZone zone) : base(zone) { }

            public override void Enter()
            {
                zone.SetColor(zone.emptyColor);
            }

            public override void OnInteract(PlayerController player)
            {
                // Player can start a new pizza here
                zone.StartPizza(zone.supportedSize);
            }

            public override string GetPrompt(PlayerController player)
            {
                return $"Press E to start {zone.supportedSize} pizza";
            }
        }

        private class AssemblingZoneState : AssemblyZoneState
        {
            public AssemblingZoneState(AssemblyZone zone) : base(zone) { }

            public override void Enter()
            {
                zone.SetColor(zone.assemblingColor);
            }

            public override void OnInteract(PlayerController player)
            {
                var inventory = player.GetComponent<PizzaShop.Inventory.PlayerInventory>();
                if (inventory == null || !inventory.IsHoldingItem) return;

                // Get held item
                var heldItem = inventory.GetHeldItem();

                // Check if it's an ingredient
                if (heldItem.Type == PizzaShop.Inventory.ItemType.Ingredient)
                {
                    // Get ingredient data from DataService
                    if (ServiceLocator.TryGet<IDataService>(out var dataService))
                    {
                        if (dataService.TryGetIngredient(heldItem.ItemID, out var ingredient))
                        {
                            if (zone.TryAddIngredient(ingredient))
                            {
                                inventory.ClearHeldItem();
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[AssemblyZone] Ingredient not found: {heldItem.ItemID}");
                        }
                    }
                    else
                    {
                        Debug.LogError("[AssemblyZone] DataService not available!");
                    }
                }
            }

            public override void OnAlternateInteract(PlayerController player)
            {
                // Cancel pizza
                zone.CancelPizza();
            }

            public override string GetPrompt(PlayerController player)
            {
                var inventory = player.GetComponent<PizzaShop.Inventory.PlayerInventory>();
                if (inventory != null && inventory.IsHoldingItem)
                {
                    return "Press E to add ingredient | Press Q to cancel";
                }
                return "Press Q to cancel pizza";
            }
        }

        private class CompleteZoneState : AssemblyZoneState
        {
            public CompleteZoneState(AssemblyZone zone) : base(zone) { }

            public override void Enter()
            {
                zone.SetColor(zone.completeColor);
            }

            public override void OnInteract(PlayerController player)
            {
                var inventory = player.GetComponent<PizzaShop.Inventory.PlayerInventory>();
                if (inventory == null || inventory.IsHoldingItem) return;

                // Pick up completed pizza
                Pizza pizza = zone.RemovePizza();
                if (pizza != null)
                {
                    pizza.transform.SetParent(null);

                    var item = new PizzaShop.Inventory.InventoryItem(
                        "pizza_" + pizza.Size.ToString().ToLower(),
                        $"{pizza.Size} Pizza",
                        PizzaShop.Inventory.ItemType.Pizza
                    );

                    if (inventory.PickupItem(item, pizza.gameObject))
                    {
                        EventBus.RaisePizzaPickedUp(pizza);
                    }
                }
            }

            public override string GetPrompt(PlayerController player)
            {
                var inventory = player.GetComponent<PizzaShop.Inventory.PlayerInventory>();
                if (inventory != null && !inventory.IsHoldingItem)
                {
                    return "Press E to pick up pizza";
                }
                return "Hands full!";
            }
        }

        #endregion
    }
}