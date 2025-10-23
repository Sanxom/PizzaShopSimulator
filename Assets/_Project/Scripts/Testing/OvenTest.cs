using UnityEngine;
using PizzaShop.Equipment;
using PizzaShop.Data;
using PizzaShop.Food;

namespace PizzaShop.Testing
{
    /// <summary>
    /// Test script for oven functionality.
    /// </summary>
    public class OvenTest : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Oven testOven;
        [SerializeField] private GameObject testPizzaPrefab;

        [Header("Test Controls")]
        [SerializeField] private KeyCode turnOnKey = KeyCode.O;
        [SerializeField] private KeyCode turnOffKey = KeyCode.P;
        [SerializeField] private KeyCode addPizzaKey = KeyCode.L;

        private void Update()
        {
            if (testOven == null)
                return;

            if (UnityEngine.Input.GetKeyDown(turnOnKey))
            {
                testOven.TurnOn();
                Debug.Log("[OvenTest] Turned on oven");
            }

            if (UnityEngine.Input.GetKeyDown(turnOffKey))
            {
                testOven.TurnOff();
                Debug.Log("[OvenTest] Turned off oven");
            }

            if (UnityEngine.Input.GetKeyDown(addPizzaKey))
            {
                TestAddPizza();
            }
        }

        private void TestAddPizza()
        {
            if (testPizzaPrefab == null)
            {
                Debug.LogWarning("[OvenTest] No test pizza prefab assigned!");
                return;
            }

            // Create test pizza
            GameObject pizzaObj = Instantiate(testPizzaPrefab);
            Pizza pizza = pizzaObj.GetComponent<Pizza>();

            if (pizza == null)
            {
                pizza = pizzaObj.AddComponent<Pizza>();
            }

            // Initialize as complete pizza
            pizza.Initialize(Orders.PizzaSize.Large, null);

            // Simulate adding all ingredients
            // (In real game, this would be done through AssemblyZone)

            Debug.Log("[OvenTest] Created test pizza - attempting to add to oven");
        }

        [ContextMenu("Turn On Oven")]
        public void ContextTurnOn()
        {
            if (testOven != null)
                testOven.TurnOn();
        }

        [ContextMenu("Turn Off Oven")]
        public void ContextTurnOff()
        {
            if (testOven != null)
                testOven.TurnOff();
        }
    }
}