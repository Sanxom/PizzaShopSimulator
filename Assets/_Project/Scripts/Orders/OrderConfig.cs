using System.Collections.Generic;
using UnityEngine;

namespace PizzaShop.Orders
{
    [CreateAssetMenu(fileName = "OrderConfig", menuName = "PizzaShop/Order Configuration")]
    public class OrderConfig : ScriptableObject
    {
        [Header("Order Generation")]
        [SerializeField] private int maxActiveOrders = 3;
        [SerializeField] private float baseOrderInterval = 30f;
        [SerializeField] private float orderIntervalVariance = 10f;

        [Header("Time Limits")]
        [SerializeField] private float smallPizzaTime = 120f;
        [SerializeField] private float mediumPizzaTime = 150f;
        [SerializeField] private float largePizzaTime = 180f;

        [Header("Payment")]
        [SerializeField] private int smallPizzaPayment = 10;
        [SerializeField] private int mediumPizzaPayment = 15;
        [SerializeField] private int largePizzaPayment = 20;
        [SerializeField] private int toppingPayment = 2;

        [Header("Probabilities")]
        [SerializeField] private WeightedItem<PizzaSize>[] sizeWeights;
        [SerializeField] private WeightedItem<SauceType>[] sauceWeights;
        [SerializeField] private WeightedItem<ToppingType>[] toppingWeights;
        [SerializeField] private WeightedItem<CookLevel>[] cookLevelWeights;
        [SerializeField, Range(0f, 1f)] private float cheeseChance = 0.8f;
        [SerializeField, Range(0, 5)] private int minToppings = 0;
        [SerializeField, Range(1, 8)] private int maxToppings = 4;

        [Header("Customer Names")]
        [SerializeField]
        private string[] customerNames = new string[]
        {
            "John", "Sarah", "Mike", "Emily", "David", "Lisa",
            "Tom", "Anna", "Chris", "Mary", "Jake", "Sophie"
        };

        [Header("Difficulty Scaling")]
        [SerializeField] private bool enableScaling = true;
        [SerializeField] private float scalingStartTime = 300f;
        [SerializeField] private float maxDifficultyTime = 1200f;

        public int MaxActiveOrders => maxActiveOrders;
        public float BaseOrderInterval => baseOrderInterval;
        public float OrderIntervalVariance => orderIntervalVariance;

        public float GetTimeLimitForSize(PizzaSize size)
        {
            return size switch
            {
                PizzaSize.Small => smallPizzaTime,
                PizzaSize.Medium => mediumPizzaTime,
                PizzaSize.Large => largePizzaTime,
                _ => mediumPizzaTime
            };
        }

        public int GetBasePaymentForSize(PizzaSize size)
        {
            return size switch
            {
                PizzaSize.Small => smallPizzaPayment,
                PizzaSize.Medium => mediumPizzaPayment,
                PizzaSize.Large => largePizzaPayment,
                _ => mediumPizzaPayment
            };
        }

        public int GetToppingPayment() => toppingPayment;

        public PizzaSize GetRandomSize()
        {
            return GetWeightedRandom(sizeWeights);
        }

        public SauceType GetRandomSauce()
        {
            return GetWeightedRandom(sauceWeights);
        }

        public CookLevel GetRandomCookLevel()
        {
            return GetWeightedRandom(cookLevelWeights);
        }

        public List<ToppingType> GetRandomToppings()
        {
            int count = Random.Range(minToppings, maxToppings + 1);
            List<ToppingType> toppings = new List<ToppingType>();

            for (int i = 0; i < count; i++)
            {
                ToppingType topping = GetWeightedRandom(toppingWeights);
                if (!toppings.Contains(topping))
                {
                    toppings.Add(topping);
                }
            }

            return toppings;
        }

        public bool GetRandomCheese()
        {
            return Random.value < cheeseChance;
        }

        public string GetRandomCustomerName()
        {
            return customerNames[Random.Range(0, customerNames.Length)];
        }

        public float GetScaledOrderInterval(float gameTime)
        {
            if (!enableScaling) return BaseOrderInterval;

            if (gameTime < scalingStartTime)
                return BaseOrderInterval;

            float t = Mathf.Clamp01((gameTime - scalingStartTime) / (maxDifficultyTime - scalingStartTime));
            float scaledInterval = Mathf.Lerp(BaseOrderInterval, BaseOrderInterval * 0.5f, t);

            return scaledInterval + Random.Range(-OrderIntervalVariance, OrderIntervalVariance);
        }

        private T GetWeightedRandom<T>(WeightedItem<T>[] items)
        {
            float totalWeight = 0f;
            foreach (var item in items)
            {
                totalWeight += item.weight;
            }

            float random = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var item in items)
            {
                cumulative += item.weight;
                if (random <= cumulative)
                {
                    return item.value;
                }
            }

            return items[0].value;
        }
    }

    [System.Serializable]
    public struct WeightedItem<T>
    {
        public T value;
        public float weight;
    }
}