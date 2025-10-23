using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using PizzaShop.Core;

namespace PizzaShop.Orders.UI
{
    public class OrderPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI orderIDText;
        [SerializeField] private TextMeshProUGUI customerNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI paymentText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerFillImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color dangerColor = Color.red;
        [SerializeField] private Color completedColor = Color.green;

        [Header("Animation")]
        [SerializeField] private float showDuration = 0.3f;
        [SerializeField] private float hideDuration = 0.2f;
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;

        private Order currentOrder;
        private bool isShowing = false;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            transform.localScale = Vector3.zero;
        }

        public void SetOrder(Order order)
        {
            currentOrder = order;
            UpdateDisplay();

            if (!isShowing)
            {
                Show();
            }
        }

        public void UpdateDisplay()
        {
            if (currentOrder == null) return;

            orderIDText.text = currentOrder.OrderID;
            customerNameText.text = currentOrder.CustomerName;
            descriptionText.text = currentOrder.GetDescription();

            int payment = currentOrder.GetCurrentPayment();
            paymentText.text = $"${payment}";

            UpdateTimer();
        }

        private void Update()
        {
            if (currentOrder != null && currentOrder.State == OrderState.Active)
            {
                UpdateTimer();
            }
        }

        private void UpdateTimer()
        {
            if (currentOrder == null) return;

            float timeRemaining = currentOrder.TimeRemaining;
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";

            float percentage = currentOrder.GetTimePercentage();
            timerFillImage.fillAmount = percentage;

            // Update color based on time remaining
            Color targetColor;
            if (percentage > 0.5f)
                targetColor = normalColor;
            else if (percentage > 0.25f)
                targetColor = warningColor;
            else
                targetColor = dangerColor;

            timerFillImage.color = targetColor;
            backgroundImage.color = Color.Lerp(Color.white, targetColor, 0.2f);
        }

        public void Show()
        {
            isShowing = true;
            gameObject.SetActive(true);

            transform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(Vector3.one, showDuration).SetEase(showEase));
            sequence.Join(canvasGroup.DOFade(1f, showDuration));
            sequence.Play();
        }

        public void Hide()
        {
            isShowing = false;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(Vector3.zero, hideDuration).SetEase(hideEase));
            sequence.Join(canvasGroup.DOFade(0f, hideDuration));
            sequence.OnComplete(() => gameObject.SetActive(false));
            sequence.Play();
        }

        public void MarkCompleted()
        {
            backgroundImage.DOColor(completedColor, 0.3f);

            DOTween.Sequence()
                .AppendInterval(1f)
                .AppendCallback(Hide);
        }

        public void MarkExpired()
        {
            backgroundImage.DOColor(dangerColor, 0.3f);

            // Flash effect
            Sequence flash = DOTween.Sequence();
            flash.Append(canvasGroup.DOFade(0.3f, 0.2f));
            flash.Append(canvasGroup.DOFade(1f, 0.2f));
            flash.SetLoops(3);
            flash.OnComplete(Hide);
        }

        public Order GetOrder()
        {
            return currentOrder;
        }

        public void Clear()
        {
            currentOrder = null;
            Hide();
        }
    }
}