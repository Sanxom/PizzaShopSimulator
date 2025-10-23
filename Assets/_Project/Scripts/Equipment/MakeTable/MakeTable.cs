using UnityEngine;
using System.Collections.Generic;
using PizzaShop.Data;
using PizzaShop.Core;
using PizzaShop.Utilities;
using DG.Tweening;

namespace PizzaShop.Equipment
{
    /// <summary>
    /// Main make table component managing grid and assembly zones.
    /// Handles container placement and pizza assembly workflow.
    /// </summary>
    public class MakeTable : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private MakeTableData tableData;

        [Header("Prefabs")]
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private GameObject assemblyZonePrefab;

        [Header("Parent Transforms")]
        [SerializeField] private Transform slotsParent;
        [SerializeField] private Transform zonesParent;

        private GridSystem<ContainerSlot> containerGrid;
        private List<AssemblyZone> assemblyZones;
        private ContainerSlot highlightedSlot;

        public MakeTableData Data => tableData;
        public GridSystem<ContainerSlot> Grid => containerGrid;
        public IReadOnlyList<AssemblyZone> AssemblyZones => assemblyZones;

        private void Awake()
        {
            if (tableData == null)
            {
                Debug.LogError("[MakeTable] No table data assigned!");
                return;
            }

            assemblyZones = new List<AssemblyZone>();

            InitializeGrid();
            InitializeAssemblyZones();
        }

        /// <summary>
        /// Initialize container grid system.
        /// </summary>
        private void InitializeGrid()
        {
            if (slotsParent == null)
            {
                GameObject parent = new GameObject("Slots");
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
                slotsParent = parent.transform;
            }

            // Create grid system
            containerGrid = new GridSystem<ContainerSlot>(
                tableData.GridWidth,
                tableData.GridDepth,
                tableData.SlotSpacing,
                slotsParent,
                tableData.GridOffset
            );

            // Create visual slots
            for (int x = 0; x < tableData.GridWidth; x++)
            {
                for (int z = 0; z < tableData.GridDepth; z++)
                {
                    CreateSlot(x, z);
                }
            }

            Debug.Log($"[MakeTable] Initialized {tableData.GridWidth}x{tableData.GridDepth} grid");
        }

        /// <summary>
        /// Create individual slot GameObject.
        /// </summary>
        private void CreateSlot(int x, int z)
        {
            Vector3 worldPos = containerGrid.GetWorldPosition(x, z);

            GameObject slotObj = slotPrefab != null
                ? Instantiate(slotPrefab, worldPos, Quaternion.identity, slotsParent)
                : CreateDefaultSlot(worldPos);

            ContainerSlot slot = slotObj.GetComponent<ContainerSlot>();
            if (slot == null)
            {
                slot = slotObj.AddComponent<ContainerSlot>();
            }

            slot.Initialize(new Vector2Int(x, z));
            containerGrid.SetCell(x, z, slot);
        }

        /// <summary>
        /// Create default slot if no prefab provided.
        /// </summary>
        private GameObject CreateDefaultSlot(Vector3 position)
        {
            GameObject slotObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            slotObj.transform.position = position;
            slotObj.transform.localScale = 0.9f * tableData.SlotSpacing * Vector3.one;
            slotObj.transform.SetParent(slotsParent);

            // Remove collider (we'll use raycasting from player)
            Destroy(slotObj.GetComponent<Collider>());

            return slotObj;
        }

        /// <summary>
        /// Initialize assembly zones.
        /// </summary>
        private void InitializeAssemblyZones()
        {
            if (zonesParent == null)
            {
                GameObject parent = new GameObject("AssemblyZones");
                parent.transform.SetParent(transform);
                parent.transform.localPosition = Vector3.zero;
                zonesParent = parent.transform;
            }

            // Create zones at the back of the table
            int zoneRow = tableData.GridDepth - 1;
            int zonesPerRow = Mathf.Min(tableData.AssemblyZoneCount, tableData.GridWidth);

            for (int i = 0; i < zonesPerRow; i++)
            {
                int x = i * (tableData.GridWidth / zonesPerRow);
                CreateAssemblyZone(x, zoneRow, i);
            }

            Debug.Log($"[MakeTable] Created {assemblyZones.Count} assembly zones");
        }

        /// <summary>
        /// Create individual assembly zone.
        /// </summary>
        private void CreateAssemblyZone(int x, int z, int index)
        {
            Vector3 worldPos = containerGrid.GetWorldPosition(x, z);

            GameObject zoneObj = assemblyZonePrefab != null
                ? Instantiate(assemblyZonePrefab, worldPos, Quaternion.identity, zonesParent)
                : CreateDefaultZone(worldPos);

            if (!zoneObj.TryGetComponent<AssemblyZone>(out var zone))
            {
                zone = zoneObj.AddComponent<AssemblyZone>();
            }

            // Assign size based on configuration
            Orders.PizzaSize size = index < tableData.SupportedSizes.Length
                ? tableData.SupportedSizes[index]
                : Orders.PizzaSize.Large;

            zone.Initialize(new Vector2Int(x, z), size);
            assemblyZones.Add(zone);
        }

        /// <summary>
        /// Create default assembly zone if no prefab provided.
        /// </summary>
        private GameObject CreateDefaultZone(Vector3 position)
        {
            GameObject zoneObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            zoneObj.transform.position = position;
            zoneObj.transform.localScale = new Vector3(
                tableData.SlotSpacing * 0.8f,
                0.1f,
                tableData.SlotSpacing * 0.8f
            );
            zoneObj.transform.SetParent(zonesParent);

            return zoneObj;
        }

        // ==================== CONTAINER PLACEMENT ====================

        /// <summary>
        /// Try to place container at world position.
        /// Returns true if successful.
        /// </summary>
        public bool TryPlaceContainer(Container container, Vector3 worldPosition)
        {
            if (container == null)
            {
                Debug.LogWarning("[MakeTable] Cannot place null container!");
                return false;
            }

            // Find nearest empty slot
            var nearestEmpty = containerGrid.GetNearestEmptyCell(worldPosition);

            if (!nearestEmpty.HasValue)
            {
                Debug.LogWarning("[MakeTable] No empty slots available!");
                return false;
            }

            var (x, z) = nearestEmpty.Value;
            ContainerSlot slot = containerGrid.GetCell(x, z);

            if (slot == null)
            {
                Debug.LogError($"[MakeTable] Slot at ({x}, {z}) is null!");
                return false;
            }

            // Place container on slot
            container.PlaceOnSlot(slot);

            return true;
        }

        /// <summary>
        /// Try to place container at specific grid coordinates.
        /// </summary>
        public bool TryPlaceContainerAt(Container container, int x, int z)
        {
            if (container == null || !containerGrid.IsValidCell(x, z))
            {
                return false;
            }

            ContainerSlot slot = containerGrid.GetCell(x, z);

            if (slot == null || slot.IsOccupied)
            {
                return false;
            }

            container.PlaceOnSlot(slot);
            return true;
        }

        /// <summary>
        /// Get container at grid position.
        /// </summary>
        public Container GetContainerAt(int x, int z)
        {
            if (!containerGrid.IsValidCell(x, z))
                return null;

            ContainerSlot slot = containerGrid.GetCell(x, z);
            return slot?.CurrentContainer;
        }

        /// <summary>
        /// Get all containers on table.
        /// </summary>
        public List<Container> GetAllContainers()
        {
            List<Container> containers = new List<Container>();
            var slots = containerGrid.GetAllItems();

            foreach (var slot in slots)
            {
                if (slot.IsOccupied)
                {
                    containers.Add(slot.CurrentContainer);
                }
            }

            return containers;
        }

        /// <summary>
        /// Check if there are any empty slots.
        /// </summary>
        public bool HasEmptySlot()
        {
            return containerGrid.GetEmptyCells().Count > 0;
        }

        /// <summary>
        /// Get number of empty slots.
        /// </summary>
        public int GetEmptySlotCount()
        {
            return containerGrid.GetEmptyCells().Count;
        }

        // ==================== SLOT HIGHLIGHTING ====================

        /// <summary>
        /// Highlight nearest slot to world position.
        /// </summary>
        public void HighlightNearestSlot(Vector3 worldPosition)
        {
            var (x, z) = containerGrid.GetGridPosition(worldPosition);
            HighlightSlot(x, z);
        }

        /// <summary>
        /// Highlight slot at grid coordinates.
        /// </summary>
        public void HighlightSlot(int x, int z)
        {
            // Clear previous highlight
            ClearHighlight();

            if (!containerGrid.IsValidCell(x, z))
                return;

            ContainerSlot slot = containerGrid.GetCell(x, z);
            if (slot != null && !slot.IsOccupied)
            {
                slot.Highlight();
                highlightedSlot = slot;
            }
        }

        /// <summary>
        /// Clear slot highlight.
        /// </summary>
        public void ClearHighlight()
        {
            if (highlightedSlot != null)
            {
                highlightedSlot.Unhighlight();
                highlightedSlot = null;
            }
        }

        // ==================== ASSEMBLY ZONES ====================

        /// <summary>
        /// Get assembly zone for specific pizza size.
        /// </summary>
        public AssemblyZone GetAssemblyZone(Orders.PizzaSize size)
        {
            foreach (var zone in assemblyZones)
            {
                if (zone.SupportedSize == size && !zone.HasPizza)
                {
                    return zone;
                }
            }
            return null;
        }

        /// <summary>
        /// Get first available assembly zone.
        /// </summary>
        public AssemblyZone GetAvailableAssemblyZone()
        {
            foreach (var zone in assemblyZones)
            {
                if (!zone.HasPizza)
                {
                    return zone;
                }
            }
            return null;
        }

        /// <summary>
        /// Check if any assembly zones are available.
        /// </summary>
        public bool HasAvailableAssemblyZone()
        {
            return GetAvailableAssemblyZone() != null;
        }

        // ==================== UTILITY ====================

        /// <summary>
        /// Get world position for grid coordinates.
        /// </summary>
        public Vector3 GetWorldPosition(int x, int z)
        {
            return containerGrid.GetWorldPosition(x, z);
        }

        /// <summary>
        /// Get grid position for world coordinates.
        /// </summary>
        public (int x, int z) GetGridPosition(Vector3 worldPosition)
        {
            return containerGrid.GetGridPosition(worldPosition);
        }

        /// <summary>
        /// Clear all containers from table.
        /// </summary>
        public void ClearAllContainers()
        {
            var containers = GetAllContainers();
            foreach (var container in containers)
            {
                if (container.CurrentSlot != null)
                {
                    container.CurrentSlot.RemoveContainer();
                }
            }

            Debug.Log("[MakeTable] All containers cleared");
        }

        #region Debug Visualization
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (tableData == null || containerGrid == null)
                return;

            // Draw grid
            Gizmos.color = Color.cyan;
            for (int x = 0; x < tableData.GridWidth; x++)
            {
                for (int z = 0; z < tableData.GridDepth; z++)
                {
                    Vector3 pos = containerGrid.GetWorldPosition(x, z);
                    Gizmos.DrawWireCube(pos, Vector3.one * tableData.SlotSpacing * 0.9f);
                }
            }

            // Draw assembly zones
            Gizmos.color = Color.green;
            foreach (var zone in assemblyZones)
            {
                Gizmos.DrawWireSphere(zone.transform.position, tableData.SlotSpacing * 0.4f);
            }
        }
#endif
        #endregion
    }
}