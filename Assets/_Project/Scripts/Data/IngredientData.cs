using UnityEngine;

namespace PizzaShop.Data
{
    /// <summary>
    /// Configuration data for ingredients.
    /// Defines properties like color, category, price.
    /// </summary>
    [CreateAssetMenu(fileName = "IngredientData", menuName = "PizzaShop/Data/Ingredient", order = 2)]
    public class IngredientData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string ingredientID;
        [SerializeField] private string displayName;
        [SerializeField] private IngredientCategory category;

        [Header("Visual")]
        [SerializeField] private Color ingredientColor = Color.white;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject visualPrefab;

        [Header("Gameplay")]
        [SerializeField] private int basePrice = 10;
        [SerializeField] private bool startsUnlocked = true;

        // Properties
        public string IngredientID => ingredientID;
        public string DisplayName => displayName;
        public IngredientCategory Category => category;
        public Color IngredientColor => ingredientColor;
        public Sprite Icon => icon;
        public GameObject VisualPrefab => visualPrefab;
        public int BasePrice => basePrice;
        public bool StartsUnlocked => startsUnlocked;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ingredientID))
            {
                ingredientID = name.ToLower().Replace(" ", "_");
            }
        }
    }

    public enum IngredientCategory
    {
        Base,      // Dough
        Sauce,     // Marinara, Alfredo, etc.
        Cheese,    // Mozzarella, etc.
        Topping,   // Pepperoni, Mushrooms, etc.
    }
}