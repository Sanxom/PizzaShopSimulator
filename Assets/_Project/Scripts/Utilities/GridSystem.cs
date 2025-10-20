using UnityEngine;
using System;
using System.Collections.Generic;

namespace PizzaShop.Utilities
{
    /// <summary>
    /// Generic 2D grid system for spatial management.
    /// Type T must be MonoBehaviour for transform access.
    /// Performance-optimized with O(1) lookups.
    /// </summary>
    public class GridSystem<T> where T : MonoBehaviour
    {
        private T[,] grid;
        private readonly int width;
        private readonly int depth;
        private readonly float spacing;
        private readonly Transform parent;
        private readonly Vector3 originOffset;

        public int Width => width;
        public int Depth => depth;
        public float Spacing => spacing;

        public GridSystem(int width, int depth, float spacing, Transform parent, Vector3 originOffset = default)
        {
            this.width = width;
            this.depth = depth;
            this.spacing = spacing;
            this.parent = parent;
            this.originOffset = originOffset;

            grid = new T[width, depth];
        }

        /// <summary>
        /// Set item at grid coordinates.
        /// </summary>
        public void SetCell(int x, int z, T item)
        {
            if (!IsValidCell(x, z))
            {
                Debug.LogWarning($"[GridSystem] Invalid cell coordinates: ({x}, {z})");
                return;
            }

            grid[x, z] = item;
        }

        /// <summary>
        /// Get item at grid coordinates.
        /// </summary>
        public T GetCell(int x, int z)
        {
            return IsValidCell(x, z) ? grid[x, z] : null;
        }

        /// <summary>
        /// Check if cell is occupied.
        /// </summary>
        public bool IsCellOccupied(int x, int z)
        {
            return IsValidCell(x, z) && grid[x, z] != null;
        }

        /// <summary>
        /// Clear cell at coordinates.
        /// </summary>
        public void ClearCell(int x, int z)
        {
            if (IsValidCell(x, z))
            {
                grid[x, z] = null;
            }
        }

        /// <summary>
        /// Convert grid coordinates to world position.
        /// </summary>
        public Vector3 GetWorldPosition(int x, int z)
        {
            Vector3 localPos = new Vector3(
                (x - width / 2f) * spacing + spacing / 2f,
                0,
                (z - depth / 2f) * spacing + spacing / 2f
            );

            return parent.position + parent.TransformDirection(localPos) + originOffset;
        }

        /// <summary>
        /// Convert world position to grid coordinates.
        /// </summary>
        public (int x, int z) GetGridPosition(Vector3 worldPosition)
        {
            Vector3 localPos = parent.InverseTransformPoint(worldPosition) - originOffset;

            int x = Mathf.RoundToInt((localPos.x + width * spacing / 2f - spacing / 2f) / spacing);
            int z = Mathf.RoundToInt((localPos.z + depth * spacing / 2f - spacing / 2f) / spacing);

            return (x, z);
        }

        /// <summary>
        /// Find nearest empty cell to world position.
        /// Returns null if no empty cells.
        /// </summary>
        public (int x, int z)? GetNearestEmptyCell(Vector3 worldPosition)
        {
            var (startX, startZ) = GetGridPosition(worldPosition);

            // Clamp to grid bounds
            startX = Mathf.Clamp(startX, 0, width - 1);
            startZ = Mathf.Clamp(startZ, 0, depth - 1);

            // Check starting position first
            if (!IsCellOccupied(startX, startZ))
            {
                return (startX, startZ);
            }

            // Spiral search outward
            for (int radius = 1; radius < Mathf.Max(width, depth); radius++)
            {
                for (int x = startX - radius; x <= startX + radius; x++)
                {
                    for (int z = startZ - radius; z <= startZ + radius; z++)
                    {
                        // Only check perimeter of current radius
                        if (Mathf.Abs(x - startX) != radius && Mathf.Abs(z - startZ) != radius)
                            continue;

                        if (IsValidCell(x, z) && !IsCellOccupied(x, z))
                        {
                            return (x, z);
                        }
                    }
                }
            }

            return null; // No empty cells found
        }

        /// <summary>
        /// Get all items in grid.
        /// </summary>
        public List<T> GetAllItems()
        {
            List<T> items = new List<T>();

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (grid[x, z] != null)
                    {
                        items.Add(grid[x, z]);
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// Get all empty cell coordinates.
        /// </summary>
        public List<(int x, int z)> GetEmptyCells()
        {
            List<(int x, int z)> emptyCells = new List<(int x, int z)>();

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (grid[x, z] == null)
                    {
                        emptyCells.Add((x, z));
                    }
                }
            }

            return emptyCells;
        }

        /// <summary>
        /// Clear entire grid.
        /// </summary>
        public void Clear()
        {
            Array.Clear(grid, 0, grid.Length);
        }

        /// <summary>
        /// Check if coordinates are within grid bounds.
        /// </summary>
        public bool IsValidCell(int x, int z)
        {
            return x >= 0 && x < width && z >= 0 && z < depth;
        }

        /// <summary>
        /// Get grid dimensions.
        /// </summary>
        public (int width, int depth) GetDimensions()
        {
            return (width, depth);
        }
    }
}