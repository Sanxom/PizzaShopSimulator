using UnityEngine;

namespace PizzaShop.Data
{
    /// <summary>
    /// Configuration data for ovens.
    /// Defines cooking times, temperature, and capacity.
    /// </summary>
    [CreateAssetMenu(fileName = "OvenData", menuName = "PizzaShop/Data/Oven", order = 4)]
    public class OvenData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string ovenID;
        [SerializeField] private string displayName;
        [TextArea(2, 4)]
        [SerializeField] private string description;

        [Header("Capacity")]
        [SerializeField] private int maxPizzas = 2;
        [SerializeField] private PizzaSize[] supportedSizes = { PizzaSize.Small, PizzaSize.Medium, PizzaSize.Large };

        [Header("Cooking Times (seconds)")]
        [SerializeField] private CookingProfile smallPizzaProfile = new CookingProfile(30f, 45f);
        [SerializeField] private CookingProfile mediumPizzaProfile = new CookingProfile(45f, 60f);
        [SerializeField] private CookingProfile largePizzaProfile = new CookingProfile(60f, 75f);
        [SerializeField] private CookingProfile xLargePizzaProfile = new CookingProfile(75f, 90f);

        [Header("Temperature")]
        [SerializeField] private float cookingTemperature = 450f; // Fahrenheit
        [SerializeField] private float heatUpTime = 5f; // Time to reach temperature

        [Header("Visual")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private Sprite icon;
        [SerializeField] private Color heatingColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color cookingColor = new Color(1f, 0.3f, 0f);

        [Header("Progression")]
        [SerializeField] private int unlockCost = 500;
        [SerializeField] private bool startsUnlocked = true;

        // Properties
        public string OvenID => ovenID;
        public string DisplayName => displayName;
        public string Description => description;
        public int MaxPizzas => maxPizzas;
        public PizzaSize[] SupportedSizes => supportedSizes;
        public float CookingTemperature => cookingTemperature;
        public float HeatUpTime => heatUpTime;
        public GameObject Prefab => prefab;
        public Sprite Icon => icon;
        public Color HeatingColor => heatingColor;
        public Color CookingColor => cookingColor;
        public int UnlockCost => unlockCost;
        public bool StartsUnlocked => startsUnlocked;

        /// <summary>
        /// Get cooking profile for specific pizza size.
        /// </summary>
        public CookingProfile GetProfile(PizzaSize size)
        {
            return size switch
            {
                PizzaSize.Small => smallPizzaProfile,
                PizzaSize.Medium => mediumPizzaProfile,
                PizzaSize.Large => largePizzaProfile,
                PizzaSize.XLarge => xLargePizzaProfile,
                _ => largePizzaProfile
            };
        }

        /// <summary>
        /// Check if oven supports pizza size.
        /// </summary>
        public bool SupportsSize(PizzaSize size)
        {
            foreach (var supportedSize in supportedSizes)
            {
                if (supportedSize == size)
                    return true;
            }
            return false;
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ovenID))
            {
                ovenID = name.ToLower().Replace(" ", "_");
            }

            maxPizzas = Mathf.Max(1, maxPizzas);
            cookingTemperature = Mathf.Max(0f, cookingTemperature);
            heatUpTime = Mathf.Max(0f, heatUpTime);
            unlockCost = Mathf.Max(0, unlockCost);
        }
    }

    /// <summary>
    /// Cooking profile for a specific pizza size.
    /// Defines perfect cooking time and burn time.
    /// </summary>
    [System.Serializable]
    public class CookingProfile
    {
        [Tooltip("Time in seconds to reach perfect cook quality")]
        public float perfectTime = 60f;

        [Tooltip("Time in seconds before pizza burns")]
        public float burnTime = 75f;

        public CookingProfile(float perfect, float burn)
        {
            perfectTime = perfect;
            burnTime = burn;
        }

        /// <summary>
        /// Get cook quality based on elapsed time.
        /// </summary>
        public CookQuality GetQuality(float elapsedTime)
        {
            if (elapsedTime < perfectTime * 0.5f)
                return CookQuality.Raw;

            if (elapsedTime < perfectTime * 0.8f)
                return CookQuality.Undercooked;

            if (elapsedTime < perfectTime * 1.2f)
                return CookQuality.Perfect;

            if (elapsedTime < burnTime)
                return CookQuality.Overcooked;

            return CookQuality.Burnt;
        }

        /// <summary>
        /// Get normalized progress (0-1) through cooking.
        /// </summary>
        public float GetProgress(float elapsedTime)
        {
            return Mathf.Clamp01(elapsedTime / perfectTime);
        }
    }

    public enum CookQuality
    {
        Raw,
        Undercooked,
        Perfect,
        Overcooked,
        Burnt
    }
}