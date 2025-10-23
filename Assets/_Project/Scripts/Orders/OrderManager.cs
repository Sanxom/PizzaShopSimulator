using PizzaShop.Core;
using PizzaShop.Food;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PizzaShop.Orders
{
    public class OrderManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private OrderConfig config;
        [SerializeField] private OrderGenerationMode startingMode = OrderGenerationMode.WeightedRandom;

        [Header("Debug")]
        [SerializeField] private bool autoGenerate = true;
        [SerializeField] private bool showDebugLogs = true;

        private IOrderGenerationStrategy generationStrategy;
        private List<Order> activeOrders = new List<Order>();
        private List<Order> completedOrders = new List<Order>();
        private List<Order> expiredOrders = new List<Order>();

        private float nextOrderTime;
        private float gameStartTime;
        private int totalOrdersGenerated = 0;
        private int totalMoneyEarned = 0;

        public IReadOnlyList<Order> ActiveOrders => activeOrders.AsReadOnly();
        public IReadOnlyList<Order> CompletedOrders => completedOrders.AsReadOnly();
        public IReadOnlyList<Order> ExpiredOrders => expiredOrders.AsReadOnly();

        public int TotalOrdersGenerated => totalOrdersGenerated;
        public int TotalMoneyEarned => totalMoneyEarned;

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError("OrderManager: OrderConfig is not assigned!");
                enabled = false;
                return;
            }

            SetGenerationMode(startingMode);
        }

        private void Start()
        {
            gameStartTime = Time.time;
            ScheduleNextOrder();

            if (showDebugLogs)
                Debug.Log("OrderManager: Started");
        }

        private void Update()
        {
            UpdateActiveOrders();

            if (autoGenerate && Time.time >= nextOrderTime)
            {
                if (activeOrders.Count < config.MaxActiveOrders)
                {
                    GenerateOrder();
                }
                ScheduleNextOrder();
            }
        }

        private void UpdateActiveOrders()
        {
            for (int i = activeOrders.Count - 1; i >= 0; i--)
            {
                Order order = activeOrders[i];
                order.UpdateTimer(Time.deltaTime);

                EventBus.RaiseOrderTimerUpdated(order, order.TimeRemaining);

                if (order.State == OrderState.Expired)
                {
                    ExpireOrder(order);
                }
            }
        }

        public void GenerateOrder()
        {
            if (activeOrders.Count >= config.MaxActiveOrders)
            {
                if (showDebugLogs)
                    Debug.LogWarning("OrderManager: Cannot generate order - max active orders reached");
                return;
            }

            Order newOrder = generationStrategy.GenerateOrder(config);
            activeOrders.Add(newOrder);
            totalOrdersGenerated++;

            EventBus.RaiseOrderReceived(newOrder);

            if (showDebugLogs)
                Debug.Log($"OrderManager: Generated {newOrder.OrderID} for {newOrder.CustomerName} - {newOrder.GetDescription()}");
        }

        public bool ValidateOrder(Pizza pizza, Order order)
        {
            if (!activeOrders.Contains(order))
            {
                if (showDebugLogs)
                    Debug.LogWarning($"OrderManager: Order {order.OrderID} is not active");
                return false;
            }

            bool isValid = order.MatchesPizza(pizza);

            if (isValid)
            {
                CompleteOrder(order);
            }
            else
            {
                if (showDebugLogs)
                    Debug.LogWarning($"OrderManager: Pizza does not match order {order.OrderID}");
            }

            return isValid;
        }

        private void CompleteOrder(Order order)
        {
            if (!activeOrders.Remove(order))
            {
                if (showDebugLogs)
                    Debug.LogWarning($"OrderManager: Order {order.OrderID} was not in active orders");
                return;
            }

            order.Complete();
            completedOrders.Add(order);

            int payment = order.GetCurrentPayment();
            totalMoneyEarned += payment;

            EventBus.RaiseOrderCompleted(order, payment);
            EventBus.RaiseMoneyAdded(payment);

            if (showDebugLogs)
                Debug.Log($"OrderManager: Completed {order.OrderID} - Earned ${payment}");
        }

        private void ExpireOrder(Order order)
        {
            if (!activeOrders.Remove(order))
                return;

            order.Expire();
            expiredOrders.Add(order);

            EventBus.RaiseOrderExpired(order);

            if (showDebugLogs)
                Debug.Log($"OrderManager: Order {order.OrderID} expired");
        }

        public void CancelOrder(Order order)
        {
            if (activeOrders.Remove(order))
            {
                expiredOrders.Add(order);
                EventBus.RaiseOrderExpired(order);

                if (showDebugLogs)
                    Debug.Log($"OrderManager: Manually cancelled {order.OrderID}");
            }
        }

        private void ScheduleNextOrder()
        {
            float gameTime = Time.time - gameStartTime;
            float interval = config.GetScaledOrderInterval(gameTime);
            nextOrderTime = Time.time + interval;

            if (showDebugLogs)
                Debug.Log($"OrderManager: Next order in {interval:F1} seconds");
        }

        public void SetGenerationMode(OrderGenerationMode mode)
        {
            generationStrategy = mode switch
            {
                OrderGenerationMode.WeightedRandom => new WeightedRandomStrategy(),
                OrderGenerationMode.TrendBased => new TrendBasedStrategy(),
                OrderGenerationMode.Simple => new SimpleOrderStrategy(),
                _ => new WeightedRandomStrategy()
            };

            if (showDebugLogs)
                Debug.Log($"OrderManager: Set generation mode to {mode}");
        }

        public Order GetOrderByID(string orderId)
        {
            return activeOrders.FirstOrDefault(o => o.OrderID == orderId);
        }

        public void ClearExpiredOrders()
        {
            expiredOrders.Clear();
        }

        public void ClearCompletedOrders()
        {
            completedOrders.Clear();
        }

        public void ResetStats()
        {
            totalOrdersGenerated = 0;
            totalMoneyEarned = 0;
            activeOrders.Clear();
            completedOrders.Clear();
            expiredOrders.Clear();
            gameStartTime = Time.time;
        }

        // Debug methods
        [ContextMenu("Generate Test Order")]
        public void GenerateTestOrder()
        {
            GenerateOrder();
        }

        [ContextMenu("Complete First Order")]
        public void CompleteFirstOrder()
        {
            if (activeOrders.Count > 0)
            {
                CompleteOrder(activeOrders[0]);
            }
        }

        [ContextMenu("Expire All Orders")]
        public void ExpireAllOrders()
        {
            while (activeOrders.Count > 0)
            {
                ExpireOrder(activeOrders[0]);
            }
        }
    }

    public enum OrderGenerationMode
    {
        WeightedRandom,
        TrendBased,
        Simple
    }
}