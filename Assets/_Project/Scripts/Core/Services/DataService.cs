using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PizzaShop.Data;

namespace PizzaShop.Core
{
    /// <summary>
    /// Implementation of IDataService.
    /// Loads and manages all ScriptableObject data at runtime.
    /// Uses Resources folder for data loading.
    /// </summary>
    public class DataService : IDataService
    {
        private Dictionary<string, IngredientData> ingredients;
        private Dictionary<string, ContainerData> containers;
        private Dictionary<string, MakeTableData> makeTables;
        private Dictionary<string, OvenData> ovens;

        public bool IsDataLoaded { get; private set; }

        public void Initialize()
        {
            Debug.Log("[DataService] Initializing...");

            ingredients = new Dictionary<string, IngredientData>();
            containers = new Dictionary<string, ContainerData>();
            makeTables = new Dictionary<string, MakeTableData>();
            ovens = new Dictionary<string, OvenData>();

            LoadAllData();

            IsDataLoaded = true;
            Debug.Log("[DataService] Initialized successfully");
        }

        public void Shutdown()
        {
            ingredients?.Clear();
            containers?.Clear();
            makeTables?.Clear();
            IsDataLoaded = false;

            Debug.Log("[DataService] Shutdown complete");
        }

        // ==================== DATA LOADING ====================

        private void LoadAllData()
        {
            LoadIngredients();
            LoadContainers();
            LoadMakeTables();
            LoadOvens();
        }

        private void LoadIngredients()
        {
            var loadedIngredients = Resources.LoadAll<IngredientData>("Data/Ingredients");

            foreach (var ingredient in loadedIngredients)
            {
                if (string.IsNullOrEmpty(ingredient.IngredientID))
                {
                    Debug.LogWarning($"[DataService] Ingredient {ingredient.name} has no ID!");
                    continue;
                }

                if (ingredients.ContainsKey(ingredient.IngredientID))
                {
                    Debug.LogWarning($"[DataService] Duplicate ingredient ID: {ingredient.IngredientID}");
                    continue;
                }

                ingredients.Add(ingredient.IngredientID, ingredient);
            }

            Debug.Log($"[DataService] Loaded {ingredients.Count} ingredients");
        }

        private void LoadContainers()
        {
            var loadedContainers = Resources.LoadAll<ContainerData>("Data/Containers");

            foreach (var container in loadedContainers)
            {
                if (string.IsNullOrEmpty(container.ContainerID))
                {
                    Debug.LogWarning($"[DataService] Container {container.name} has no ID!");
                    continue;
                }

                if (containers.ContainsKey(container.ContainerID))
                {
                    Debug.LogWarning($"[DataService] Duplicate container ID: {container.ContainerID}");
                    continue;
                }

                containers.Add(container.ContainerID, container);
            }

            Debug.Log($"[DataService] Loaded {containers.Count} containers");
        }

        private void LoadMakeTables()
        {
            var loadedTables = Resources.LoadAll<MakeTableData>("Data/MakeTables");

            foreach (var table in loadedTables)
            {
                if (string.IsNullOrEmpty(table.TableID))
                {
                    Debug.LogWarning($"[DataService] MakeTable {table.name} has no ID!");
                    continue;
                }

                if (makeTables.ContainsKey(table.TableID))
                {
                    Debug.LogWarning($"[DataService] Duplicate make table ID: {table.TableID}");
                    continue;
                }

                makeTables.Add(table.TableID, table);
            }

            Debug.Log($"[DataService] Loaded {makeTables.Count} make tables");
        }

        private void LoadOvens()
        {
            var loadedOvens = Resources.LoadAll<OvenData>("Data/Ovens");

            foreach (var oven in loadedOvens)
            {
                if (string.IsNullOrEmpty(oven.OvenID))
                {
                    Debug.LogWarning($"[DataService] Oven {oven.name} has no ID!");
                    continue;
                }

                if (ovens.ContainsKey(oven.OvenID))
                {
                    Debug.LogWarning($"[DataService] Duplicate oven ID: {oven.OvenID}");
                    continue;
                }

                ovens.Add(oven.OvenID, oven);
            }

            Debug.Log($"[DataService] Loaded {ovens.Count} ovens");
        }

        // ==================== INGREDIENT DATA ====================

        public IngredientData GetIngredient(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("[DataService] Cannot get ingredient with null/empty ID");
                return null;
            }

            if (ingredients.TryGetValue(id, out var ingredient))
            {
                return ingredient;
            }

            Debug.LogError($"[DataService] Ingredient not found: {id}");
            return null;
        }

        public bool TryGetIngredient(string id, out IngredientData ingredient)
        {
            if (string.IsNullOrEmpty(id))
            {
                ingredient = null;
                return false;
            }

            return ingredients.TryGetValue(id, out ingredient);
        }

        public IReadOnlyList<IngredientData> GetAllIngredients()
        {
            return ingredients.Values.ToList();
        }

        public IReadOnlyList<IngredientData> GetIngredientsByCategory(IngredientCategory category)
        {
            return ingredients.Values
                .Where(i => i.Category == category)
                .ToList();
        }

        // ==================== CONTAINER DATA ====================

        public ContainerData GetContainer(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("[DataService] Cannot get container with null/empty ID");
                return null;
            }

            if (containers.TryGetValue(id, out var container))
            {
                return container;
            }

            Debug.LogError($"[DataService] Container not found: {id}");
            return null;
        }

        public bool TryGetContainer(string id, out ContainerData container)
        {
            if (string.IsNullOrEmpty(id))
            {
                container = null;
                return false;
            }

            return containers.TryGetValue(id, out container);
        }

        public IReadOnlyList<ContainerData> GetAllContainers()
        {
            return containers.Values.ToList();
        }

        // ==================== MAKE TABLE DATA ====================

        public MakeTableData GetMakeTable(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("[DataService] Cannot get make table with null/empty ID");
                return null;
            }

            if (makeTables.TryGetValue(id, out var table))
            {
                return table;
            }

            Debug.LogError($"[DataService] Make table not found: {id}");
            return null;
        }

        public bool TryGetMakeTable(string id, out MakeTableData table)
        {
            if (string.IsNullOrEmpty(id))
            {
                table = null;
                return false;
            }

            return makeTables.TryGetValue(id, out table);
        }

        public IReadOnlyList<MakeTableData> GetAllMakeTables()
        {
            return makeTables.Values.ToList();
        }

        // ==================== OVEN DATA ====================

        public OvenData GetOven(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("[DataService] Cannot get oven with null/empty ID");
                return null;
            }

            if (ovens.TryGetValue(id, out var oven))
            {
                return oven;
            }

            Debug.LogError($"[DataService] Oven not found: {id}");
            return null;
        }

        public bool TryGetOven(string id, out OvenData oven)
        {
            if (string.IsNullOrEmpty(id))
            {
                oven = null;
                return false;
            }

            return ovens.TryGetValue(id, out oven);
        }

        public IReadOnlyList<OvenData> GetAllOvens()
        {
            return ovens.Values.ToList();
        }

        // ==================== UTILITY ====================

        public void ReloadAllData()
        {
            Debug.Log("[DataService] Reloading all data...");

            ingredients.Clear();
            containers.Clear();
            makeTables.Clear();
            ovens.Clear();

            LoadAllData();

            Debug.Log("[DataService] Data reload complete");
        }
    }
}