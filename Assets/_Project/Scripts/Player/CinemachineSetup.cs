using UnityEngine;
using Unity.Cinemachine;

namespace PizzaShop.Player
{
    /// <summary>
    /// Sets up Cinemachine camera to follow the player.
    /// Compatible with Cinemachine 3.0+ (Unity 6)
    /// </summary>
    public class CinemachineSetup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private Transform followTarget;

        private void Start()
        {
            if (cinemachineCamera == null)
            {
                cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            }

            if (followTarget == null)
            {
                followTarget = transform; // Use this object as target
            }

            if (cinemachineCamera != null && followTarget != null)
            {
                cinemachineCamera.Target.TrackingTarget = followTarget;
                Debug.Log("[CinemachineSetup] Cinemachine camera configured");
            }
            else
            {
                Debug.LogError("[CinemachineSetup] Missing references!");
            }
        }
    }
}