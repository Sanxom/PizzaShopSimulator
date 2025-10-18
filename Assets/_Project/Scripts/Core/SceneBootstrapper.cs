using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PizzaShop.Core
{
    /// <summary>
    /// Handles automatic loading of gameplay scenes with loading screen.
    /// Place in CoreScene to bootstrap the game.
    /// </summary>
    public class SceneBootstrapper : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string initialGameplayScene = "MainShop";
        [SerializeField] private bool autoLoadOnStart = true;
        [SerializeField] private float loadDelay = 0.5f;

        [Header("Loading Screen")]
        [SerializeField] private LoadingIndicator loadingIndicator;
        [SerializeField] private bool useLoadingScreen = true;

        [Header("Debug")]
        [SerializeField] private bool logSceneLoading = true;

        private void Start()
        {
            // Find loading indicator if not assigned
            if (loadingIndicator == null)
            {
                loadingIndicator = FindFirstObjectByType<LoadingIndicator>();
            }

            if (autoLoadOnStart)
            {
                Invoke(nameof(LoadInitialScene), loadDelay);
            }
        }

        private void LoadInitialScene()
        {
            Scene gameplayScene = SceneManager.GetSceneByName(initialGameplayScene);

            if (gameplayScene.isLoaded)
            {
                if (logSceneLoading)
                {
                    Debug.Log($"[SceneBootstrapper] {initialGameplayScene} already loaded");
                }
                return;
            }

            StartCoroutine(LoadSceneWithProgress(initialGameplayScene));
        }

        /// <summary>
        /// Load a gameplay scene with loading screen and progress tracking.
        /// </summary>
        private IEnumerator LoadSceneWithProgress(string sceneName)
        {
            if (logSceneLoading)
            {
                Debug.Log($"[SceneBootstrapper] Loading {sceneName}...");
            }

            // Show loading screen
            if (useLoadingScreen && loadingIndicator != null)
            {
                loadingIndicator.Show();
                yield return new WaitForSecondsRealtime(0.3f); // Let fade in complete
            }

            // Start async load
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            asyncLoad.allowSceneActivation = false; // Control when scene activates

            // Update progress while loading
            while (!asyncLoad.isDone)
            {
                // AsyncOperation progress goes from 0 to 0.9, then jumps to 1
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                if (useLoadingScreen && loadingIndicator != null)
                {
                    loadingIndicator.UpdateProgress(progress);
                }

                // Scene is ready to activate
                if (asyncLoad.progress >= 0.9f)
                {
                    // Wait a moment at 100% for effect
                    if (useLoadingScreen && loadingIndicator != null)
                    {
                        loadingIndicator.UpdateProgress(1f);
                    }

                    yield return new WaitForSecondsRealtime(0.5f);

                    // Activate the scene
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            // Set as active scene (for lighting, etc.)
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.isLoaded)
            {
                SceneManager.SetActiveScene(loadedScene);
            }

            if (logSceneLoading)
            {
                Debug.Log($"[SceneBootstrapper] {sceneName} loaded and activated");
            }

            // Hide loading screen
            if (useLoadingScreen && loadingIndicator != null)
            {
                loadingIndicator.Hide();
            }
        }

        /// <summary>
        /// Public method to load different scenes at runtime.
        /// </summary>
        public void LoadGameplayScene(string sceneName)
        {
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            // Show loading screen
            if (useLoadingScreen && loadingIndicator != null)
            {
                loadingIndicator.Show();
                yield return new WaitForSecondsRealtime(0.3f);
            }

            // Unload current gameplay scenes
            yield return UnloadGameplayScenes();

            // Load new scene
            yield return LoadSceneWithProgress(sceneName);
        }

        private IEnumerator UnloadGameplayScenes()
        {
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene.name != "CoreScene" && scene.isLoaded)
                {
                    if (logSceneLoading)
                    {
                        Debug.Log($"[SceneBootstrapper] Unloading {scene.name}");
                    }

                    yield return SceneManager.UnloadSceneAsync(scene);
                }
            }
        }
    }
}