using UnityEngine;
using DG.Tweening;
using PizzaShop.Core;

namespace PizzaShop.Equipment
{
    /// <summary>
    /// Individual slot on make table that can hold a container.
    /// Handles placement, highlighting, and visual feedback.
    /// </summary>
    public class ContainerSlot : MonoBehaviour
    {
        [Header("Slot Configuration")]
        [SerializeField] private Vector2Int gridPosition;

        [Header("Visual Components")]
        [SerializeField] private MeshRenderer slotRenderer;
        [SerializeField] private Transform placementPoint;

        [Header("Highlight Settings")]
        [SerializeField] private Color normalColor = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color highlightColor = new Color(0.5f, 0.8f, 1f);
        [SerializeField] private Color occupiedColor = new Color(0.8f, 0.3f, 0.3f);
        [SerializeField] private float highlightDuration = 0.2f;

        private Container currentContainer;
        private MaterialPropertyBlock propertyBlock;
        private Tweener highlightTween;
        private bool isHighlighted;

        public Vector2Int GridPosition => gridPosition;
        public bool IsOccupied => currentContainer != null;
        public Container CurrentContainer => currentContainer;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();

            if (slotRenderer == null)
                slotRenderer = GetComponent<MeshRenderer>();

            if (placementPoint == null)
                placementPoint = transform;

            SetColor(normalColor, immediate: true);
        }

        /// <summary>
        /// Set grid position (called by MakeTable during initialization).
        /// </summary>
        public void Initialize(Vector2Int position)
        {
            gridPosition = position;
            gameObject.name = $"Slot_{position.x}_{position.y}";
        }

        /// <summary>
        /// Place container in this slot.
        /// </summary>
        public void PlaceContainer(Container container)
        {
            if (currentContainer != null)
            {
                Debug.LogWarning($"[ContainerSlot] Slot {gridPosition} already occupied!");
                return;
            }

            currentContainer = container;
            container.SetSlot(this);

            SetColor(occupiedColor);

            Debug.Log($"[ContainerSlot] Container placed at {gridPosition}");
        }

        /// <summary>
        /// Remove container from this slot.
        /// </summary>
        public void RemoveContainer()
        {
            if (currentContainer == null)
            {
                Debug.LogWarning($"[ContainerSlot] Slot {gridPosition} is empty!");
                return;
            }

            currentContainer = null;
            SetColor(normalColor);

            Debug.Log($"[ContainerSlot] Container removed from {gridPosition}");
        }

        /// <summary>
        /// Highlight this slot.
        /// </summary>
        public void Highlight()
        {
            if (isHighlighted) return;

            isHighlighted = true;
            SetColor(highlightColor);
        }

        /// <summary>
        /// Remove highlight from this slot.
        /// </summary>
        public void Unhighlight()
        {
            if (!isHighlighted) return;

            isHighlighted = false;
            Color targetColor = IsOccupied ? occupiedColor : normalColor;
            SetColor(targetColor);
        }

        /// <summary>
        /// Get world position for container placement.
        /// </summary>
        public Vector3 GetPlacementPosition()
        {
            return placementPoint.position;
        }

        /// <summary>
        /// Set slot color with optional animation.
        /// </summary>
        private void SetColor(Color color, bool immediate = false)
        {
            if (slotRenderer == null) return;

            highlightTween?.Kill();

            if (immediate)
            {
                propertyBlock.SetColor("_BaseColor", color);
                slotRenderer.SetPropertyBlock(propertyBlock);
            }
            else
            {
                Color currentColor = slotRenderer.sharedMaterial.GetColor("_BaseColor");
                highlightTween = DOVirtual.Color(currentColor, color, highlightDuration, c =>
                {
                    propertyBlock.SetColor("_BaseColor", c);
                    slotRenderer.SetPropertyBlock(propertyBlock);
                });
            }
        }

        private void OnDestroy()
        {
            highlightTween?.Kill();
        }

        #region Debug Visualization
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (placementPoint != null)
            {
                Gizmos.color = IsOccupied ? Color.red : Color.green;
                Gizmos.DrawWireSphere(placementPoint.position, 0.1f);

                // Draw grid position
                UnityEditor.Handles.Label(
                    transform.position + Vector3.up * 0.5f,
                    $"({gridPosition.x}, {gridPosition.y})"
                );
            }
        }
#endif
        #endregion
    }
}