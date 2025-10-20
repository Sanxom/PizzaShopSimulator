using System;
using System.Collections.Generic;
using PizzaShop.Data;

namespace PizzaShop.Orders
{
    /// <summary>
    /// Represents a customer order for a pizza.
    /// </summary>
    [Serializable]
    public class Order
    {
        public string OrderID { get; set; }
        public PizzaSize Size { get; set; }
        public List<IngredientData> RequiredIngredients { get; set; }
        public int BasePayment { get; set; }
        public float TimeLimit { get; set; }
        public float TimeRemaining { get; set; }
        public bool IsExpired => TimeRemaining <= 0f;

        public Order()
        {
            OrderID = Guid.NewGuid().ToString();
            RequiredIngredients = new List<IngredientData>();
        }
    }
}