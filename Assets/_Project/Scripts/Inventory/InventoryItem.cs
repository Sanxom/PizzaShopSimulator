using UnityEngine;

namespace PizzaShop.Inventory
{
    /// <summary>
    /// Represents an item that can be held in player inventory.
    /// </summary>
    [System.Serializable]
    public class InventoryItem
    {
        public string itemID;
        public string itemName;
        public ItemType itemType;
        public GameObject visualPrefab;
        public int stackSize;

        public InventoryItem(string id, string name, ItemType type)
        {
            itemID = id;
            itemName = name;
            itemType = type;
            stackSize = 1;
        }
    }

    public enum ItemType
    {
        None,
        Ingredient,
        Container,
        Tool,
        Pizza
    }
}