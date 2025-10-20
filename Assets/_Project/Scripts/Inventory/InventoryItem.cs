using UnityEngine;

namespace PizzaShop.Inventory
{
    /// <summary>
    /// Represents an item in the player's inventory.
    /// Can be an ingredient, container, pizza, or tool.
    /// Stores only the ID - actual data retrieved from DataService.
    /// </summary>
    [System.Serializable]
    public class InventoryItem
    {
        public string itemID;
        public string itemName;
        public ItemType itemType;
        public GameObject visualPrefab;

        // Properties for cleaner access
        public string ItemID => itemID;
        public string ItemName => itemName;
        public ItemType Type => itemType;

        public InventoryItem(string id, string name, ItemType type)
        {
            itemID = id;
            itemName = name;
            itemType = type;
        }
    }

    public enum ItemType
    {
        Ingredient,
        Container,
        Pizza,
        Tool
    }
}