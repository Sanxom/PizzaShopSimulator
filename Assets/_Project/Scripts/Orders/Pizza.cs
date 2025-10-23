using System.Collections.Generic;
using UnityEngine;

namespace PizzaShop.Orders
{
    public class Pizza
    {
        public string PizzaID { get; private set; }
        public PizzaSize Size { get; private set; }
        public bool HasDough { get; set; }
        public SauceType Sauce { get; set; }
        public bool HasCheese { get; set; }
        public List<ToppingType> Toppings { get; private set; }
        public CookLevel CookLevel { get; set; }
        public PizzaState State { get; set; }

        public Pizza(PizzaSize size)
        {
            PizzaID = System.Guid.NewGuid().ToString();
            Size = size;
            HasDough = false;
            Sauce = SauceType.Marinara;
            HasCheese = false;
            Toppings = new List<ToppingType>();
            CookLevel = CookLevel.LightlyCooked;
            State = PizzaState.NoDough;
        }

        public void AddTopping(ToppingType topping)
        {
            if (!Toppings.Contains(topping))
            {
                Toppings.Add(topping);
                UpdateState();
            }
        }

        public void RemoveTopping(ToppingType topping)
        {
            if (Toppings.Remove(topping))
            {
                UpdateState();
            }
        }

        public bool IsComplete()
        {
            return HasDough && State >= PizzaState.Cooked;
        }

        private void UpdateState()
        {
            if (!HasDough)
            {
                State = PizzaState.NoDough;
            }
            else if (Sauce == SauceType.Marinara && !HasCheese && Toppings.Count == 0)
            {
                State = PizzaState.DoughOnly;
            }
            else if (HasCheese || Toppings.Count > 0)
            {
                State = PizzaState.ReadyToCook;
            }
        }

        public string GetDescription()
        {
            string desc = $"{Size} Pizza";

            if (!HasDough)
                return desc + " (No dough)";

            desc += $" with {Sauce} sauce";

            if (HasCheese)
                desc += ", cheese";

            if (Toppings.Count > 0)
            {
                desc += ", " + string.Join(", ", Toppings);
            }

            if (State >= PizzaState.Cooked)
            {
                desc += $" ({CookLevel})";
            }

            return desc;
        }
    }

    public enum PizzaState
    {
        NoDough,
        DoughOnly,
        HasSauce,
        HasCheese,
        HasToppings,
        ReadyToCook,
        Cooking,
        Cooked,
        Burnt
    }
}