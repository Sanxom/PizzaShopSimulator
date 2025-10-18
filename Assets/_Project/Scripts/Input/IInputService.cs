using UnityEngine;

namespace PizzaShop.Input
{
    public interface IInputService : Core.IService
    {
        Vector2 MovementInput { get; }
        Vector2 LookInput { get; }
        bool IsInteracting { get; }
        bool IsAlternateInteracting { get; }
        bool IsSprinting { get; }
        bool IsCancelPressed { get; }

        void EnablePlayerInput();
        void DisablePlayerInput();
        void EnableUIInput();
        void DisableUIInput();
    }
}