using UnityEngine;
using System.Collections.Generic;

namespace PizzaShop.Utilities
{
    /// <summary>
    /// Custom outline effect for highlighting interactable objects.
    /// Uses mesh duplication and scaling technique.
    /// </summary>
    [DisallowMultipleComponent]
    public class Outline : MonoBehaviour
    {
        [Header("Outline Settings")]
        [SerializeField] private Color outlineColor = Color.yellow;
        [SerializeField] private float outlineWidth = 5f;
        [SerializeField] private bool precomputeOutline = false;

        [Header("Optional")]
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Material outlineMaskMaterial;
        [SerializeField] private Material outlineFillMaterial;

        private bool needsUpdate = false;

        // Property accessors
        public Color OutlineColor
        {
            get => outlineColor;
            set
            {
                outlineColor = value;
                needsUpdate = true;
            }
        }

        public float OutlineWidth
        {
            get => outlineWidth;
            set
            {
                outlineWidth = value;
                needsUpdate = true;
            }
        }

        private void Awake()
        {
            // Load or create outline materials
            if (outlineMaskMaterial == null)
            {
                outlineMaskMaterial = new Material(Shader.Find("Hidden/OutlineMask"));
            }

            if (outlineFillMaterial == null)
            {
                outlineFillMaterial = new Material(Shader.Find("Hidden/OutlineFill"));
            }

            // Find all renderers if not assigned
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>();
            }

            // Initial setup
            UpdateMaterialProperties();
        }

        private void OnEnable()
        {
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                // Add outline materials to renderer
                var materials = new List<Material>(renderer.sharedMaterials);
                materials.Add(outlineMaskMaterial);
                materials.Add(outlineFillMaterial);
                renderer.materials = materials.ToArray();
            }

            UpdateMaterialProperties();
        }

        private void OnDisable()
        {
            foreach (var renderer in renderers)
            {
                if (renderer == null) continue;

                // Remove outline materials
                var materials = new List<Material>(renderer.sharedMaterials);
                materials.RemoveAll(m => m == outlineMaskMaterial || m == outlineFillMaterial);
                renderer.materials = materials.ToArray();
            }
        }

        private void Update()
        {
            if (needsUpdate)
            {
                UpdateMaterialProperties();
                needsUpdate = false;
            }
        }

        private void UpdateMaterialProperties()
        {
            if (outlineFillMaterial != null)
            {
                outlineFillMaterial.SetColor("_OutlineColor", outlineColor);
                outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
            }
        }

        private void OnDestroy()
        {
            // Clean up materials
            if (outlineMaskMaterial != null)
            {
                Destroy(outlineMaskMaterial);
            }

            if (outlineFillMaterial != null)
            {
                Destroy(outlineFillMaterial);
            }
        }

        private void OnValidate()
        {
            needsUpdate = true;
        }
    }
}