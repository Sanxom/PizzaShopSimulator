using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PizzaShop.Equipment;
using PizzaShop.Data;

namespace PizzaShop.UI
{
    /// <summary>
    /// UI display for oven cooking status.
    /// Shows temperature, cooking progress, and slot status.
    /// </summary>
    public class OvenStatusUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Oven targetOven;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI temperatureText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image temperatureBar;
        [SerializeField] private GameObject slotStatusParent;
        [SerializeField] private OvenSlotStatusUI slotStatusPrefab;

        [Header("Colors")]
        [SerializeField] private Color coldColor = Color.blue;
        [SerializeField] private Color hotColor = Color.red;

        private OvenSlotStatusUI[] slotUIs;

        private void Start()
        {
            if (targetOven != null)
            {
                InitializeSlotUI();
            }
        }

        private void Update()
        {
            if (targetOven != null)
            {
                UpdateDisplay();
            }
        }

        private void InitializeSlotUI()
        {
            if (slotStatusParent == null || slotStatusPrefab == null)
                return;

            slotUIs = new OvenSlotStatusUI[targetOven.Data.MaxPizzas];

            for (int i = 0; i < targetOven.Data.MaxPizzas; i++)
            {
                OvenSlotStatusUI slotUI = Instantiate(slotStatusPrefab, slotStatusParent.transform);
                slotUI.Initialize(i);
                slotUIs[i] = slotUI;
            }
        }

        private void UpdateDisplay()
        {
            // Update temperature
            if (temperatureText != null)
            {
                temperatureText.text = $"{Mathf.RoundToInt(targetOven.CurrentTemperature)}°F";
            }

            if (temperatureBar != null)
            {
                float tempProgress = targetOven.CurrentTemperature / targetOven.Data.CookingTemperature;
                temperatureBar.fillAmount = tempProgress;
                temperatureBar.color = Color.Lerp(coldColor, hotColor, tempProgress);
            }

            // Update status
            if (statusText != null)
            {
                statusText.text = targetOven.DisplayName;
            }

            // Update slots
            if (slotUIs != null)
            {
                for (int i = 0; i < slotUIs.Length; i++)
                {
                    if (i < targetOven.Slots.Count)
                    {
                        slotUIs[i].UpdateStatus(targetOven.Slots[i]);
                    }
                }
            }
        }

        public void SetOven(Oven oven)
        {
            targetOven = oven;
            InitializeSlotUI();
        }
    }

    /// <summary>
    /// Individual slot status display.
    /// </summary>
    public class OvenSlotStatusUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI slotText;
        [SerializeField] private Image progressBar;
        [SerializeField] private Image qualityIndicator;

        private int slotIndex;

        public void Initialize(int index)
        {
            slotIndex = index;
            if (slotText != null)
            {
                slotText.text = $"Slot {index + 1}";
            }
        }

        public void UpdateStatus(OvenSlot slot)
        {
            if (slot == null)
                return;

            if (!slot.IsOccupied)
            {
                // Empty slot
                if (progressBar != null)
                    progressBar.fillAmount = 0f;

                if (qualityIndicator != null)
                    qualityIndicator.color = Color.gray;

                if (slotText != null)
                    slotText.text = $"Slot {slotIndex + 1}: Empty";
            }
            else
            {
                // Occupied slot
                var pizza = slot.CurrentPizza;
                var profile = slot.GetComponent<Oven>()?.Data.GetProfile(pizza.Size);

                if (profile != null && progressBar != null)
                {
                    float progress = profile.GetProgress(slot.CookingTimer);
                    progressBar.fillAmount = progress;
                }

                if (qualityIndicator != null)
                {
                    qualityIndicator.color = GetQualityColor(pizza.CookQuality);
                }

                if (slotText != null)
                {
                    slotText.text = $"Slot {slotIndex + 1}: {pizza.CookQuality}";
                }
            }
        }

        private Color GetQualityColor(CookQuality quality)
        {
            return quality switch
            {
                CookQuality.Raw => new Color(0.5f, 0.5f, 1f),
                CookQuality.Undercooked => new Color(1f, 1f, 0.5f),
                CookQuality.Perfect => new Color(0.2f, 1f, 0.2f),
                CookQuality.Overcooked => new Color(1f, 0.5f, 0f),
                CookQuality.Burnt => new Color(0.3f, 0.1f, 0.1f),
                _ => Color.gray
            };
        }
    }
}