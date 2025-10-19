using PizzaShop.Data;

namespace PizzaShop.Equipment
{
    /// <summary>
    /// State pattern for container behavior.
    /// Different states handle interactions differently.
    /// </summary>
    public interface IContainerState
    {
        void Enter(Container container);
        void Exit(Container container);
        bool CanAddServing(Container container);
        bool CanTakeServing(Container container);
        void OnAddServing(Container container, IngredientData ingredient);
        void OnTakeServing(Container container);
        string GetStateDescription();
    }

    /// <summary>
    /// Container is empty and unassigned - accepts any ingredient.
    /// </summary>
    public class EmptyContainerState : IContainerState
    {
        public void Enter(Container container)
        {
            container.UpdateVisuals();
        }

        public void Exit(Container container) { }

        public bool CanAddServing(Container container) => true;
        public bool CanTakeServing(Container container) => false;

        public void OnAddServing(Container container, IngredientData ingredient)
        {
            container.AssignIngredient(ingredient);
            container.AddServing();
            container.TransitionToState(container.FillingState);
        }

        public void OnTakeServing(Container container)
        {
            // Can't take from empty
        }

        public string GetStateDescription() => "Empty Container";
    }

    /// <summary>
    /// Container is assigned and being filled/used.
    /// </summary>
    public class FillingContainerState : IContainerState
    {
        public void Enter(Container container)
        {
            container.UpdateVisuals();
        }

        public void Exit(Container container) { }

        public bool CanAddServing(Container container)
        {
            return container.CurrentServings < container.MaxCapacity;
        }

        public bool CanTakeServing(Container container)
        {
            return container.CurrentServings > 0;
        }

        public void OnAddServing(Container container, IngredientData ingredient)
        {
            // Validate same ingredient
            if (container.AssignedIngredient != null &&
                container.AssignedIngredient.IngredientID != ingredient.IngredientID)
            {
                UnityEngine.Debug.LogWarning($"[Container] Cannot add {ingredient.DisplayName} to {container.AssignedIngredient.DisplayName} container!");
                return;
            }

            container.AddServing();

            if (container.CurrentServings >= container.MaxCapacity)
            {
                container.TransitionToState(container.FullState);
            }
        }

        public void OnTakeServing(Container container)
        {
            container.RemoveServing();

            if (container.CurrentServings <= 0)
            {
                container.TransitionToState(container.EmptyState);
            }
        }

        public string GetStateDescription() => "Filling";
    }

    /// <summary>
    /// Container is at maximum capacity.
    /// </summary>
    public class FullContainerState : IContainerState
    {
        public void Enter(Container container)
        {
            container.UpdateVisuals();
        }

        public void Exit(Container container) { }

        public bool CanAddServing(Container container) => false;

        public bool CanTakeServing(Container container) => true;

        public void OnAddServing(Container container, IngredientData ingredient)
        {
            // Full - cannot add
        }

        public void OnTakeServing(Container container)
        {
            container.RemoveServing();
            container.TransitionToState(container.FillingState);
        }

        public string GetStateDescription() => "Full";
    }
}