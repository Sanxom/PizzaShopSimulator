using PizzaShop.Food;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PizzaShop.Orders
{
    [Serializable]
    public class Order
    {
        public string OrderID { get; private set; }
        public string CustomerName { get; private set; }
        public PizzaSize Size { get; private set; }
        public SauceType Sauce { get; private set; }
        public bool RequiresCheese { get; private set; }
        public List<ToppingType> Toppings { get; private set; }
        public CookLevel DesiredCookLevel { get; private set; }

        public int BasePayment { get; private set; }
        public int BonusPayment { get; private set; }
        public float TimeLimit { get; private set; }
        public float TimeRemaining { get; private set; }

        public OrderState State { get; private set; }
        public float CreatedAt { get; private set; }

        public Order(string orderId, string customerName, PizzaSize size,
                    SauceType sauce, bool cheese, List<ToppingType> toppings,
                    CookLevel cookLevel, int basePayment, float timeLimit)
        {
            OrderID = orderId;
            CustomerName = customerName;
            Size = size;
            Sauce = sauce;
            RequiresCheese = cheese;
            Toppings = new List<ToppingType>(toppings);
            DesiredCookLevel = cookLevel;
            BasePayment = basePayment;
            BonusPayment = Mathf.RoundToInt(basePayment * 0.5f);
            TimeLimit = timeLimit;
            TimeRemaining = timeLimit;
            State = OrderState.Active;
            CreatedAt = Time.time;
        }

        public void UpdateTimer(float deltaTime)
        {
            if (State != OrderState.Active) return;

            TimeRemaining -= deltaTime;

            if (TimeRemaining <= 0)
            {
                TimeRemaining = 0;
                State = OrderState.Expired;
            }
        }

        public float GetTimePercentage()
        {
            return TimeRemaining / TimeLimit;
        }

        public int GetCurrentPayment()
        {
            float timePercent = GetTimePercentage();

            if (timePercent > 0.75f)
                return BasePayment + BonusPayment;
            else if (timePercent > 0.5f)
                return BasePayment + Mathf.RoundToInt(BonusPayment * 0.5f);
            else if (timePercent > 0.25f)
                return BasePayment;
            else
                return Mathf.RoundToInt(BasePayment * 0.75f);
        }

        public void Complete()
        {
            State = OrderState.Completed;
        }

        public void Expire()
        {
            State = OrderState.Expired;
        }

        public bool MatchesPizza(Pizza pizza)
        {
            // Check size
            if (pizza.Size != Size) return false;

            // Check dough
            if (!pizza.HasDough) return false;

            // Check sauce
            if (pizza.Sauce != Sauce) return false;

            // Check cheese
            if (pizza.HasCheese != RequiresCheese) return false;

            // Check toppings (must have all required, no extras)
            if (pizza.Toppings.Count != Toppings.Count) return false;

            foreach (var topping in Toppings)
            {
                if (!pizza.Toppings.Contains(topping)) return false;
            }

            // Check cook level
            if (pizza.CookLevel != DesiredCookLevel) return false;

            return true;
        }

        public string GetDescription()
        {
            string desc = $"{Size} Pizza with {Sauce} sauce";

            if (RequiresCheese)
                desc += ", cheese";

            if (Toppings.Count > 0)
            {
                desc += ", " + string.Join(", ", Toppings);
            }

            desc += $" ({DesiredCookLevel})";

            return desc;
        }
    }

    public enum OrderState
    {
        Active,
        Completed,
        Expired
    }

    public enum CookLevel
    {
        LightlyCooked,
        WellDone,
        Crispy
    }

    public enum PizzaSize
    {
        Small,
        Medium,
        Large
    }

    public enum SauceType
    {
        Marinara,
        White,
        BBQ,
        Pesto
    }

    public enum ToppingType
    {
        Pepperoni,
        Mushrooms,
        Onions,
        Sausage,
        Bacon,
        Peppers,
        Olives,
        Pineapple
    }
}