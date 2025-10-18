using PizzaShop.Player;

namespace PizzaShop.Interaction
{
    /// <summary>
    /// Interface for all objects that can be interacted with by the player.
    /// Implements the Strategy pattern for different interaction behaviors.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Display name shown to player (e.g., "Cheese Container").
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Type of interaction (Instant, Hold, Toggle).
        /// </summary>
        InteractionType InteractionType { get; }

        /// <summary>
        /// Can the player currently interact with this object?
        /// </summary>
        bool CanInteract(PlayerController player);

        /// <summary>
        /// Primary interaction (E key).
        /// </summary>
        void OnInteract(PlayerController player);

        /// <summary>
        /// Alternate interaction (Q key).
        /// </summary>
        void OnAlternateInteract(PlayerController player);

        /// <summary>
        /// Get the interaction prompt text to display.
        /// Example: "Press E to pick up Dough"
        /// </summary>
        string GetInteractionPrompt(PlayerController player);

        /// <summary>
        /// For hold interactions, return progress (0 to 1).
        /// Return 0 for instant interactions.
        /// </summary>
        float GetInteractionProgress();

        /// <summary>
        /// Called when player starts looking at this object.
        /// Use for highlighting, outlining, etc.
        /// </summary>
        void OnLookEnter(PlayerController player);

        /// <summary>
        /// Called when player stops looking at this object.
        /// Use to remove highlighting.
        /// </summary>
        void OnLookExit(PlayerController player);
    }

    /// <summary>
    /// Types of interactions supported.
    /// </summary>
    public enum InteractionType
    {
        Instant,    // Single press (E)
        Hold,       // Hold E for duration
        Toggle      // Press E to start, E again to stop
    }
}