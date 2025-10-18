using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace PizzaShop.Core
{
    /// <summary>
    /// Manages loading screen UI with smooth animations.
    /// Shows progress, loading text, and optional tips.
    /// </summary>
    public class LoadingIndicator : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private Image progressBarFill;
        [SerializeField] private TextMeshProUGUI tipText;
        [SerializeField] private Transform logo;

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float minDisplayTime = 1f; // Minimum time to show loading screen

        [Header("Loading Tips")]
        [SerializeField]
        private string[] loadingTips = new string[]
        {
            "Tip: Keep your ingredients stocked to avoid running out!",
            "Tip: Faster service means happier customers!",
            "Tip: Perfect pizzas earn more money!",
            "Tip: Upgrade your equipment to serve more customers!",
            "Tip: Don't let pizzas burn in the oven!",
            "Tip: Match orders exactly for maximum payment!",
            "Tip: Organize your containers for faster workflow!"
        };

        [Header("Progress Animation")]
        [SerializeField] private bool smoothProgressAnimation = true;
        [SerializeField] private float progressAnimSpeed = 2f;

        private float targetProgress = 0f;
        private float currentProgress = 0f;
        private float displayStartTime;
        private Coroutine animateDotsCoroutine;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            // Start hidden
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Show the loading screen with fade in animation.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            displayStartTime = Time.realtimeSinceStartup;

            // Reset progress
            currentProgress = 0f;
            targetProgress = 0f;
            UpdateProgressBar(0f);

            // Show random tip
            if (tipText != null && loadingTips.Length > 0)
            {
                tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
            }

            // Fade in
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, fadeInDuration).SetUpdate(true);
            }

            // Animate logo (optional)
            if (logo != null)
            {
                logo.localScale = Vector3.zero;
                logo.DOScale(Vector3.one, fadeInDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            }

            // Start animating loading text dots
            if (loadingText != null)
            {
                animateDotsCoroutine = StartCoroutine(AnimateLoadingDots());
            }

            Debug.Log("[LoadingIndicator] Shown");
        }

        /// <summary>
        /// Hide the loading screen with fade out animation.
        /// Ensures minimum display time is met.
        /// </summary>
        public void Hide()
        {
            // Calculate how long loading screen has been visible
            float displayDuration = Time.realtimeSinceStartup - displayStartTime;
            float remainingTime = Mathf.Max(0f, minDisplayTime - displayDuration);

            // Wait for minimum display time, then fade out
            DOVirtual.DelayedCall(remainingTime, () =>
            {
                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(0f, fadeOutDuration)
                        .SetUpdate(true)
                        .OnComplete(() => gameObject.SetActive(false));
                }
                else
                {
                    gameObject.SetActive(false);
                }

                // Stop animating dots
                if (animateDotsCoroutine != null)
                {
                    StopCoroutine(animateDotsCoroutine);
                }

                Debug.Log("[LoadingIndicator] Hidden");
            }, true);
        }

        /// <summary>
        /// Update the progress bar (0 to 1).
        /// </summary>
        public void UpdateProgress(float progress)
        {
            targetProgress = Mathf.Clamp01(progress);

            if (!smoothProgressAnimation)
            {
                currentProgress = targetProgress;
                UpdateProgressBar(currentProgress);
            }
        }

        private void Update()
        {
            if (!gameObject.activeSelf || !smoothProgressAnimation) return;

            // Smoothly animate progress towards target
            if (currentProgress < targetProgress)
            {
                currentProgress = Mathf.MoveTowards(
                    currentProgress,
                    targetProgress,
                    progressAnimSpeed * Time.unscaledDeltaTime
                );
                UpdateProgressBar(currentProgress);
            }
        }

        private void UpdateProgressBar(float progress)
        {
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = progress;
            }
        }

        /// <summary>
        /// Animates "Loading..." to "Loading." to "Loading.." to "Loading..."
        /// </summary>
        private IEnumerator AnimateLoadingDots()
        {
            if (loadingText == null) yield break;

            string baseText = "Loading";
            int dotCount = 0;

            while (true)
            {
                dotCount = (dotCount + 1) % 4;
                loadingText.text = baseText + new string('.', dotCount);
                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        /// <summary>
        /// Set custom loading text.
        /// </summary>
        public void SetLoadingText(string text)
        {
            if (loadingText != null)
            {
                // Stop dot animation if running
                if (animateDotsCoroutine != null)
                {
                    StopCoroutine(animateDotsCoroutine);
                }

                loadingText.text = text;
            }
        }

        /// <summary>
        /// Show a specific tip.
        /// </summary>
        public void SetTip(string tip)
        {
            if (tipText != null)
            {
                tipText.text = tip;
            }
        }
    }
}