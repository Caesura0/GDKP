using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject storing the baked grid snapshot.
/// Created once in your project (Assets/Grid/GridBakeData.asset) and
/// referenced by GridEdgeBlockerData for scene-view gizmo drawing.
/// Updated whenever you click "Bake Grid" in the Grid Edge Painter window.
/// </summary>
[CreateAssetMenu(fileName = "GridBakeData", menuName = "Grid/Grid Bake Data")]
public class GridBakeData : ScriptableObject
{
    [Serializable]
    public struct CellData
    {
        public int x;
        public int z;
        public bool isWalkable;
        public float worldY;
    }

    // Grid dimensions at bake time — used to detect stale data
    public int bakedWidth;
    public int bakedHeight;
    public float bakedCellSize;

    public List<CellData> cells = new List<CellData>();

    /// <summary>Fast lookup: is this position walkable according to the last bake?</summary>
    public bool IsWalkable(int x, int z)
    {
        // Linear scan is fine — this is editor-only display, not runtime pathfinding
        foreach (var cell in cells)
            if (cell.x == x && cell.z == z) return cell.isWalkable;
        return true;
    }

    public float GetWorldY(int x, int z)
    {
        foreach (var cell in cells)
            if (cell.x == x && cell.z == z) return cell.worldY;
        return 0f;
    }
}