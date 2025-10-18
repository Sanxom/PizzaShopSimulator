using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace PizzaShop.Interaction
{
    /// <summary>
    /// Manages the interaction prompt UI.
    /// Shows prompts and progress for hold interactions.
    /// </summary>
    public class InteractionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private Image progressFill;
        [SerializeField] private GameObject progressBar;

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.15f;

        private bool isVisible = false;
        private Tweener currentTween;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            // Start hidden
            canvasGroup.alpha = 0f;
            isVisible = false;

            if (progressBar != null)
            {
                progressBar.SetActive(false);
            }
        }

        /// <summary>
        /// Show the interaction prompt.
        /// </summary>
        public void Show(string prompt, bool showProgress = false)
        {
            if (promptText != null)
            {
                promptText.text = prompt;
            }

            if (progressBar != null)
            {
                progressBar.SetActive(showProgress);
            }

            if (!isVisible)
            {
                isVisible = true;
                currentTween?.Kill();
                currentTween = canvasGroup.DOFade(1f, fadeInDuration).SetUpdate(true);
            }
        }

        /// <summary>
        /// Hide the interaction prompt.
        /// </summary>
        public void Hide()
        {
            if (isVisible)
            {
                isVisible = false;
                currentTween?.Kill();
                currentTween = canvasGroup.DOFade(0f, fadeOutDuration).SetUpdate(true);

                if (progressBar != null)
                {
                    progressBar.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Update hold interaction progress (0 to 1).
        /// </summary>
        public void UpdateProgress(float progress)
        {
            if (progressFill != null)
            {
                progressFill.fillAmount = progress;
            }
        }

        private void OnDestroy()
        {
            currentTween?.Kill();
        }
    }
}