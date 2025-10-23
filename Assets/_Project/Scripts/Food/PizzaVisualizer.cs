using DG.Tweening;
using PizzaShop.Core;
using PizzaShop.Data;
using System.Collections.Generic;
using UnityEngine;

namespace PizzaShop.Food
{
    /// <summary>
    /// Handles visual representation of pizza assembly and cooking.
    /// Separated from Pizza logic (Single Responsibility Principle).
    /// </summary>
    public class PizzaVisualizer : MonoBehaviour
    {
        [Header("Visual Layers")]
        [SerializeField] private Transform doughLayer;
        [SerializeField] private Transform sauceLayer;
        [SerializeField] private Transform cheeseLayer;
        [SerializeField] private Transform toppingsParent;

        [Header("Prefabs")]
        [SerializeField] private GameObject defaultDoughPrefab;
        [SerializeField] private GameObject defaultSaucePrefab;
        [SerializeField] private GameObject defaultCheesePrefab;

        [Header("Animation Settings")]
        [SerializeField] private float ingredientAppearDuration = 0.3f;
        [SerializeField] private Ease ingredientAppearEase = Ease.OutBack;

        private Pizza pizza;
        private List<GameObject> toppingVisuals;
        private Material cookMaterial;
        private float currentCookDarkness = 0f;

        private void Awake()
        {
            toppingVisuals = new List<GameObject>();

            // Create layer transforms if they don't exist
            if (doughLayer == null) doughLayer = CreateLayer("Dough");
            if (sauceLayer == null) sauceLayer = CreateLayer("Sauce");
            if (cheeseLayer == null) cheeseLayer = CreateLayer("Cheese");
            if (toppingsParent == null) toppingsParent = CreateLayer("Toppings");

            // Initially hide all layers
            doughLayer.gameObject.SetActive(false);
            sauceLayer.gameObject.SetActive(false);
            cheeseLayer.gameObject.SetActive(false);
        }

        private Transform CreateLayer(string name)
        {
            GameObject layer = new GameObject(name);
            layer.transform.SetParent(transform);
            layer.transform.localPosition = Vector3.zero;
            layer.transform.localRotation = Quaternion.identity;
            return layer.transform;
        }

        public void Initialize(Pizza pizzaComponent)
        {
            pizza = pizzaComponent;
        }

        /// <summary>
        /// Add visual for ingredient.
        /// </summary>
        public void AddIngredientVisual(IngredientData ingredient)
        {
            GameObject visual = null;

            switch (ingredient.Category)
            {
                case IngredientCategory.Base:
                    visual = CreateDoughVisual(ingredient);
                    break;

                case IngredientCategory.Sauce:
                    visual = CreateSauceVisual(ingredient);
                    break;

                case IngredientCategory.Cheese:
                    visual = CreateCheeseVisual(ingredient);
                    break;

                case IngredientCategory.Topping:
                    visual = CreateToppingVisual(ingredient);
                    break;
            }

            if (visual != null)
            {
                AnimateIngredientAppear(visual);
            }
        }

        private GameObject CreateDoughVisual(IngredientData ingredient)
        {
            GameObject dough = ingredient.VisualPrefab != null
                ? Instantiate(ingredient.VisualPrefab, doughLayer)
                : Instantiate(defaultDoughPrefab, doughLayer);

            dough.transform.localPosition = Vector3.zero;
            dough.transform.localRotation = Quaternion.identity;

            doughLayer.gameObject.SetActive(true);

            return dough;
        }

        private GameObject CreateSauceVisual(IngredientData ingredient)
        {
            GameObject sauce = ingredient.VisualPrefab != null
                ? Instantiate(ingredient.VisualPrefab, sauceLayer)
                : Instantiate(defaultSaucePrefab, sauceLayer);

            sauce.transform.localPosition = Vector3.up * 0.01f; // Slightly above dough
            sauce.transform.localRotation = Quaternion.identity;

            // Tint sauce color
            if (sauce.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.material.color = ingredient.IngredientColor;
            }

            sauceLayer.gameObject.SetActive(true);

            return sauce;
        }

        private GameObject CreateCheeseVisual(IngredientData ingredient)
        {
            GameObject cheese = ingredient.VisualPrefab != null
                ? Instantiate(ingredient.VisualPrefab, cheeseLayer)
                : Instantiate(defaultCheesePrefab, cheeseLayer);

            cheese.transform.localPosition = Vector3.up * 0.02f; // Above sauce
            cheese.transform.localRotation = Quaternion.identity;

            cheeseLayer.gameObject.SetActive(true);

            return cheese;
        }

        private GameObject CreateToppingVisual(IngredientData ingredient)
        {
            if (ingredient.VisualPrefab == null)
            {
                Debug.LogWarning($"[PizzaVisualizer] No visual prefab for {ingredient.DisplayName}!");
                return null;
            }

            GameObject topping = Instantiate(ingredient.VisualPrefab, toppingsParent);

            // Random position on pizza surface
            float radius = pizza.Size switch
            {
                Orders.PizzaSize.Small => 0.15f,
                Orders.PizzaSize.Medium => 0.2f,
                Orders.PizzaSize.Large => 0.25f,
                //PizzaSize.XLarge => 0.3f,
                _ => 0.2f
            };

            Vector2 randomCircle = Random.insideUnitCircle * radius;
            topping.transform.localPosition = new Vector3(randomCircle.x, 0.03f, randomCircle.y);
            topping.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            toppingVisuals.Add(topping);

            return topping;
        }

        private void AnimateIngredientAppear(GameObject visual)
        {
            if (visual == null) return;

            // Start small and scale up
            visual.transform.localScale = Vector3.zero;
            visual.transform.DOScale(Vector3.one, ingredientAppearDuration)
                .SetEase(ingredientAppearEase);
        }

        public void UpdateState(PizzaState state)
        {
            // Visual updates based on state if needed
        }

        public void UpdateCookQuality(CookQuality quality)
        {
            // Darken pizza as it cooks
            float targetDarkness = quality switch
            {
                CookQuality.Raw => 0f,
                CookQuality.Undercooked => 0.2f,
                CookQuality.Perfect => 0.4f,
                CookQuality.Overcooked => 0.6f,
                CookQuality.Burnt => 0.9f,
                _ => 0f
            };

            DOVirtual.Float(currentCookDarkness, targetDarkness, 0.5f, darkness =>
            {
                currentCookDarkness = darkness;
                ApplyCookDarkness(darkness);
            });
        }

        private void ApplyCookDarkness(float darkness)
        {
            Color darkColor = Color.Lerp(Color.white, new Color(0.2f, 0.1f, 0.05f), darkness);

            ApplyColorToLayer(doughLayer, darkColor);
            ApplyColorToLayer(cheeseLayer, darkColor);
            ApplyColorToLayer(toppingsParent, darkColor);
        }

        private void ApplyColorToLayer(Transform layer, Color color)
        {
            if (layer == null) return;

            foreach (Transform child in layer)
            {
                if (child.TryGetComponent<Renderer>(out var renderer))
                {
                    renderer.material.color = color;
                }
            }
        }
    }
}