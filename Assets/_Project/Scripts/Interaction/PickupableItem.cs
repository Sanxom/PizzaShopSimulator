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
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory == null)
            {
                Debug.LogWarning("[PickupableItem] No PlayerInventory found!");
                return;
            }

            // Create inventory item
            InventoryItem item = new InventoryItem(itemID, displayName, itemType)
            {
                visualPrefab = visualPrefab != null ? visualPrefab : gameObject
            };

            // Try to pick up
            if (inventory.PickupItem(item, destroyOnPickup ? null : gameObject))
            {
                // Animate and destroy
                if (destroyOnPickup)
                {
                    transform.DOScale(Vector3.zero, destroyTime)
                        .SetEase(Ease.InBack)
                        .OnComplete(() => Destroy(gameObject));
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        public override string GetInteractionPrompt(PlayerController player)
        {
            return $"Press E to pick up {displayName}";
        }
    }
}