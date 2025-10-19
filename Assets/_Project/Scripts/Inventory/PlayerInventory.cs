using UnityEngine;
using PizzaShop.Core;
using DG.Tweening;

namespace PizzaShop.Inventory
{
    /// <summary>
    /// Manages player's held items.
    /// Handles pickup, drop, and visual representation of held items.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxHeldItems = 1; // Can only hold 1 item for now

        [Header("Hand Position")]
        [SerializeField] private Transform handPosition;
        [SerializeField] private Vector3 handOffset = new Vector3(0.3f, -0.2f, 0.5f);
        [SerializeField] private float handScale = 0.5f; // Add this - scale items to 50% in hand

        [Header("Drop Settings")]
        [SerializeField] private float dropDistance = 1.5f; // How far in front to drop
        [SerializeField] private float dropForce = 2f; // Optional physics force

        [Header("Animation")]
        [SerializeField] private float pickupDuration = 0.3f;
        [SerializeField] private float dropDuration = 0.2f;

        private InventoryItem currentItem;
        private GameObject heldItemVisual;
        private Transform previousParent;
        private Vector3 previousScale;

        public bool IsHoldingItem => currentItem != null;
        public InventoryItem CurrentItem => currentItem;

        private void Awake()
        {
            // Create hand position if not assigned
            if (handPosition == null)
            {
                GameObject handObj = new GameObject("HandPosition");
                handPosition = handObj.transform;
                handPosition.SetParent(Camera.main.transform);
                handPosition.localPosition = handOffset;
            }
        }

        /// <summary>
        /// Pick up an item and add to inventory.
        /// </summary>
        public bool PickupItem(InventoryItem item, GameObject worldObject = null)
        {
            if (IsHoldingItem && maxHeldItems == 1)
            {
                Debug.Log("[PlayerInventory] Already holding an item!");
                return false;
            }

            previousParent = worldObject.transform.parent;
            previousScale = worldObject.transform.localScale;
            currentItem = item;

            // Create visual in hand
            CreateHeldItemVisual(item.visualPrefab, worldObject);

            EventBus.RaiseIngredientPickedUp(null);

            Debug.Log($"[PlayerInventory] Picked up: {item.itemName}");
            return true;
        }

        /// <summary>
        /// Drop the currently held item into the world.
        /// </summary>
        public void DropItem()
        {
            if (!IsHoldingItem) return;

            Debug.Log($"[PlayerInventory] Dropping: {currentItem.itemName}");

            if (heldItemVisual != null)
            {
                // Kill any active tweens on the object
                heldItemVisual.transform.DOKill();

                // Unparent from hand
                heldItemVisual.transform.SetParent(previousParent);
                heldItemVisual.transform.localScale = previousScale;

                // Calculate drop position (in front of player)
                //Vector3 dropPosition = Camera.main.transform.position + Camera.main.transform.forward * dropDistance;

                // Animate to drop position
                //heldItemVisual.transform.DOMove(dropPosition, dropDuration)
                //    .SetEase(Ease.OutQuad)
                //    .OnComplete(() =>
                    
                // Re-enable physics
                if (heldItemVisual.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.isKinematic = false;
                    // Optional: add a little forward force
                    rb.AddForce(Camera.main.transform.forward * dropForce, ForceMode.Impulse);
                }

                // Re-enable collider
                if (heldItemVisual.TryGetComponent<Collider>(out var col)) col.enabled = true;

                // Restore to original layer (Interactable)
                int interactableLayer = LayerMask.NameToLayer("Interactable");
                if (interactableLayer != -1)
                {
                    heldItemVisual.layer = interactableLayer;
                }
                    
            }

            currentItem = null;
            previousParent = null;
            heldItemVisual = null;
        }

        /// <summary>
        /// Use/consume the currently held item.
        /// </summary>
        public bool UseItem()
        {
            if (!IsHoldingItem) return false;

            Debug.Log($"[PlayerInventory] Used: {currentItem.itemName}");

            // For single-use items, remove after use
            DropItem();
            return true;
        }

        private void CreateHeldItemVisual(GameObject prefab, GameObject worldObject)
        {
            if (worldObject != null)
            {
                heldItemVisual = worldObject;

                // Store original state
                previousParent = heldItemVisual.transform.parent;
                previousScale = heldItemVisual.transform.localScale;

                Debug.Log($"[CreateHeldItemVisual] Stored parent: {(previousParent != null ? previousParent.name : "null")}, scale: {previousScale}");

                // Disable physics immediately
                if (heldItemVisual.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.isKinematic = true;
                }

                if (heldItemVisual.TryGetComponent<Collider>(out var col))
                {
                    col.enabled = false;
                }

                // Parent to hand immediately
                heldItemVisual.transform.SetParent(handPosition);
                heldItemVisual.transform.localPosition = Vector3.zero;
                heldItemVisual.transform.localRotation = Quaternion.identity;
                heldItemVisual.transform.localScale = Vector3.one * handScale;
            }
            else
            {
                // Instantiate new visual
                heldItemVisual = Instantiate(prefab, handPosition);
                heldItemVisual.transform.localPosition = Vector3.zero;
                heldItemVisual.transform.localRotation = Quaternion.identity;
                heldItemVisual.transform.localScale = Vector3.one * handScale;

                previousParent = null;
                previousScale = Vector3.one;
            }
        }

        //private void CreateHeldItemVisual(GameObject prefab, GameObject worldObject)
        //{
        //    if (worldObject != null)
        //    {
        //        heldItemVisual = worldObject;

        //        // Disable physics immediately
        //        if (heldItemVisual.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;

        //        if (heldItemVisual.TryGetComponent<Collider>(out var col)) col.enabled = false;

        //        // Animate directly to hand position in world space
        //        // After animation, parent to hand so it follows camera
        //        heldItemVisual.transform.DOMove(handPosition.position, pickupDuration)
        //            .SetEase(Ease.OutQuad)
        //            .OnUpdate(() =>
        //            {
        //                // Update target position each frame as camera moves
        //                // This makes it follow a moving target
        //            })
        //            .OnComplete(() =>
        //            {
        //                // Parent to hand after reaching it
        //                heldItemVisual.transform.SetParent(handPosition);
        //                heldItemVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        //                heldItemVisual.transform.localScale = Vector3.one * handScale;
        //            });

        //        // Also animate rotation
        //        heldItemVisual.transform.DORotateQuaternion(handPosition.rotation, pickupDuration)
        //            .SetEase(Ease.OutQuad);

        //        // Scale down smoothly during movement
        //        Vector3 targetScale = heldItemVisual.transform.localScale * handScale;
        //        heldItemVisual.transform.DOScale(targetScale, pickupDuration)
        //            .SetEase(Ease.OutQuad);
        //    }
        //    else
        //    {
        //        // Instantiate new visual
        //        heldItemVisual = Instantiate(prefab, handPosition);
        //        heldItemVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        //        heldItemVisual.transform.localScale = Vector3.one * handScale;
        //    }
        //}

        private void OnDestroy()
        {
            // Clean up tweens
            if (heldItemVisual != null)
            {
                heldItemVisual.transform.DOKill();
            }
        }
    }
}