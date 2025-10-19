using UnityEngine;

namespace PizzaShop.Data
{
    /// <summary>
    /// Immutable configuration data for container types.
    /// Stored as ScriptableObject for data-driven design.
    /// </summary>
    [CreateAssetMenu(fileName = "ContainerData", menuName = "PizzaShop/Data/Container", order = 1)]
    public class ContainerData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string containerID;
        [SerializeField] private string displayName;
        [TextArea(2, 4)]
        [SerializeField] private string description;

        [Header("Capacity")]
        [SerializeField] private int maxCapacity = 20;
        [SerializeField] private int initialServings = 0;

        [Header("Visual")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private Sprite icon;
        [SerializeField] private Color emptyColor = new Color(0.7f, 0.7f, 0.7f);

        [Header("Progression")]
        [SerializeField] private int unlockCost = 0;
        [SerializeField] private bool startsUnlocked = true;

        // Public read-only properties
        public string ContainerID => containerID;
        public string DisplayName => displayName;
        public string Description => description;
        public int MaxCapacity => maxCapacity;
        public int InitialServings => initialServings;
        public GameObject Prefab => prefab;
        public Sprite Icon => icon;
        public Color EmptyColor => emptyColor;
        public int UnlockCost => unlockCost;
        public bool StartsUnlocked => startsUnlocked;

        private void OnValidate()
        {
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(containerID))
            {
                containerID = name.ToLower().Replace(" ", "_");
            }

            // Clamp values
            maxCapacity = Mathf.Max(1, maxCapacity);
            initialServings = Mathf.Clamp(initialServings, 0, maxCapacity);
            unlockCost = Mathf.Max(0, unlockCost);
        }
    }
}