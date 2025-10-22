using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

namespace PizzaShop.Interaction
{
    /// <summary>
    /// UI display for interaction prompts.
    /// Shows prompts with smooth fade animations and dynamic text updates.
    /// </summary>
    public class InteractionUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private Image progressBar;
        [SerializeField] private GameObject progressBarContainer;

        [Header("Animation Settings")]
        [SerializeField] private float fadeDuration = 0.2f;
        [SerializeField] private Ease fadeEase = Ease.OutQuad;

        private Tweener fadeTween;
        private bool isVisible;
        private string currentPromptText;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (promptText == null)
            {
                promptText = GetComponentInChildren<TextMeshProUGUI>();
            }

            // Start hidden
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            isVisible = false;

            if (progressBarContainer != null)
            {
                progressBarContainer.SetActive(false);
            }
        }

        /// <summary>
        /// Show the interaction prompt.
        /// </summary>
        public void Show(string prompt, bool showProgress = false)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                Hide();
                return;
            }

            // Update text
            currentPromptText = prompt;
            if (promptText != null)
            {
                promptText.text = prompt;
            }

            // Handle progress bar visibility
            if (progressBarContainer != null)
            {
                progressBarContainer.SetActive(showProgress);
            }

            // Fade in if not already visible
            if (!isVisible)
            {
                isVisible = true;
                FadeIn();
            }
        }

        /// <summary>
        /// Update just the text without re-fading.
        /// Used for dynamic prompts that change while looking at same object.
        /// </summary>
        public void UpdateText(string prompt)
        {
            if (string.IsNullOrEmpty(prompt))
            {
                Hide();
                return;
            }

            // Only update if text actually changed
            if (currentPromptText != prompt)
            {
                currentPromptText = prompt;
                if (promptText != null)
                {
                    promptText.text = prompt;
                }
            }
        }

        /// <summary>
        /// Update progress bar (for hold interactions).
        /// </summary>
        public void UpdateProgress(float progress)
        {
            if (progressBar != null)
            {
                progressBar.fillAmount = Mathf.Clamp01(progress);
            }
        }

        /// <summary>
        /// Hide the interaction prompt.
        /// </summary>
        public void Hide()
        {
            if (!isVisible) return;

            isVisible = false;
            currentPromptText = null;
            FadeOut();

            if (progressBarContainer != null)
            {
                progressBarContainer.SetActive(false);
            }
        }

        /// <summary>
        /// Check if prompt is currently visible.
        /// </summary>
        public bool IsVisible()
        {
            return isVisible;
        }

        /// <summary>
        /// Get current prompt text.
        /// </summary>
        public string GetCurrentText()
        {
            return currentPromptText;
        }

        private void FadeIn()
        {
            fadeTween?.Kill();
            canvasGroup.blocksRaycasts = false; // UI should not block interactions
            fadeTween = canvasGroup.DOFade(1f, fadeDuration)
                .SetEase(fadeEase)
                .SetUpdate(true); // Ignore timescale
        }

        private void FadeOut()
        {
            fadeTween?.Kill();
            fadeTween = canvasGroup.DOFade(0f, fadeDuration)
                .SetEase(fadeEase)
                .SetUpdate(true)
                .OnComplete(() => canvasGroup.blocksRaycasts = false);
        }

        private void OnDestroy()
        {
            fadeTween?.Kill();
        }
    }
}