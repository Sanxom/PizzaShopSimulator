using System;
using UnityEngine;

namespace PizzaShop.Core
{
    /// <summary>
    /// Global event bus for decoupled communication between systems.
    /// Uses C# events to implement the Observer pattern.
    /// </summary>
    public static class EventBus
    {
        // ==================== INGREDIENT EVENTS ====================
        /// <summary>Fired when player picks up an ingredient from a container.</summary>
        public static event Action<IngredientData> OnIngredientPickedUp;

        /// <summary>Fired when an ingredient is used (placed on pizza, etc).</summary>
        public static event Action<IngredientData> OnIngredientUsed;

        /// <summary>Fired when an ingredient is dropped or wasted.</summary>
        public static event Action<IngredientData> OnIngredientDropped;

        // ==================== CONTAINER EVENTS ====================
        /// <summary>Fired when a container is assigned to a specific ingredient type.</summary>
        public static event Action<Container, IngredientData> OnContainerAssigned;

        /// <summary>Fired when a container is refilled with servings.</summary>
        public static event Action<Container, int> OnContainerRefilled;

        /// <summary>Fired when a serving is taken from a container.</summary>
        public static event Action<Container, IngredientData> OnServingTaken;

        /// <summary>Fired when a container is reset to generic state.</summary>
        public static event Action<Container> OnContainerReset;

        /// <summary>Fired when player picks up a container.</summary>
        public static event Action<Container> OnContainerPickedUp;

        /// <summary>Fired when a container is placed on a slot.</summary>
        public static event Action<Container, ContainerSlot> OnContainerPlaced;

        // ==================== ORDER EVENTS ====================
        /// <summary>Fired when a new order is generated.</summary>
        public static event Action<Order> OnOrderReceived;

        /// <summary>Fired when an order is completed successfully.</summary>
        public static event Action<Order, int> OnOrderCompleted; // order, payment

        /// <summary>Fired when an order timer expires.</summary>
        public static event Action<Order> OnOrderExpired;

        /// <summary>Fired every frame an order timer updates.</summary>
        public static event Action<Order, float> OnOrderTimerUpdated; // order, time remaining

        // ==================== PIZZA EVENTS ====================
        /// <summary>Fired when pizza assembly starts (dough placed).</summary>
        public static event Action<Pizza> OnPizzaStarted;

        /// <summary>Fired when an ingredient is added to a pizza.</summary>
        public static event Action<Pizza, IngredientData> OnIngredientAddedToPizza;

        /// <summary>Fired when a pizza is marked as complete and ready.</summary>
        public static event Action<Pizza> OnPizzaCompleted;

        /// <summary>Fired when a pizza assembly is cancelled.</summary>
        public static event Action<Pizza> OnPizzaCancelled;

        /// <summary>Fired when player picks up a completed pizza.</summary>
        public static event Action<Pizza> OnPizzaPickedUp;

        // ==================== COOKING EVENTS ====================
        /// <summary>Fired when a pizza is placed in an oven.</summary>
        public static event Action<Pizza, Oven> OnPizzaPlacedInOven;

        /// <summary>Fired when a pizza finishes cooking with quality rating.</summary>
        public static event Action<Pizza, CookQuality> OnPizzaCooked;

        /// <summary>Fired when a pizza burns from overcooking.</summary>
        public static event Action<Pizza> OnPizzaBurnt;

        /// <summary>Fired when a pizza is removed from an oven.</summary>
        public static event Action<Pizza, Oven> OnPizzaRemovedFromOven;

        // ==================== ECONOMY EVENTS ====================
        /// <summary>Fired when money amount changes.</summary>
        public static event Action<int, int> OnMoneyChanged; // old amount, new amount

        /// <summary>Fired when money is added.</summary>
        public static event Action<int> OnMoneyAdded;

        /// <summary>Fired when money is spent.</summary>
        public static event Action<int> OnMoneySpent;

        /// <summary>Fired when an item is purchased.</summary>
        public static event Action<string, int> OnPurchase; // item id, cost

        // ==================== PROGRESSION EVENTS ====================
        /// <summary>Fired when an item is unlocked.</summary>
        public static event Action<UnlockData> OnItemUnlocked;

        /// <summary>Fired when an upgrade is purchased.</summary>
        public static event Action<string, int> OnUpgradePurchased; // upgrade id, level

        /// <summary>Fired when a day is completed.</summary>
        public static event Action<int> OnDayCompleted; // day number

        /// <summary>Fired when a shift starts.</summary>
        public static event Action<int> OnShiftStarted; // shift number

        /// <summary>Fired when a shift ends.</summary>
        public static event Action<int> OnShiftEnded; // shift number

        // ==================== UI EVENTS ====================
        /// <summary>Fired when a notification should be shown.</summary>
        public static event Action<string, NotificationType> OnNotificationShown;

        /// <summary>Fired when a tooltip should be shown.</summary>
        public static event Action<string> OnTooltipShown;

        /// <summary>Fired when tooltip should be hidden.</summary>
        public static event Action OnTooltipHidden;

        /// <summary>Fired when a UI panel is opened.</summary>
        public static event Action<PanelType> OnPanelOpened;

        /// <summary>Fired when a UI panel is closed.</summary>
        public static event Action<PanelType> OnPanelClosed;

        // ==================== PLAYER EVENTS ====================
        /// <summary>Fired when player state changes.</summary>
        public static event Action<PlayerState> OnPlayerStateChanged;

        /// <summary>Fired when player moves.</summary>
        public static event Action<Vector3> OnPlayerMoved;

        /// <summary>Fired when player interacts with something.</summary>
        public static event Action OnPlayerInteracted;

        // ==================== GAME EVENTS ====================
        /// <summary>Fired when game is paused.</summary>
        public static event Action OnGamePaused;

        /// <summary>Fired when game is resumed.</summary>
        public static event Action OnGameResumed;

        /// <summary>Fired when game starts.</summary>
        public static event Action OnGameStarted;

        /// <summary>Fired when game ends.</summary>
        public static event Action OnGameEnded;

        // ==================== DEBUG EVENTS ====================
        /// <summary>Fired for debug logging.</summary>
        public static event Action<string> OnDebugLog;

        // ==================== SAFE INVOCATION METHODS ====================
        /// <summary>
        /// Safely invoke an event with exception handling.
        /// </summary>
        private static void SafeInvoke(Action action, string eventName)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[EventBus] Error in {eventName}: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Safely invoke an event with one parameter.
        /// </summary>
        private static void SafeInvoke<T>(Action<T> action, T param, string eventName)
        {
            try
            {
                action?.Invoke(param);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EventBus] Error in {eventName}: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Safely invoke an event with two parameters.
        /// </summary>
        private static void SafeInvoke<T1, T2>(Action<T1, T2> action, T1 param1, T2 param2, string eventName)
        {
            try
            {
                action?.Invoke(param1, param2);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EventBus] Error in {eventName}: {e.Message}\n{e.StackTrace}");
            }
        }

        // ==================== PUBLIC INVOCATION METHODS ====================
        public static void RaisePlayerMoved(Vector3 position)
            => SafeInvoke(OnPlayerMoved, position, nameof(OnPlayerMoved));

        public static void RaisePlayerInteracted()
            => SafeInvoke(OnPlayerInteracted, nameof(OnPlayerInteracted));

        public static void RaiseIngredientPickedUp(IngredientData ingredient)
            => SafeInvoke(OnIngredientPickedUp, ingredient, nameof(OnIngredientPickedUp));

        public static void RaiseIngredientUsed(IngredientData ingredient)
            => SafeInvoke(OnIngredientUsed, ingredient, nameof(OnIngredientUsed));

        public static void RaiseOrderCompleted(Order order, int payment)
            => SafeInvoke(OnOrderCompleted, order, payment, nameof(OnOrderCompleted));

        public static void RaisePizzaStarted(Pizza pizza)
            => SafeInvoke(OnPizzaStarted, pizza, nameof(OnPizzaStarted));

        public static void RaiseMoneyChanged(int oldAmount, int newAmount)
            => SafeInvoke(OnMoneyChanged, oldAmount, newAmount, nameof(OnMoneyChanged));

        public static void RaiseGameStarted()
            => SafeInvoke(OnGameStarted, nameof(OnGameStarted));

        public static void RaiseGamePaused()
            => SafeInvoke(OnGamePaused, nameof(OnGamePaused));

        public static void RaiseGameResumed()
            => SafeInvoke(OnGameResumed, nameof(OnGameResumed));

        public static void RaiseGameEnded()
            => SafeInvoke(OnGameEnded, nameof(OnGameEnded));

        // ==================== UTILITY METHODS ====================
        /// <summary>
        /// Clear all event subscriptions. Use with caution!
        /// Typically called during scene transitions.
        /// </summary>
        public static void ClearAllEvents()
        {
            OnIngredientPickedUp = null;
            OnIngredientUsed = null;
            OnIngredientDropped = null;
            OnContainerAssigned = null;
            OnContainerRefilled = null;
            OnServingTaken = null;
            OnContainerReset = null;
            OnContainerPickedUp = null;
            OnContainerPlaced = null;
            OnOrderReceived = null;
            OnOrderCompleted = null;
            OnOrderExpired = null;
            OnOrderTimerUpdated = null;
            OnPizzaStarted = null;
            OnIngredientAddedToPizza = null;
            OnPizzaCompleted = null;
            OnPizzaCancelled = null;
            OnPizzaPickedUp = null;
            OnPizzaPlacedInOven = null;
            OnPizzaCooked = null;
            OnPizzaBurnt = null;
            OnPizzaRemovedFromOven = null;
            OnMoneyChanged = null;
            OnMoneyAdded = null;
            OnMoneySpent = null;
            OnPurchase = null;
            OnItemUnlocked = null;
            OnUpgradePurchased = null;
            OnDayCompleted = null;
            OnShiftStarted = null;
            OnShiftEnded = null;
            OnNotificationShown = null;
            OnTooltipShown = null;
            OnTooltipHidden = null;
            OnPanelOpened = null;
            OnPanelClosed = null;
            OnPlayerStateChanged = null;
            OnPlayerMoved = null;
            OnPlayerInteracted = null;
            OnGamePaused = null;
            OnGameResumed = null;
            OnGameStarted = null;
            OnGameEnded = null;
            OnDebugLog = null;

            Debug.Log("[EventBus] All events cleared");
        }
    }

    // ==================== SUPPORTING ENUMS ====================
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public enum PanelType
    {
        MainMenu,
        PauseMenu,
        Shop,
        Upgrades,
        Settings,
        OrderList
    }

    public enum PlayerState
    {
        Idle,
        Walking,
        HoldingItem,
        HoldingContainer,
        Interacting
    }

    public enum CookQuality
    {
        Raw,
        Undercooked,
        Perfect,
        Overcooked,
        Burnt
    }

    // Placeholder classes for compilation (will be implemented in later phases)
    public class IngredientData { }
    public class Container { }
    public class ContainerSlot { }
    public class Order { }
    public class Pizza { }
    public class Oven { }
    public class UnlockData { }
}