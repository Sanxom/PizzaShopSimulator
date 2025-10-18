namespace PizzaShop.Core
{
    /// <summary>
    /// Base interface for all services in the game.
    /// Services are registered with the ServiceLocator and can be accessed globally.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Called when the service is registered with the ServiceLocator.
        /// Initialize resources, subscribe to events, etc.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Called when the service is unregistered or the game is shutting down.
        /// Clean up resources, unsubscribe from events, etc.
        /// </summary>
        void Shutdown();
    }
}