using PizzaShop.Core;
using PizzaShop.Food;
using PizzaShop.Player;
using UnityEngine;

namespace PizzaShop.Equipment
{
    /// <summary>
    /// Base state for oven state machine.
    /// </summary>
    public abstract class OvenState
    {
        protected Oven oven;

        public OvenState(Oven ovenRef)
        {
            oven = ovenRef;
        }

        public virtual void Enter() { }
        public virtual void Update(float deltaTime) { }
        public virtual void Exit() { }
        public abstract void OnInteract(PlayerController player);
        public virtual void OnAlternateInteract(PlayerController player) { }
        public abstract string GetPrompt(PlayerController player);
    }

    /// <summary>
    /// Oven is off and cold.
    /// </summary>
    public class OvenOffState : OvenState
    {
        public OvenOffState(Oven ovenRef) : base(ovenRef) { }

        public override void Enter()
        {
            oven.SetTemperature(0f);
            oven.UpdateVisuals();
        }

        public override void OnInteract(PlayerController player)
        {
            // Turn on oven
            oven.TurnOn();
        }

        public override string GetPrompt(PlayerController player)
        {
            return "Press E to turn on oven";
        }
    }

    /// <summary>
    /// Oven is heating up to cooking temperature.
    /// </summary>
    public class OvenHeatingState : OvenState
    {
        private float heatingTimer;

        public OvenHeatingState(Oven ovenRef) : base(ovenRef) { }

        public override void Enter()
        {
            heatingTimer = 0f;
            oven.UpdateVisuals();
        }

        public override void Update(float deltaTime)
        {
            heatingTimer += deltaTime;

            // Calculate temperature
            float progress = heatingTimer / oven.Data.HeatUpTime;
            float temperature = Mathf.Lerp(0f, oven.Data.CookingTemperature, progress);
            oven.SetTemperature(temperature);

            // Check if reached cooking temperature
            if (heatingTimer >= oven.Data.HeatUpTime)
            {
                oven.TransitionToState(oven.ReadyState);
            }
        }

        public override void OnInteract(PlayerController player)
        {
            // Cannot interact while heating
        }

        public override void OnAlternateInteract(PlayerController player)
        {
            // Turn off oven
            oven.TurnOff();
        }

        public override string GetPrompt(PlayerController player)
        {
            float progress = heatingTimer / oven.Data.HeatUpTime;
            return $"Heating... {Mathf.RoundToInt(progress * 100)}% | Press Q to turn off";
        }
    }

    /// <summary>
    /// Oven is at cooking temperature and ready.
    /// </summary>
    public class OvenReadyState : OvenState
    {
        public OvenReadyState(Oven ovenRef) : base(ovenRef) { }

        public override void Enter()
        {
            oven.SetTemperature(oven.Data.CookingTemperature);
            oven.UpdateVisuals();
        }

        public override void OnInteract(PlayerController player)
        {
            var inventory = player.GetComponent<PizzaShop.Inventory.PlayerInventory>();
            if (inventory == null || !inventory.IsHoldingItem)
                return;

            var heldItem = inventory.GetHeldItem();

            // Check if holding a pizza
            if (heldItem.Type == PizzaShop.Inventory.ItemType.Pizza)
            {
                // Try to place pizza in oven
                if (oven.TryPlacePizza(inventory, heldItem))
                {
                    // Pizza placed successfully
                }
            }
        }

        public override void OnAlternateInteract(PlayerController player)
        {
            // Turn off oven
            oven.TurnOff();
        }

        public override string GetPrompt(PlayerController player)
        {
            var inventory = player.GetComponent<PizzaShop.Inventory.PlayerInventory>();
            if (inventory != null && inventory.IsHoldingItem)
            {
                var heldItem = inventory.GetHeldItem();
                if (heldItem.Type == PizzaShop.Inventory.ItemType.Pizza)
                {
                    if (oven.HasEmptySlot())
                    {
                        return "Press E to place pizza in oven | Press Q to turn off";
                    }
                    return "Oven is full! | Press Q to turn off";
                }
            }

            if (oven.HasPizzas())
            {
                return "Press E to check pizzas | Press Q to turn off";
            }

            return "Oven ready | Press Q to turn off";
        }
    }

    /// <summary>
    /// Oven is cooking pizzas.
    /// </summary>
    public class OvenCookingState : OvenState
    {
        public OvenCookingState(Oven ovenRef) : base(ovenRef) { }

        public override void Enter()
        {
            oven.UpdateVisuals();
        }

        public override void Update(float deltaTime)
        {
            // Update all cooking pizzas
            oven.UpdateCookingPizzas(deltaTime);
        }

        public override void OnInteract(PlayerController player)
        {
            var inventory = player.GetComponent<PizzaShop.Inventory.PlayerInventory>();
            if (inventory == null)
                return;

            // If holding pizza and oven has space, add it
            if (inventory.IsHoldingItem)
            {
                var heldItem = inventory.GetHeldItem();
                if (heldItem.Type == PizzaShop.Inventory.ItemType.Pizza)
                {
                    oven.TryPlacePizza(inventory, heldItem);
                }
            }
            // Otherwise try to remove a cooked pizza
            else
            {
                oven.TryRemoveCookedPizza(inventory);
            }
        }

        public override void OnAlternateInteract(PlayerController player)
        {
            // Turn off oven (warning if pizzas cooking)
            if (oven.HasPizzas())
            {
                Debug.LogWarning("[Oven] Cannot turn off oven while pizzas are cooking!");
                // Could add a confirmation dialog here
            }
            else
            {
                oven.TurnOff();
            }
        }

        public override string GetPrompt(PlayerController player)
        {
            var inventory = player.GetComponent<PizzaShop.Inventory.PlayerInventory>();
            if (inventory != null)
            {
                if (inventory.IsHoldingItem)
                {
                    var heldItem = inventory.GetHeldItem();
                    if (heldItem.Type == PizzaShop.Inventory.ItemType.Pizza && oven.HasEmptySlot())
                    {
                        return "Press E to add pizza";
                    }
                }
                else if (oven.HasCookedPizza())
                {
                    return "Press E to remove pizza";
                }
            }

            return $"Cooking... ({oven.GetOccupiedSlotCount()}/{oven.Data.MaxPizzas})";
        }
    }
}