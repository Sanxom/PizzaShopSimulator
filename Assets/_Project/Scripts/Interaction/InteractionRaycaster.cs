using UnityEngine;

namespace PizzaShop.Interaction
{
    /// <summary>
    /// Handles raycasting to detect interactable objects.
    /// Uses camera forward direction to detect what player is looking at.
    /// </summary>
    public class InteractionRaycaster : MonoBehaviour
    {
        [Header("Raycast Settings")]
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private LayerMask interactableLayer = ~0; // All layers by default
        [SerializeField] private float raycastRadius = 0.1f; // SphereCast radius

        [Header("Debug")]
        [SerializeField] private bool drawDebugRay = true;
        [SerializeField] private Color debugRayColor = Color.yellow;

        private Camera playerCamera;
        private IInteractable currentInteractable;
        private RaycastHit lastHit;

        public IInteractable CurrentInteractable => currentInteractable;
        public bool HasTarget => currentInteractable != null;
        public RaycastHit LastHit => lastHit;

        private void Start()
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("[InteractionRaycaster] No main camera found!");
            }
        }

        /// <summary>
        /// Perform raycast and return the interactable object being looked at.
        /// </summary>
        public IInteractable PerformRaycast()
        {
            if (playerCamera == null) return null;

            Ray ray = new(playerCamera.transform.position, playerCamera.transform.forward);
            IInteractable newInteractable = null;

            // Use SphereCast for more forgiving detection
            if (Physics.SphereCast(ray, raycastRadius, out RaycastHit hit, interactionDistance, interactableLayer))
            {
                lastHit = hit;
                newInteractable = hit.collider.GetComponent<IInteractable>();
            }

            // Handle interactable change
            if (newInteractable != currentInteractable)
            {
                // Exit previous
                currentInteractable?.OnLookExit(GetPlayerController());

                // Enter new
                newInteractable?.OnLookEnter(GetPlayerController());

                currentInteractable = newInteractable;
            }

            return currentInteractable;
        }

        /// <summary>
        /// Clear current interactable (called when interaction system is disabled).
        /// </summary>
        public void ClearInteractable()
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnLookExit(GetPlayerController());
                currentInteractable = null;
            }
        }

        private Player.PlayerController GetPlayerController()
        {
            return GetComponentInParent<Player.PlayerController>();
        }

        private void OnDrawGizmos()
        {
            if (!drawDebugRay || playerCamera == null) return;

            Gizmos.color = debugRayColor;
            Vector3 direction = playerCamera.transform.forward * interactionDistance;
            Gizmos.DrawRay(playerCamera.transform.position, direction);

            // Draw sphere at end of ray
            Gizmos.DrawWireSphere(playerCamera.transform.position + direction, raycastRadius);
        }
    }
}