using UnityEngine;
using System.Collections.Generic;
using PizzaShop.Data;
using PizzaShop.Core;
using PizzaShop.Interaction;
using PizzaShop.Player;
using PizzaShop.Food;
using DG.Tweening;

namespace PizzaShop.Equipment
{
    /// <summary>
    /// Main oven component that cooks pizzas.
    /// Uses state machine for different oven modes.
    /// Manages multiple cooking slots.
    /// </summary>
    public class Oven : InteractableBase
    {
        [Header("Configuration")]
        [SerializeField] private OvenData ovenData;

        [Header("Oven Slots")]
        [SerializeField] private Transform slotsParent;
        [SerializeField] private GameObject slotPrefab;

        [Header("Visual Components")]
        [SerializeField] private MeshRenderer ovenRenderer;
        [SerializeField] private Light ovenLight;
        [SerializeField] private ParticleSystem heatParticles;

        [Header("Audio")]
        [SerializeField] private AudioSource ovenAudio;
        [SerializeField] private AudioClip turnOnSound;
        [SerializeField] private AudioClip cookingLoopSound;
        [SerializeField] private AudioClip doneSound;

        private OvenState currentState;
        private List<OvenSlot> slots;
        private float currentTemperature;
        private MaterialPropertyBlock propertyBlock;

        // State instances
        public OvenOffState OffState { get; private set; }
        public OvenHeatingState HeatingState { get; private set; }
        public OvenReadyState ReadyState { get; private set; }
        public OvenCookingState CookingState { get; private set; }

        public OvenData Data => ovenData;
        public float CurrentTemperature => currentTemperature;
        public IReadOnlyList<OvenSlot> Slots => slots;

        protected override void Awake()
        {
            base.Awake();

            if (ovenData == null)
            {
                Debug.LogError("[Oven] No oven data assigned!");
                return;
            }

            propertyBlock = new MaterialPropertyBlock();
            slots = new List<OvenSlot>();

            // Initialize states
            OffState = new OvenOffState(this);
            HeatingState = new OvenHeatingState(this);
            ReadyState = new OvenReadyState(this);
            CookingState = new OvenCookingState(this);

            InitializeSlots();
            TransitionToState(OffState);
        }

        private void Update()
        {
            currentState?.Update(Time.deltaTime);
        }

        /// <summary>
        /// Initialize cooking slots.
        /// </summary>
        private void InitializeSlots()
        {
            if (slotsParent == null)
            {
                GameObject parent = new GameObject("Slots");
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
                slotsParent = parent.transform;
            }

            for (int i = 0; i < ovenData.MaxPizzas; i++)
            {
                CreateSlot(i);
            }

            Debug.Log($"[Oven] Initialized {slots.Count} slots");
        }

        private void CreateSlot(int index)
        {
            Vector3 slotPosition = CalculateSlotPosition(index);

            GameObject slotObj = slotPrefab != null
                ? Instantiate(slotPrefab, slotPosition, Quaternion.identity, slotsParent)
                : CreateDefaultSlot(slotPosition);

            OvenSlot slot = slotObj.GetComponent<OvenSlot>();
            if (slot == null)
            {
                slot = slotObj.AddComponent<OvenSlot>();
            }

            slot.Initialize(index);
            slots.Add(slot);
        }

        private Vector3 CalculateSlotPosition(int index)
        {
            // Arrange slots in a row
            float spacing = 0.3f;
            float offset = (ovenData.MaxPizzas - 1) * spacing * 0.5f;
            return slotsParent.position + Vector3.right * (index * spacing - offset);
        }

        private GameObject CreateDefaultSlot(Vector3 position)
        {
            GameObject slotObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            slotObj.transform.position = position;
            slotObj.transform.localScale = Vector3.one * 0.2f;
            slotObj.transform.SetParent(slotsParent);
            return slotObj;
        }

        // ==================== STATE MANAGEMENT ====================

        public void TransitionToState(OvenState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();

            UpdateDisplayName();
        }

        private void UpdateDisplayName()
        {
            if (currentState == OffState)
                displayName = $"{ovenData.DisplayName} (Off)";
            else if (currentState == HeatingState)
                displayName = $"{ovenData.DisplayName} (Heating)";
            else if (currentState == ReadyState)
                displayName = $"{ovenData.DisplayName} (Ready)";
            else if (currentState == CookingState)
                displayName = $"{ovenData.DisplayName} (Cooking)";
        }

        // ==================== OVEN OPERATIONS ====================

        public void TurnOn()
        {
            if (currentState != OffState)
            {
                Debug.LogWarning("[Oven] Oven is already on!");
                return;
            }

            TransitionToState(HeatingState);

            if (ovenAudio != null && turnOnSound != null)
            {
                ovenAudio.PlayOneShot(turnOnSound);
            }

            Debug.Log("[Oven] Oven turned on");
        }

        public void TurnOff()
        {
            if (currentState == OffState)
            {
                Debug.LogWarning("[Oven] Oven is already off!");
                return;
            }

            TransitionToState(OffState);

            if (ovenAudio != null)
            {
                ovenAudio.Stop();
            }

            Debug.Log("[Oven] Oven turned off");
        }

        public void SetTemperature(float temperature)
        {
            currentTemperature = temperature;
            UpdateVisuals();
        }

        // ==================== PIZZA MANAGEMENT ====================

        public bool TryPlacePizza(PizzaShop.Inventory.PlayerInventory inventory, PizzaShop.Inventory.InventoryItem pizzaItem)
        {
            OvenSlot emptySlot = GetEmptySlot();
            if (emptySlot == null)
            {
                Debug.LogWarning("[Oven] No empty slots available!");
                return false;
            }

            // Get the pizza GameObject from inventory
            GameObject pizzaObj = inventory.CurrentItem.visualPrefab;
            if (pizzaObj == null)
            {
                Debug.LogError("[Oven] Pizza has no visual!");
                return false;
            }

            Pizza pizza = pizzaObj.GetComponent<Pizza>();
            if (pizza == null)
            {
                Debug.LogError("[Oven] Object is not a pizza!");
                return false;
            }

            // Check if pizza is complete
            if (!pizza.IsComplete)
            {
                Debug.LogWarning("[Oven] Cannot cook incomplete pizza!");
                return false;
            }

            // Check if oven supports this size
            if (!ovenData.SupportsSize(pizza.Size))
            {
                Debug.LogWarning($"[Oven] This oven doesn't support {pizza.Size} pizzas!");
                return false;
            }

            // Remove from inventory
            inventory.ClearHeldItem();

            // Place in slot
            emptySlot.PlacePizza(pizza);

            // Raise event
            EventBus.RaisePizzaPlacedInOven(pizza, this);

            // Transition to cooking if not already
            if (currentState != CookingState)
            {
                TransitionToState(CookingState);

                if (ovenAudio != null && cookingLoopSound != null)
                {
                    ovenAudio.clip = cookingLoopSound;
                    ovenAudio.loop = true;
                    ovenAudio.Play();
                }
            }

            Debug.Log($"[Oven] Pizza placed in slot {emptySlot.SlotIndex}");
            return true;
        }

        public bool TryRemoveCookedPizza(PizzaShop.Inventory.PlayerInventory inventory)
        {
            // Find first cooked pizza
            foreach (var slot in slots)
            {
                if (slot.IsOccupied && slot.CurrentPizza.State == PizzaState.Cooked)
                {
                    Pizza pizza = slot.RemovePizza();
                    pizza.transform.SetParent(null);

                    // Create inventory item
                    var item = new PizzaShop.Inventory.InventoryItem(
                        $"cooked_pizza_{pizza.Size}",
                        $"Cooked {pizza.Size} Pizza ({pizza.CookQuality})",
                        PizzaShop.Inventory.ItemType.Pizza
                    );

                    if (inventory.PickupItem(item, pizza.gameObject))
                    {
                        EventBus.RaisePizzaRemovedFromOven(pizza, this);

                        // Check if oven is now empty
                        if (!HasPizzas())
                        {
                            TransitionToState(ReadyState);

                            if (ovenAudio != null)
                            {
                                ovenAudio.Stop();
                            }
                        }

                        Debug.Log($"[Oven] Pizza removed from oven - Quality: {pizza.CookQuality}");
                        return true;
                    }
                }
            }

            Debug.LogWarning("[Oven] No cooked pizzas available!");
            return false;
        }

        public void UpdateCookingPizzas(float deltaTime)
        {
            foreach (var slot in slots)
            {
                if (slot.IsOccupied && slot.IsCooking)
                {
                    CookingProfile profile = ovenData.GetProfile(slot.CurrentPizza.Size);
                    slot.UpdateCooking(deltaTime, profile.perfectTime, profile.burnTime);

                    // Check if pizza just finished cooking
                    if (slot.CurrentPizza.State == PizzaState.Cooked)
                    {
                        slot.CurrentPizza.FinishCooking();

                        if (ovenAudio != null && doneSound != null)
                        {
                            ovenAudio.PlayOneShot(doneSound);
                        }
                    }
                }
            }
        }

        // ==================== SLOT QUERIES ====================

        public OvenSlot GetEmptySlot()
        {
            foreach (var slot in slots)
            {
                if (!slot.IsOccupied)
                    return slot;
            }
            return null;
        }

        public bool HasEmptySlot()
        {
            return GetEmptySlot() != null;
        }

        public bool HasPizzas()
        {
            foreach (var slot in slots)
            {
                if (slot.IsOccupied)
                    return true;
            }
            return false;
        }

        public bool HasCookedPizza()
        {
            foreach (var slot in slots)
            {
                if (slot.IsOccupied && slot.CurrentPizza.State == PizzaState.Cooked)
                    return true;
            }
            return false;
        }

        public int GetOccupiedSlotCount()
        {
            int count = 0;
            foreach (var slot in slots)
            {
                if (slot.IsOccupied)
                    count++;
            }
            return count;
        }

        // ==================== VISUALS ====================

        public void UpdateVisuals()
        {
            if (ovenRenderer != null)
            {
                Color targetColor = currentState switch
                {
                    OvenHeatingState => ovenData.HeatingColor,
                    OvenReadyState => ovenData.CookingColor,
                    OvenCookingState => ovenData.CookingColor,
                    _ => Color.gray
                };

                propertyBlock.SetColor("_BaseColor", targetColor);
                ovenRenderer.SetPropertyBlock(propertyBlock);
            }

            if (ovenLight != null)
            {
                ovenLight.enabled = currentState != OffState;
                ovenLight.intensity = Mathf.Lerp(0f, 2f, currentTemperature / ovenData.CookingTemperature);
            }

            if (heatParticles != null)
            {
                var emission = heatParticles.emission;
                emission.enabled = currentState != OffState;

                if (currentState != OffState)
                {
                    float rate = Mathf.Lerp(0f, 50f, currentTemperature / ovenData.CookingTemperature);
                    emission.rateOverTime = rate;
                }
            }
        }

        // ==================== INTERACTION INTERFACE ====================

        public override void OnInteract(PlayerController player)
        {
            currentState?.OnInteract(player);
        }

        public override void OnAlternateInteract(PlayerController player)
        {
            currentState?.OnAlternateInteract(player);
        }

        public override string GetInteractionPrompt(PlayerController player)
        {
            return currentState?.GetPrompt(player) ?? "";
        }

        private void OnDestroy()
        {
            // Clean up DOTween
            transform.DOKill();
        }
    }
}