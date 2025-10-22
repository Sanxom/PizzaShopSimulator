using UnityEngine;
using DG.Tweening;
using PizzaShop.Food;
using PizzaShop.Data;

namespace PizzaShop.Equipment
{
    /// <summary>
    /// Individual slot inside an oven that holds one pizza.
    /// Tracks cooking progress and manages pizza visual.
    /// </summary>
    public class OvenSlot : MonoBehaviour
    {
        [Header("Slot Configuration")]
        [SerializeField] private int slotIndex;

        [Header("Visual Components")]
        [SerializeField] private Transform pizzaAnchor;
        [SerializeField] private MeshRenderer slotRenderer;

        [Header("Indicator")]
        [SerializeField] private GameObject cookingIndicator;

        private Pizza currentPizza;
        private float cookingTimer;
        private bool isCooking;

        public int SlotIndex => slotIndex;
        public bool IsOccupied => currentPizza != null;
        public Pizza CurrentPizza => currentPizza;
        public float CookingTimer => cookingTimer;
        public bool IsCooking => isCooking;

        private void Awake()
        {
            if (pizzaAnchor == null)
                pizzaAnchor = transform;

            if (cookingIndicator != null)
                cookingIndicator.SetActive(false);
        }

        /// <summary>
        /// Initialize slot with index.
        /// </summary>
        public void Initialize(int index)
        {
            slotIndex = index;
            gameObject.name = $"OvenSlot_{index}";
        }

        /// <summary>
        /// Place pizza in slot and start cooking.
        /// </summary>
        public void PlacePizza(Pizza pizza)
        {
            if (currentPizza != null)
            {
                Debug.LogWarning($"[OvenSlot] Slot {slotIndex} already occupied!");
                return;
            }

            currentPizza = pizza;
            cookingTimer = 0f;
            isCooking = true;

            // Position pizza in slot
            pizza.transform.SetParent(pizzaAnchor);
            pizza.transform.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
            pizza.transform.DOLocalRotate(Vector3.zero, 0.3f);

            // Start cooking
            pizza.StartCooking();

            if (cookingIndicator != null)
                cookingIndicator.SetActive(true);

            Debug.Log($"[OvenSlot] Pizza placed in slot {slotIndex}");
        }

        /// <summary>
        /// Remove pizza from slot.
        /// </summary>
        public Pizza RemovePizza()
        {
            if (currentPizza == null)
                return null;

            Pizza pizza = currentPizza;
            currentPizza = null;
            cookingTimer = 0f;
            isCooking = false;

            if (cookingIndicator != null)
                cookingIndicator.SetActive(false);

            Debug.Log($"[OvenSlot] Pizza removed from slot {slotIndex}");

            return pizza;
        }

        /// <summary>
        /// Update cooking progress.
        /// Called by Oven component.
        /// </summary>
        public void UpdateCooking(float deltaTime, float perfectTime, float burnTime)
        {
            if (!isCooking || currentPizza == null)
                return;

            cookingTimer += deltaTime;
            currentPizza.UpdateCooking(deltaTime, perfectTime, burnTime);

            // Check if burnt
            if (currentPizza.CookQuality == CookQuality.Burnt)
            {
                isCooking = false;
                Debug.LogWarning($"[OvenSlot] Pizza burnt in slot {slotIndex}!");
            }
        }

        /// <summary>
        /// Get cooking progress (0-1).
        /// </summary>
        public float GetCookingProgress(float perfectTime)
        {
            if (!isCooking || perfectTime <= 0f)
                return 0f;

            return Mathf.Clamp01(cookingTimer / perfectTime);
        }

        /// <summary>
        /// Get world position for pizza placement.
        /// </summary>
        public Vector3 GetPizzaPosition()
        {
            return pizzaAnchor.position;
        }
    }
}