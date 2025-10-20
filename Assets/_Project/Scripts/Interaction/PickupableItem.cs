using UnityEngine;
using PizzaShop.Player;
using PizzaShop.Inventory;
using DG.Tweening;

namespace PizzaShop.Interaction
{
    /// <summary>
    /// Simple item that can be picked up by the player.
    /// </summary>
    public class PickupableItem : InteractableBase
    {
        [Header("Item Settings")]
        [SerializeField] private string itemID = "test_item";
        [SerializeField] private ItemType itemType = ItemType.Ingredient;

        [Header("Pickup Settings")]
        [SerializeField] private bool destroyOnPickup = true;
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private float destroyTime = 0.3f;

        public override void OnInteract(PlayerController player)
        {
            if (!player.TryGetComponent<PlayerInventory>(out var inventory))
            {
                Debug.LogWarning("[PickupableItem] No PlayerInventory found!");
                return;
            }

            InventoryItem item = new(itemID, displayName, itemType)
            {
                visualPrefab = visualPrefab != null ? visualPrefab : gameObject
            };

            // Pass THIS gameObject as the worldObject parameter
            inventory.PickupItem(item, gameObject); // <-- This is key
        }

        public override string GetInteractionPrompt(PlayerController player)
        {
            return $"Press E to pick up {displayName}";
        }
    }
}