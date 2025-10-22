using PizzaShop.Data;
using System.Collections.Generic;

namespace PizzaShop.Core
{
    /// <summary>
    /// Service interface for managing game data (ScriptableObjects).
    /// Provides centralized access to ingredients, recipes, containers, etc.
    /// </summary>
    public interface IDataService : IService
    {
        // Ingredient Data
        IngredientData GetIngredient(string id);
        bool TryGetIngredient(string id, out IngredientData ingredient);
        IReadOnlyList<IngredientData> GetAllIngredients();
        IReadOnlyList<IngredientData> GetIngredientsByCategory(IngredientCategory category);

        // Container Data
        ContainerData GetContainer(string id);
        bool TryGetContainer(string id, out ContainerData container);
        IReadOnlyList<ContainerData> GetAllContainers();

        // Make Table Data
        MakeTableData GetMakeTable(string id);
        bool TryGetMakeTable(string id, out MakeTableData table);
        IReadOnlyList<MakeTableData> GetAllMakeTables();

        // Oven Data
        OvenData GetOven(string id);
        bool TryGetOven(string id, out OvenData oven);
        IReadOnlyList<OvenData> GetAllOvens();

        // Utility
        bool IsDataLoaded { get; }
        void ReloadAllData();
    }
}