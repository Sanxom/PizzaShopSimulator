using UnityEngine;
using DG.Tweening;

namespace PizzaShop.Utilities
{
    /// <summary>
    /// Extension methods for common DOTween animations.
    /// </summary>
    public static class DOTweenExtensions
    {
        // Smooth popup animation
        public static Sequence DoPop(this Transform transform, float duration = 0.3f)
        {
            Vector3 originalScale = transform.localScale;
            transform.localScale = Vector3.zero;

            return DOTween.Sequence()
                .Append(transform.DOScale(originalScale * 1.1f, duration * 0.6f).SetEase(Ease.OutBack))
                .Append(transform.DOScale(originalScale, duration * 0.4f).SetEase(Ease.InOutQuad));
        }

        // Smooth hide animation
        public static Sequence DoHide(this Transform transform, float duration = 0.2f)
        {
            return DOTween.Sequence()
                .Append(transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack));
        }

        // Shake effect
        public static Tweener DoShake(this Transform transform, float duration = 0.5f, float strength = 1f)
        {
            return transform.DOShakePosition(duration, strength, 10, 90, false, true);
        }

        // Pulse effect
        public static Tweener DoPulse(this Transform transform, float scale = 1.1f, float duration = 0.5f)
        {
            Vector3 originalScale = transform.localScale;
            return transform.DOScale(originalScale * scale, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // Fade CanvasGroup
        public static Tweener DoFadeIn(this CanvasGroup group, float duration = 0.3f)
        {
            group.alpha = 0f;
            group.gameObject.SetActive(true);
            return group.DOFade(1f, duration);
        }

        public static Tweener DoFadeOut(this CanvasGroup group, float duration = 0.3f)
        {
            return group.DOFade(0f, duration)
                .OnComplete(() => group.gameObject.SetActive(false));
        }
    }
}