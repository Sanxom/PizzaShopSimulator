using PizzaShop.Core;
using PizzaShop.Interaction;
using PizzaShop.Inventory;
using PizzaShop.Player;
using UnityEngine;

namespace PizzaShop.Orders
{
    public class OrderValidationStation : InteractableBase
    {
        [Header("Configuration")]
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private string stationName = "Order Window";

        [Header("Visual Feedback")]
        [SerializeField] private GameObject successVFX;
        [SerializeField] private GameObject failureVFX;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private AudioClip failureSound;

        public InteractionType Type => interactionType;

        protected override void Awake()
        {
            base.Awake();

            if (orderManager == null)
            {
                orderManager = FindFirstObjectByType<OrderManager>();
                if (orderManager == null)
                {
                    Debug.LogError("OrderValidationStation: OrderManager not found!");
                    enabled = false;
                }
            }
        }

        public override bool CanInteract(PlayerController playerController)
        {
            base.CanInteract(playerController);

            var inventory = playerController.GetComponent<PlayerInventory>();
            // Player must be holding a pizza
            if (inventory == null) return false;

            // Check if player is holding a completed pizza
            // This would need to be implemented based on your inventory system
            return true; // Placeholder
        }

        public override void OnInteract(PlayerController player)
        {
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null)
            {
                Debug.LogWarning("OrderValidationStation: Player has no inventory");
                return;
            }

            // Get the pizza from player's hands
            Pizza pizza = GetPizzaFromPlayer(inventory);
            if (pizza == null)
            {
                Debug.LogWarning("OrderValidationStation: No pizza to submit");
                EventBus.RaiseNotificationShown("No pizza to submit!", NotificationType.Warning);
                return;
            }

            // Try to match with any active order
            Order matchedOrder = FindMatchingOrder(pizza);

            if (matchedOrder != null)
            {
                // Valid order!
                bool success = orderManager.ValidateOrder(pizza, matchedOrder);

                if (success)
                {
                    HandleSuccess(matchedOrder);
                    RemovePizzaFromPlayer(inventory);
                }
                else
                {
                    HandleFailure("Pizza doesn't match the order specifications");
                }
            }
            else
            {
                HandleFailure("No matching order found for this pizza");
            }
        }

        public override void OnAlternateInteract(PlayerController player)
        {
            base.OnAlternateInteract(player);
            // Show active orders
            EventBus.RaiseNotificationShown($"Active orders: {orderManager.ActiveOrders.Count}", NotificationType.Info);
        }

        public override string GetInteractionPrompt(PlayerController playerController)
        {
            if (!CanInteract(playerController))
                return "Need a completed pizza";

            return $"Submit Pizza to {stationName}";
        }

        public override float GetInteractionProgress()
        {
            base.GetInteractionProgress();
            return 0f;
        }

        private Order FindMatchingOrder(Pizza pizza)
        {
            foreach (var order in orderManager.ActiveOrders)
            {
                if (order.MatchesPizza(pizza))
                {
                    return order;
                }
            }

            return null;
        }

        private void HandleSuccess(Order order)
        {
            int payment = order.GetCurrentPayment();
            string message = $"Order completed! Earned ${payment}";
            EventBus.RaiseNotificationShown(message, NotificationType.Success);

            // Play success VFX
            if (successVFX != null)
            {
                Instantiate(successVFX, transform.position, Quaternion.identity);
            }

            // Play success sound
            if (successSound != null)
            {
                // AudioService would handle this
                // ServiceLocator.Get<IAudioService>().PlaySound(successSound);
            }

            Debug.Log($"OrderValidationStation: Successfully completed {order.OrderID}");
        }

        private void HandleFailure(string reason)
        {
            EventBus.RaiseNotificationShown($"Order rejected: {reason}", NotificationType.Error);

            // Play failure VFX
            if (failureVFX != null)
            {
                Instantiate(failureVFX, transform.position, Quaternion.identity);
            }

            // Play failure sound
            if (failureSound != null)
            {
                // AudioService would handle this
                // ServiceLocator.Get<IAudioService>().PlaySound(failureSound);
            }

            Debug.LogWarning($"OrderValidationStation: {reason}");
        }

        // These methods would interface with your actual inventory system
        private Pizza GetPizzaFromPlayer(PlayerInventory inventory)
        {
            // Placeholder - implement based on your inventory system
            // This should get the pizza the player is currently holding
            return null;
        }

        private void RemovePizzaFromPlayer(PlayerInventory inventory)
        {
            // Placeholder - implement based on your inventory system
            // This should remove the pizza from the player's hands
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }

        public override void OnLookEnter(PlayerController player)
        {
            base.OnLookEnter(player);
        }

        public override void OnLookExit(PlayerController player)
        {
            base.OnLookExit(player);
        }
    }
}