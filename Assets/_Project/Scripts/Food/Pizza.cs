using UnityEngine;
using System.Collections.Generic;
using PizzaShop.Data;
using PizzaShop.Core;
using PizzaShop.Equipment;
using PizzaShop.Orders;

namespace PizzaShop.Food
{
    /// <summary>
    /// Represents a pizza being assembled or cooked.
    /// Tracks ingredients, state, and quality.
    /// </summary>
    public class Pizza : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PizzaSize size;

        [Header("Components")]
        [SerializeField] private PizzaVisualizer visualizer;

        private AssemblyZone currentZone;
        private PizzaState currentState;
        private List<IngredientData> ingredients;
        private CookQuality cookQuality;
        private float cookTimer;

        // Pizza requirements
        private bool hasDough;
        private bool hasSauce;
        private bool hasCheese;

        public PizzaSize Size => size;
        public PizzaState State => currentState;
        public IReadOnlyList<IngredientData> Ingredients => ingredients;
        public bool HasDough => hasDough;
        public bool HasSauce => hasSauce;
        public bool HasCheese => hasCheese;
        public bool IsComplete => hasDough && hasSauce && hasCheese;
        public CookQuality CookQuality => cookQuality;
        public AssemblyZone CurrentZone => currentZone;

        private void Awake()
        {
            ingredients = new List<IngredientData>();
            currentState = PizzaState.NoDough;
            cookQuality = CookQuality.Raw;

            if (visualizer == null)
            {
                visualizer = GetComponent<PizzaVisualizer>();
                if (visualizer == null)
                {
                    visualizer = gameObject.AddComponent<PizzaVisualizer>();
                }
            }
        }

        /// <summary>
        /// Initialize pizza with size and zone.
        /// </summary>
        public void Initialize(PizzaSize pizzaSize, AssemblyZone zone)
        {
            size = pizzaSize;
            currentZone = zone;

            visualizer?.Initialize(this);

            Debug.Log($"[Pizza] Initialized {size} pizza");
        }

        /// <summary>
        /// Try to add ingredient to pizza.
        /// Returns true if successful.
        /// </summary>
        public bool TryAddIngredient(IngredientData ingredient)
        {
            if (ingredient == null)
            {
                Debug.LogWarning("[Pizza] Cannot add null ingredient!");
                return false;
            }

            // Check ingredient category and requirements
            switch (ingredient.Category)
            {
                case IngredientCategory.Base:
                    if (hasDough)
                    {
                        Debug.LogWarning("[Pizza] Pizza already has dough!");
                        return false;
                    }
                    hasDough = true;
                    UpdateState(PizzaState.DoughOnly);
                    break;

                case IngredientCategory.Sauce:
                    if (!hasDough)
                    {
                        Debug.LogWarning("[Pizza] Need dough before adding sauce!");
                        return false;
                    }
                    if (hasSauce)
                    {
                        Debug.LogWarning("[Pizza] Pizza already has sauce!");
                        return false;
                    }
                    hasSauce = true;
                    UpdateState(PizzaState.DoughAndSauce);
                    break;

                case IngredientCategory.Cheese:
                    if (!hasDough || !hasSauce)
                    {
                        Debug.LogWarning("[Pizza] Need dough and sauce before adding cheese!");
                        return false;
                    }
                    if (hasCheese)
                    {
                        Debug.LogWarning("[Pizza] Pizza already has cheese!");
                        return false;
                    }
                    hasCheese = true;
                    UpdateState(PizzaState.ReadyForToppings);
                    break;

                case IngredientCategory.Topping:
                    if (!IsComplete)
                    {
                        Debug.LogWarning("[Pizza] Need dough, sauce, and cheese before adding toppings!");
                        return false;
                    }
                    // Can add multiple toppings
                    break;

                default:
                    Debug.LogWarning($"[Pizza] Unknown ingredient category: {ingredient.Category}");
                    return false;
            }

            ingredients.Add(ingredient);
            visualizer?.AddIngredientVisual(ingredient);

            EventBus.RaiseIngredientAddedToPizza(this, ingredient);

            Debug.Log($"[Pizza] Added {ingredient.DisplayName} to pizza");

            return true;
        }

        /// <summary>
        /// Update pizza state.
        /// </summary>
        private void UpdateState(PizzaState newState)
        {
            currentState = newState;
            visualizer?.UpdateState(newState);

            // Check if pizza is complete
            if (IsComplete && currentState != PizzaState.Complete)
            {
                currentState = PizzaState.Complete;
                currentZone?.CompletePizza();
            }
        }

        /// <summary>
        /// Start cooking process.
        /// </summary>
        public void StartCooking()
        {
            if (!IsComplete)
            {
                Debug.LogWarning("[Pizza] Cannot cook incomplete pizza!");
                return;
            }

            currentState = PizzaState.Cooking;
            cookTimer = 0f;

            Debug.Log($"[Pizza] Started cooking {size} pizza");
        }

        /// <summary>
        /// Update cooking progress.
        /// Called by Oven system.
        /// </summary>
        public void UpdateCooking(float deltaTime, float perfectTime, float burnTime)
        {
            if (currentState != PizzaState.Cooking)
                return;

            cookTimer += deltaTime;

            // Update cook quality based on time
            if (cookTimer < perfectTime * 0.5f)
            {
                cookQuality = CookQuality.Raw;
            }
            else if (cookTimer < perfectTime * 0.8f)
            {
                cookQuality = CookQuality.Undercooked;
            }
            else if (cookTimer < perfectTime * 1.2f)
            {
                cookQuality = CookQuality.Perfect;
            }
            else if (cookTimer < burnTime)
            {
                cookQuality = CookQuality.Overcooked;
            }
            else
            {
                cookQuality = CookQuality.Burnt;
                currentState = PizzaState.Burnt;
                EventBus.RaisePizzaBurnt(this);
            }

            visualizer?.UpdateCookQuality(cookQuality);
        }

        /// <summary>
        /// Finish cooking.
        /// </summary>
        public void FinishCooking()
        {
            currentState = PizzaState.Cooked;
            EventBus.RaisePizzaCooked(this, cookQuality);

            Debug.Log($"[Pizza] Finished cooking {size} pizza - Quality: {cookQuality}");
        }

        /// <summary>
        /// Check if pizza matches an order.
        /// </summary>
        public bool MatchesOrder(Order order)
        {
            if (order == null)
                return false;

            // Check size
            if (size != order.Size)
                return false;

            // Check required ingredients
            // This is simplified - you'd want more detailed matching logic
            foreach (var requiredIngredient in order.RequiredIngredients)
            {
                bool found = false;
                foreach (var ingredient in ingredients)
                {
                    if (ingredient.IngredientID == requiredIngredient.IngredientID)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get pizza value based on quality.
        /// </summary>
        public int GetValue()
        {
            int baseValue = size switch
            {
                PizzaSize.Small => 10,
                PizzaSize.Medium => 15,
                PizzaSize.Large => 20,
                PizzaSize.XLarge => 25,
                _ => 10
            };

            float qualityMultiplier = cookQuality switch
            {
                CookQuality.Perfect => 1.5f,
                CookQuality.Undercooked => 0.7f,
                CookQuality.Overcooked => 0.5f,
                CookQuality.Burnt => 0.1f,
                _ => 1.0f
            };

            return Mathf.RoundToInt(baseValue * qualityMultiplier);
        }
    }

    public enum PizzaState
    {
        NoDough,
        DoughOnly,
        DoughAndSauce,
        ReadyForToppings,
        Complete,
        Cooking,
        Cooked,
        Burnt
    }
}