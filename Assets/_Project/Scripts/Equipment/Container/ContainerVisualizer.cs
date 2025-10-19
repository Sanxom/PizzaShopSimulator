using UnityEngine;
using TMPro;
using DG.Tweening;
using PizzaShop.Data;

namespace PizzaShop.Equipment
{
    /// <summary>
    /// Handles all visual representation for containers.
    /// Separated from Container logic (Single Responsibility Principle).
    /// </summary>
    public class ContainerVisualizer : MonoBehaviour
    {
        [Header("Visual Components")]
        [SerializeField] private MeshRenderer containerRenderer;
        [SerializeField] private Transform fillIndicator;
        [SerializeField] private TextMeshPro label;
        [SerializeField] private GameObject emptyIndicator;

        [Header("Animation Settings")]
        [SerializeField] private float fillAnimationDuration = 0.3f;
        [SerializeField] private Ease fillEase = Ease.OutQuad;

        private MaterialPropertyBlock propertyBlock;
        private Tweener currentTween;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();

            // Get components if not assigned
            if (containerRenderer == null)
                containerRenderer = GetComponent<MeshRenderer>();

            if (fillIndicator == null)
            {
                var fill = transform.Find("FillIndicator");
                if (fill != null) fillIndicator = fill;
            }

            if (label == null)
            {
                var labelObj = GetComponentInChildren<TextMeshPro>();
                if (labelObj != null) label = labelObj;
            }
        }

        /// <summary>
        /// Show container as assigned to ingredient.
        /// </summary>
        public void ShowAssigned(IngredientData ingredient, int current, int max)
        {
            if (ingredient == null) return;

            // Update color
            if (containerRenderer != null)
            {
                Color tintColor = Color.Lerp(ingredient.IngredientColor, Color.white, 0.5f);
                propertyBlock.SetColor("_BaseColor", tintColor);
                containerRenderer.SetPropertyBlock(propertyBlock);
            }

            // Update fill indicator
            UpdateFillLevel((float)current / max, ingredient.IngredientColor);

            // Update label
            if (label != null)
            {
                label.text = $"{ingredient.DisplayName}\n{current}/{max}";
                label.DOFade(1f, 0.2f);
            }

            // Hide empty indicator
            if (emptyIndicator != null)
            {
                emptyIndicator.SetActive(false);
            }
        }

        /// <summary>
        /// Show container as empty/generic.
        /// </summary>
        public void ShowEmpty()
        {
            // Reset color to gray
            if (containerRenderer != null)
            {
                propertyBlock.SetColor("_BaseColor", new Color(0.7f, 0.7f, 0.7f));
                containerRenderer.SetPropertyBlock(propertyBlock);
            }

            // Hide fill
            if (fillIndicator != null)
            {
                fillIndicator.DOScaleY(0f, fillAnimationDuration).SetEase(fillEase);
            }

            // Update label
            if (label != null)
            {
                label.text = "Empty\nContainer";
                label.DOFade(0.5f, 0.2f);
            }

            // Show empty indicator
            if (emptyIndicator != null)
            {
                emptyIndicator.SetActive(true);
            }
        }

        /// <summary>
        /// Animate fill level smoothly.
        /// </summary>
        private void UpdateFillLevel(float percentage, Color fillColor)
        {
            if (fillIndicator == null) return;

            // Kill existing tween
            currentTween?.Kill();

            // Show fill indicator
            fillIndicator.gameObject.SetActive(true);

            // Animate scale
            float targetHeight = Mathf.Clamp01(percentage);
            currentTween = fillIndicator.DOScaleY(targetHeight, fillAnimationDuration)
                .SetEase(fillEase);

            // Update position to keep bottom aligned
            Vector3 pos = fillIndicator.localPosition;
            pos.y = targetHeight * 0.5f;
            fillIndicator.localPosition = pos;

            // Color based on fill level
            Color displayColor = GetFillColor(percentage, fillColor);

            if (fillIndicator.TryGetComponent<MeshRenderer>(out var renderer))
            {
                propertyBlock.SetColor("_BaseColor", displayColor);
                renderer.SetPropertyBlock(propertyBlock);
            }
        }

        private Color GetFillColor(float percentage, Color baseColor)
        {
            if (percentage > 0.3f) return baseColor;
            if (percentage > 0.1f) return Color.Lerp(baseColor, Color.yellow, 0.5f);
            return Color.Lerp(baseColor, Color.red, 0.5f);
        }

        private void OnDestroy()
        {
            currentTween?.Kill();
        }
    }
}