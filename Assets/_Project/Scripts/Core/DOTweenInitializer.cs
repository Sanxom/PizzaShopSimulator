using UnityEngine;
using DG.Tweening;

namespace PizzaShop.Core
{
    /// <summary>
    /// Initializes DOTween with project-wide settings.
    /// Attach to GameManager or create as separate object in Core scene.
    /// </summary>
    public class DOTweenInitializer : MonoBehaviour
    {
        [Header("DOTween Settings")]
        [SerializeField] private int tweenCapacity = 200;
        [SerializeField] private int sequenceCapacity = 50;
        [SerializeField] private bool recycleAllByDefault = false;
        [SerializeField] private bool useSafeMode = true;
        [SerializeField] private LogBehaviour logBehaviour = LogBehaviour.ErrorsOnly;

        private void Awake()
        {
            // Initialize DOTween
            DOTween.Init(recycleAllByDefault, useSafeMode, logBehaviour);
            DOTween.SetTweensCapacity(tweenCapacity, sequenceCapacity);

            // Default settings
            DOTween.defaultAutoPlay = AutoPlay.All;
            DOTween.defaultAutoKill = true;
            DOTween.defaultEaseType = Ease.OutQuad;

            Debug.Log("[DOTween] Initialized with capacity: " + tweenCapacity);
        }

        private void OnDestroy()
        {
            DOTween.KillAll();
        }
    }
}