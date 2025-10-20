using UnityEngine;
using PizzaShop.Core;
using PizzaShop.Data;

namespace PizzaShop.Testing
{
    /// <summary>
    /// Test script to verify DataService functionality.
    /// Attach to a GameObject in the scene and press Play.
    /// </summary>
    public class DataServiceTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestOnStart = true;

        private void Start()
        {
            if (runTestOnStart)
            {
                RunTests();
            }
        }

        [ContextMenu("Run DataService Tests")]
        public void RunTests()
        {
            Debug.Log("=== Starting DataService Tests ===");

            // Check if DataService is registered
            if (!ServiceLocator.IsRegistered<IDataService>())
            {
                Debug.LogError("DataService is not registered!");
                return;
            }

            var dataService = ServiceLocator.Get<IDataService>();

            // Test 1: Data Loaded
            Debug.Log($"[Test 1] Data Loaded: {dataService.IsDataLoaded}");

            // Test 2: Get All Ingredients
            var ingredients = dataService.GetAllIngredients();
            Debug.Log($"[Test 2] Total Ingredients: {ingredients.Count}");
            foreach (var ingredient in ingredients)
            {
                Debug.Log($"  - {ingredient.DisplayName} ({ingredient.IngredientID}) - Category: {ingredient.Category}");
            }

            // Test 3: Get Ingredients by Category
            var sauces = dataService.GetIngredientsByCategory(IngredientCategory.Sauce);
            Debug.Log($"[Test 3] Sauce Ingredients: {sauces.Count}");

            // Test 4: Get Specific Ingredient
            if (dataService.TryGetIngredient("dough", out var dough))
            {
                Debug.Log($"[Test 4] Found ingredient: {dough.DisplayName}");
            }
            else
            {
                Debug.Log("[Test 4] Ingredient 'dough' not found (check ID in ScriptableObject)");
            }

            // Test 5: Get All Containers
            var containers = dataService.GetAllContainers();
            Debug.Log($"[Test 5] Total Containers: {containers.Count}");

            // Test 6: Get All Make Tables
            var tables = dataService.GetAllMakeTables();
            Debug.Log($"[Test 6] Total Make Tables: {tables.Count}");

            Debug.Log("=== DataService Tests Complete ===");
        }
    }
}