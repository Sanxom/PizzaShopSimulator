using UnityEngine;

namespace PizzaShop.Interaction
{
    /// <summary>
    /// Handles audio feedback for interactions.
    /// Placeholder for Phase 4 when audio system is implemented.
    /// </summary>
    public class InteractionAudio : MonoBehaviour
    {
        [Header("Audio Clips")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private AudioClip dropSound;
        [SerializeField] private AudioClip hoverSound;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }

        public void PlayPickup()
        {
            if (pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
        }

        public void PlayDrop()
        {
            if (dropSound != null)
            {
                audioSource.PlayOneShot(dropSound);
            }
        }

        public void PlayHover()
        {
            if (hoverSound != null && !audioSource.isPlaying)
            {
                audioSource.PlayOneShot(hoverSound);
            }
        }
    }
}