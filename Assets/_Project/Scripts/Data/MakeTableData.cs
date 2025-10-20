using UnityEngine;

namespace PizzaShop.Data
{
    /// <summary>
    /// Configuration data for make tables.
    /// Defines grid layout and assembly zones.
    /// </summary>
    [CreateAssetMenu(fileName = "MakeTableData", menuName = "PizzaShop/Data/MakeTable", order = 3)]
    public class MakeTableData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string tableID;
        [SerializeField] private string displayName;
        [TextArea(2, 4)]
        [SerializeField] private string description;

        [Header("Grid Configuration")]
        [SerializeField] private int gridWidth = 4;
        [SerializeField] private int gridDepth = 3;
        [SerializeField] private float slotSpacing = 0.5f;
        [SerializeField] private Vector3 gridOffset = Vector3.zero;

        [Header("Assembly Zones")]
        [SerializeField] private int assemblyZoneCount = 2;
        [SerializeField] private PizzaSize[] supportedSizes = { PizzaSize.Large };

        [Header("Visual")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private Sprite icon;

        [Header("Progression")]
        [SerializeField] private int unlockCost = 0;
        [SerializeField] private bool startsUnlocked = true;

        // Properties
        public string TableID => tableID;
        public string DisplayName => displayName;
        public string Description => description;
        public int GridWidth => gridWidth;
        public int GridDepth => gridDepth;
        public float SlotSpacing => slotSpacing;
        public Vector3 GridOffset => gridOffset;
        public int AssemblyZoneCount => assemblyZoneCount;
        public PizzaSize[] SupportedSizes => supportedSizes;
        public GameObject Prefab => prefab;
        public Sprite Icon => icon;
        public int UnlockCost => unlockCost;
        public bool StartsUnlocked => startsUnlocked;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(tableID))
            {
                tableID = name.ToLower().Replace(" ", "_");
            }

            gridWidth = Mathf.Max(2, gridWidth);
            gridDepth = Mathf.Max(2, gridDepth);
            slotSpacing = Mathf.Max(0.1f, slotSpacing);
            assemblyZoneCount = Mathf.Max(1, assemblyZoneCount);
            unlockCost = Mathf.Max(0, unlockCost);
        }
    }

    public enum PizzaSize
    {
        Small,
        Medium,
        Large,
        XLarge
    }
}