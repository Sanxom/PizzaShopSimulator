using UnityEngine;
using PizzaShop.Input;

namespace PizzaShop.Core
{
    /// <summary>
    /// Debug utilities for development.
    /// Only active in Debug builds.
    /// </summary>
    public class DebugManager : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugUI = true;

        private PlayerInputActions inputActions;
        private IInputService inputService;
        private bool isVisible = false;
        private GUIStyle labelStyle;

        private void Awake()
        {
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            gameObject.SetActive(false);
            return;
#endif

            // Initialize input actions for debug
            inputActions = new PlayerInputActions();
            inputActions.Debug.ToggleDebug.performed += _ => ToggleDebugUI();
        }

        private void OnEnable()
        {
            inputActions?.Debug.Enable();
        }

        private void OnDisable()
        {
            inputActions?.Debug.Disable();
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out inputService))
            {
                Debug.Log("[DebugManager] Initialized");
            }

            labelStyle = new GUIStyle
            {
                fontSize = 14,
                normal = { textColor = Color.white }
            };
        }

        private void ToggleDebugUI()
        {
            isVisible = !isVisible;
        }

        private void OnGUI()
        {
            if (!showDebugUI || !isVisible || inputService == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.BeginVertical("box");

            GUILayout.Label("=== DEBUG INFO ===", labelStyle);
            GUILayout.Label("Press F1 to toggle", labelStyle);
            GUILayout.Space(10);

            // Input Info
            GUILayout.Label($"Movement: {inputService.MovementInput}", labelStyle);
            GUILayout.Label($"Look: {inputService.LookInput}", labelStyle);
            GUILayout.Label($"Sprint: {inputService.IsSprinting}", labelStyle);
            GUILayout.Label($"Interact: {inputService.IsInteracting}", labelStyle);

            GUILayout.Space(10);

            // Service Info
            GUILayout.Label($"Services: {ServiceLocator.GetServiceCount()}", labelStyle);
            GUILayout.Label($"FPS: {(int)(1f / Time.unscaledDeltaTime)}", labelStyle);

            GUILayout.Space(10);

            // Game State
            if (GameManager.Instance != null)
            {
                GUILayout.Label($"State: {GameManager.Instance.GetCurrentState()}", labelStyle);
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            inputActions?.Dispose();
        }
    }
}