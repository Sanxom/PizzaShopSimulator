using DG.Tweening;
using PizzaShop.Core;
using System.Collections.Generic;
using UnityEngine;

namespace PizzaShop.Orders.UI
{
    public class OrderListUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private Transform orderPanelContainer;
        [SerializeField] private OrderPanel orderPanelPrefab;

        [Header("Layout")]
        [SerializeField] private float panelSpacing = 10f;
        [SerializeField] private int maxVisibleOrders = 3;

        private List<OrderPanel> orderPanels = new();
        private Dictionary<string, OrderPanel> orderPanelMap = new();

        private void Awake()
        {
            if (orderManager == null)
            {
                orderManager = FindFirstObjectByType<OrderManager>();
                if (orderManager == null)
                {
                    Debug.LogError("OrderListUI: OrderManager not found!");
                    enabled = false;
                    return;
                }
            }
        }

        private void OnEnable()
        {
            EventBus.OnOrderReceived += HandleOrderReceived;
            EventBus.OnOrderCompleted += HandleOrderCompleted;
            EventBus.OnOrderExpired += HandleOrderExpired;
        }

        private void OnDisable()
        {
            EventBus.OnOrderReceived -= HandleOrderReceived;
            EventBus.OnOrderCompleted -= HandleOrderCompleted;
            EventBus.OnOrderExpired -= HandleOrderExpired;
        }

        private void Start()
        {
            // Initialize with any existing orders
            RefreshAllOrders();
        }

        private void HandleOrderReceived(Order order)
        {
            CreateOrderPanel(order);
        }

        private void HandleOrderCompleted(Order order, int payment)
        {
            if (orderPanelMap.TryGetValue(order.OrderID, out OrderPanel panel))
            {
                panel.MarkCompleted();

                // Remove from tracking after animation
                DOTween.Sequence()
                    .AppendInterval(1.5f)
                    .AppendCallback(() => RemoveOrderPanel(order.OrderID));
            }
        }

        private void HandleOrderExpired(Order order)
        {
            if (orderPanelMap.TryGetValue(order.OrderID, out OrderPanel panel))
            {
                panel.MarkExpired();

                // Remove from tracking after animation
                DOTween.Sequence()
                    .AppendInterval(1.5f)
                    .AppendCallback(() => RemoveOrderPanel(order.OrderID));
            }
        }

        private void CreateOrderPanel(Order order)
        {
            // Check if we're at max capacity
            if (orderPanels.Count >= maxVisibleOrders)
            {
                Debug.LogWarning("OrderListUI: Maximum visible orders reached");
                return;
            }

            // Check if panel already exists
            if (orderPanelMap.ContainsKey(order.OrderID))
            {
                Debug.LogWarning($"OrderListUI: Panel for {order.OrderID} already exists");
                return;
            }

            OrderPanel panel = Instantiate(orderPanelPrefab, orderPanelContainer);
            panel.SetOrder(order);

            orderPanels.Add(panel);
            orderPanelMap.Add(order.OrderID, panel);

            UpdateLayout();
        }

        private void RemoveOrderPanel(string orderID)
        {
            if (orderPanelMap.TryGetValue(orderID, out OrderPanel panel))
            {
                orderPanels.Remove(panel);
                orderPanelMap.Remove(orderID);
                Destroy(panel.gameObject);

                UpdateLayout();
            }
        }

        private void UpdateLayout()
        {
            // Simple vertical layout
            float yOffset = 0f;

            foreach (var panel in orderPanels)
            {
                RectTransform rectTransform = panel.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(0f, -yOffset);

                yOffset += rectTransform.sizeDelta.y + panelSpacing;
            }
        }

        public void RefreshAllOrders()
        {
            // Clear existing panels
            foreach (var panel in orderPanels)
            {
                if (panel != null)
                    Destroy(panel.gameObject);
            }

            orderPanels.Clear();
            orderPanelMap.Clear();

            // Create panels for all active orders
            if (orderManager != null)
            {
                foreach (var order in orderManager.ActiveOrders)
                {
                    CreateOrderPanel(order);
                }
            }
        }

        public OrderPanel GetPanelForOrder(string orderID)
        {
            orderPanelMap.TryGetValue(orderID, out OrderPanel panel);
            return panel;
        }

        public void ClearAll()
        {
            foreach (var panel in orderPanels)
            {
                if (panel != null)
                    panel.Clear();
            }
        }
    }
}