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

        [Header("Animation")]
        [SerializeField] private float pickupDuration = 0.3f;
        [SerializeField] private float dropDuration = 0.2f;

        private InventoryItem currentItem;
        private GameObject heldItemVisual;

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

            currentItem = item;

            // Create visual in hand
            if (item.visualPrefab != null)
            {
                CreateHeldItemVisual(item.visualPrefab, worldObject);
            }

            EventBus.RaiseIngredientPickedUp(null); // Placeholder - will pass proper data later

            Debug.Log($"[PlayerInventory] Picked up: {item.itemName}");
            return true;
        }

        /// <summary>
        /// Drop the currently held item.
        /// </summary>
        public void DropItem()
        {
            if (!IsHoldingItem) return;

            Debug.Log($"[PlayerInventory] Dropped: {currentItem.itemName}");

            // Animate drop
            if (heldItemVisual != null)
            {
                heldItemVisual.transform.DOScale(Vector3.zero, dropDuration)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => Destroy(heldItemVisual));
            }

            currentItem = null;
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

                // Store original world position
                Vector3 startWorldPos = worldObject.transform.position;
                Vector3 targetWorldPos = handPosition.position;

                // Disable physics immediately
                Rigidbody rb = heldItemVisual.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                Collider col = heldItemVisual.GetComponent<Collider>();
                if (col != null) col.enabled = false;

                Vector3 startScale = worldObject.transform.lossyScale;

                // Animate in world space, then parent
                Sequence pickupSequence = DOTween.Sequence();
                pickupSequence.Append(heldItemVisual.transform.DOMove(targetWorldPos, pickupDuration)/*.SetEase(Ease.OutQuad)*/);
                pickupSequence.Join(heldItemVisual.transform.DORotateQuaternion(handPosition.rotation, pickupDuration)/*.SetEase(Ease.OutQuad)*/);
                //pickupSequence.Join(heldItemVisual.transform.DOScale(startScale * handScale, pickupDuration).SetEase(Ease.OutQuad));

                pickupSequence.OnComplete(() =>
                {
                    Debug.Log("Animation complete, parenting to hand");
                    heldItemVisual.transform.SetParent(handPosition);
                });
            }
            else
            {
                // Instantiate new visual
                heldItemVisual = Instantiate(prefab, handPosition);
                heldItemVisual.transform.localPosition = Vector3.zero;
                heldItemVisual.transform.localRotation = Quaternion.identity;

                //heldItemVisual.transform.localScale = Vector3.zero;
                //heldItemVisual.transform.DOScale(Vector3.one * handScale, pickupDuration)
                //    .SetEase(Ease.OutBack);
            }
        }

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