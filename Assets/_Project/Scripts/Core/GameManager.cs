using UnityEngine;
using PizzaShop.Input;

namespace PizzaShop.Core
{
    /// <summary>
    /// Central game manager. Handles game state and initializes all services.
    /// Persists across scene loads.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("[GameManager]");
                    instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;

        [Header("Debug")]
        [SerializeField] private bool debugMode = true;

        private void Awake()
        {
            // Singleton enforcement
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeServices();
        }

        private void InitializeServices()
        {
            Debug.Log("[GameManager] Initializing services...");

            ServiceLocator.Initialize();

            // Register services in dependency order
            ServiceLocator.Register<IDataService>(new DataService());
            ServiceLocator.Register<IInputService>(new InputService());

            // Future services will be added here:
            // ServiceLocator.Register<IAudioService>(new AudioService());
            // ServiceLocator.Register<IUIService>(new UIService());
            // ServiceLocator.Register<ISaveService>(new SaveService());
            // ServiceLocator.Register<IProgressionService>(new ProgressionService());

            Debug.Log($"[GameManager] {ServiceLocator.GetServiceCount()} services initialized");
        }

        private void OnDestroy()
        {
            ServiceLocator.UnregisterAll();
            EventBus.ClearAllEvents();
        }

        public void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            var previousState = currentState;
            currentState = newState;

            Debug.Log($"[GameManager] State: {previousState} → {newState}");

            switch (newState)
            {
                case GameState.MainMenu:
                    HandleMainMenuState();
                    break;
                case GameState.Playing:
                    HandlePlayingState();
                    break;
                case GameState.Paused:
                    HandlePausedState();
                    break;
                case GameState.GameOver:
                    HandleGameOverState();
                    break;
            }
        }

        private void HandleMainMenuState()
        {
            Time.timeScale = 1f;
            ServiceLocator.Get<IInputService>().DisablePlayerInput();
            ServiceLocator.Get<IInputService>().EnableUIInput();
        }

        private void HandlePlayingState()
        {
            Time.timeScale = 1f;
            ServiceLocator.Get<IInputService>().EnablePlayerInput();
            ServiceLocator.Get<IInputService>().DisableUIInput();
            EventBus.RaiseGameStarted();
        }

        private void HandlePausedState()
        {
            Time.timeScale = 0f;
            ServiceLocator.Get<IInputService>().DisablePlayerInput();
            ServiceLocator.Get<IInputService>().EnableUIInput();
            EventBus.RaiseGamePaused();
        }

        private void HandleGameOverState()
        {
            Time.timeScale = 1f;
            ServiceLocator.Get<IInputService>().DisablePlayerInput();
            EventBus.RaiseGameEnded();
        }

        public GameState GetCurrentState() => currentState;

        public bool IsDebugMode() => debugMode;

        private void OnApplicationQuit()
        {
            ServiceLocator.UnregisterAll();
            EventBus.ClearAllEvents();
        }
    }

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }
}