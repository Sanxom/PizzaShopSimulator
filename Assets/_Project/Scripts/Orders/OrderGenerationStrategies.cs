using System;
using System.Collections.Generic;
using UnityEngine;

namespace PizzaShop.Orders
{
    public interface IOrderGenerationStrategy
    {
        Order GenerateOrder(OrderConfig config);
    }

    public class WeightedRandomStrategy : IOrderGenerationStrategy
    {
        private int orderCounter = 0;

        public Order GenerateOrder(OrderConfig config)
        {
            orderCounter++;
            string orderId = $"ORD-{orderCounter:D4}";
            string customerName = config.GetRandomCustomerName();

            PizzaSize size = config.GetRandomSize();
            SauceType sauce = config.GetRandomSauce();
            bool cheese = config.GetRandomCheese();
            List<ToppingType> toppings = config.GetRandomToppings();
            CookLevel cookLevel = config.GetRandomCookLevel();

            int basePayment = config.GetBasePaymentForSize(size) + (toppings.Count * config.GetToppingPayment());
            float timeLimit = config.GetTimeLimitForSize(size);

            return new Order(orderId, customerName, size, sauce, cheese, toppings, cookLevel, basePayment, timeLimit);
        }
    }

    public class TrendBasedStrategy : IOrderGenerationStrategy
    {
        private int orderCounter = 0;
        private Dictionary<ToppingType, float> toppingTrends = new Dictionary<ToppingType, float>();
        private SauceType trendingSauce = SauceType.Marinara;
        private float trendTimer = 0f;
        private const float TREND_DURATION = 120f;

        public Order GenerateOrder(OrderConfig config)
        {
            orderCounter++;
            UpdateTrends();

            string orderId = $"ORD-{orderCounter:D4}";
            string customerName = config.GetRandomCustomerName();

            PizzaSize size = config.GetRandomSize();
            SauceType sauce = GetTrendingSauce(config);
            bool cheese = config.GetRandomCheese();
            List<ToppingType> toppings = GetTrendingToppings(config);
            CookLevel cookLevel = config.GetRandomCookLevel();

            int basePayment = config.GetBasePaymentForSize(size) + (toppings.Count * config.GetToppingPayment());
            float timeLimit = config.GetTimeLimitForSize(size);

            return new Order(orderId, customerName, size, sauce, cheese, toppings, cookLevel, basePayment, timeLimit);
        }

        private void UpdateTrends()
        {
            trendTimer += Time.deltaTime;

            if (trendTimer >= TREND_DURATION)
            {
                trendTimer = 0f;
                // Change trending sauce
                trendingSauce = (SauceType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(SauceType)).Length);
            }
        }

        private SauceType GetTrendingSauce(OrderConfig config)
        {
            // 60% chance to use trending sauce
            if (UnityEngine.Random.value < 0.6f)
                return trendingSauce;

            return config.GetRandomSauce();
        }

        private List<ToppingType> GetTrendingToppings(OrderConfig config)
        {
            List<ToppingType> toppings = config.GetRandomToppings();

            // 40% chance to add a "trending" topping
            if (UnityEngine.Random.value < 0.4f && toppings.Count < 4)
            {
                ToppingType trending = (ToppingType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(ToppingType)).Length);
                if (!toppings.Contains(trending))
                {
                    toppings.Add(trending);
                }
            }

            return toppings;
        }
    }

    public class SimpleOrderStrategy : IOrderGenerationStrategy
    {
        private int orderCounter = 0;

        public Order GenerateOrder(OrderConfig config)
        {
            orderCounter++;
            string orderId = $"ORD-{orderCounter:D4}";
            string customerName = config.GetRandomCustomerName();

            // Simple orders - mostly cheese pizzas with 1-2 toppings
            PizzaSize size = PizzaSize.Medium;
            SauceType sauce = SauceType.Marinara;
            bool cheese = true;

            List<ToppingType> toppings = new List<ToppingType>();
            if (UnityEngine.Random.value < 0.7f)
            {
                toppings.Add(ToppingType.Pepperoni);
            }

            CookLevel cookLevel = CookLevel.WellDone;

            int basePayment = config.GetBasePaymentForSize(size) + (toppings.Count * config.GetToppingPayment());
            float timeLimit = config.GetTimeLimitForSize(size) * 1.5f; // More time for beginners

            return new Order(orderId, customerName, size, sauce, cheese, toppings, cookLevel, basePayment, timeLimit);
        }
    }
}