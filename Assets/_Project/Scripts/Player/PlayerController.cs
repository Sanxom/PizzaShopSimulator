using UnityEngine;
using PizzaShop.Core;
using PizzaShop.Input;

namespace PizzaShop.Player
{
    /// <summary>
    /// Handles player movement and rotation using new Input System.
    /// Works with CharacterController for physics-based movement.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundCheckDistance = 0.2f;

        [Header("Look Settings")]
        [SerializeField] private float lookSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 80f;
        [SerializeField] private bool invertYAxis = false;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;

        private IInputService inputService;
        private CharacterController controller;
        private float verticalVelocity;
        private float cameraPitch;
        private Vector3 lastPosition;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (cameraTransform == null)
            {
                cameraTransform = Camera.main.transform;
                if (cameraTransform == null)
                {
                    Debug.LogError("[PlayerController] No camera found!");
                }
            }

            // Lock cursor for FPS experience
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            lastPosition = transform.position;
        }

        private void Start()
        {
            inputService = ServiceLocator.Get<IInputService>();
        }

        private void Update()
        {
            HandleMovement();
            HandleLook();
            CheckForMovement();
        }

        private void HandleMovement()
        {
            Vector2 moveInput = inputService.MovementInput;

            // Calculate movement speed
            float currentSpeed = inputService.IsSprinting ? walkSpeed * sprintMultiplier : walkSpeed;

            // Calculate move direction relative to player rotation
            Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            moveDirection = moveDirection.normalized * currentSpeed;

            // Apply movement
            controller.Move(moveDirection * Time.deltaTime);

            // Handle gravity
            if (controller.isGrounded)
            {
                verticalVelocity = 0f; // Keep grounded
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            controller.Move(Time.deltaTime * verticalVelocity * Vector3.up);
        }

        private void HandleLook()
        {
            Vector2 lookInput = inputService.LookInput;

            // Horizontal rotation (yaw) - rotate player body
            transform.Rotate(Vector3.up * lookInput.x * lookSensitivity);

            // Vertical rotation (pitch) - rotate CameraHolder
            float yInput = invertYAxis ? lookInput.y : -lookInput.y;
            cameraPitch += yInput * lookSensitivity;
            cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);

            // Apply pitch to CameraHolder (parent of camera)
            if (cameraTransform != null && cameraTransform.parent != null)
            {
                cameraTransform.parent.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            }
        }

        private void CheckForMovement()
        {
            // Emit movement event if position changed
            if (Vector3.Distance(transform.position, lastPosition) > 0.01f)
            {
                EventBus.RaisePlayerMoved(transform.position);
                lastPosition = transform.position;
            }
        }

        public void SetLookSensitivity(float sensitivity)
        {
            lookSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
        }

        public void SetInvertY(bool invert)
        {
            invertYAxis = invert;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw ground check sphere
            if (controller != null)
            {
                Vector3 spherePosition = transform.position - new Vector3(0, controller.height / 2, 0);
                Gizmos.color = controller.isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(spherePosition, groundCheckDistance);
            }
        }
    }
}