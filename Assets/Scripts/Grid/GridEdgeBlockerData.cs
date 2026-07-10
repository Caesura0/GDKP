using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Attach to the same GameObject as LevelGrid.
/// - Stores serialized blocked edges (painted in the Grid Edge Painter window)
/// - Holds a reference to GridBakeData (baked walkability + terrain Y snapshot)
/// - Draws everything in the Scene view via OnDrawGizmos — no play mode needed
/// - Pushes blocked edges into PathNode at runtime
/// </summary>
public class GridEdgeBlockerData : MonoBehaviour
{
    public static GridEdgeBlockerData Instance { get; private set; }

    // ── Edge data ─────────────────────────────────────────────────────────────

    [Serializable]
    public struct GridEdge
    {
        public int ax, az, bx, bz;

        public GridEdge(int ax, int az, int bx, int bz)
        {
            this.ax = ax; this.az = az;
            this.bx = bx; this.bz = bz;
        }

        public GridPosition A => new GridPosition(ax, az);
        public GridPosition B => new GridPosition(bx, bz);

        public bool Matches(GridPosition a, GridPosition b) =>
            (A == a && B == b) || (A == b && B == a);
    }

    [SerializeField] LevelGrid levelGrid;


    [SerializeField] private List<GridEdge> blockedEdges = new List<GridEdge>();

    // ── Bake reference ────────────────────────────────────────────────────────

    /// <summary>Assigned automatically when you bake in the Grid Edge Painter window.</summary>
    [SerializeField] public GridBakeData bakeData;

    // ── Gizmo toggles (set in Inspector) ─────────────────────────────────────

    [Header("Scene View Overlays")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private bool showWalkability = true;
    [SerializeField] private bool showBlockedEdges = true;
    [SerializeField] private bool showBounds = true;
    [SerializeField] private bool showCoordinates = false;

    [Header("Colours")]
    [SerializeField] private Color colWalkable = new Color(0.20f, 0.80f, 0.30f, 0.25f);
    [SerializeField] private Color colUnwalkable = new Color(0.90f, 0.20f, 0.20f, 0.35f);
    [SerializeField] private Color colGrid = new Color(0.60f, 0.60f, 0.65f, 0.40f);
    [SerializeField] private Color colBounds = new Color(1.00f, 0.85f, 0.20f, 0.90f);
    [SerializeField] private Color colBlockedEdge = new Color(0.95f, 0.15f, 0.15f, 1.00f);

    // ── Runtime ───────────────────────────────────────────────────────────────

    private void Awake() => Instance = this;

    private void Start() => ApplyToPathFinding();

    /// <summary>
    /// Pushes blocked edges into PathFinding at runtime.
    /// Called by LevelGrid.Start() after PathFinding.Setup() completes.
    /// </summary>
    /// <summary>
    /// Pushes blocked edges into PathFinding at runtime.
    /// Called by LevelGrid.Start() after PathFinding.Setup() completes.
    /// </summary>
    public void ApplyToPathFinding()
    {
        foreach (GridEdge edge in blockedEdges)
            PathFinding.Instance.BlockEdge(edge.A, edge.B);
    }

    // ── Edge API ──────────────────────────────────────────────────────────────

    public List<GridEdge> GetBlockedEdges() => blockedEdges;

    public bool HasEdge(GridPosition a, GridPosition b)
    {
        foreach (GridEdge edge in blockedEdges)
            if (edge.Matches(a, b)) return true;
        return false;
    }

    public void AddEdge(GridPosition a, GridPosition b)
    {
        if (!HasEdge(a, b))
            blockedEdges.Add(new GridEdge(a.x, a.z, b.x, b.z));
    }

    public void RemoveEdge(GridPosition a, GridPosition b) =>
        blockedEdges.RemoveAll(e => e.Matches(a, b));

    public void ClearAllEdges() => blockedEdges.Clear();

    // ── Scene gizmos ──────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {

        //if (levelGrid == null) return;

        //int w = levelGrid.GetWidth();
        //int h = levelGrid.GetHeight();
        //float cellSize = levelGrid.GetCellSize();
        var so = new SerializedObject(levelGrid);
        int w = so.FindProperty("width").intValue;
        int h = so.FindProperty("height").intValue;
        float cellSize = so.FindProperty("cellSize").floatValue;



        bool hasBake = bakeData != null && bakeData.cells != null && bakeData.cells.Count > 0;

        // ── Per-cell: walkability fill + grid lines ───────────────────────────
        for (int x = 0; x < w; x++)
        {
            for (int z = 0; z < h; z++)
            {
                // World centre of this cell, at baked Y (or 0 if not baked yet)
                float worldY = hasBake ? bakeData.GetWorldY(x, z) : 0f;
                Vector3 centre = new Vector3(x * cellSize, worldY + 0.05f, z * cellSize);
                Vector3 size = new Vector3(cellSize, 0f, cellSize);

                // Walkability fill
                if (showWalkability && hasBake)
                {
                    bool walkable = bakeData.IsWalkable(x, z);
                    Gizmos.color = walkable ? colWalkable : colUnwalkable;
                    Gizmos.DrawCube(centre, size * 0.92f);
                }

                // Grid lines (draw cell borders as wire cube with near-zero Y)
                if (showGrid)
                {
                    Gizmos.color = colGrid;
                    Gizmos.DrawWireCube(centre, new Vector3(cellSize, 0.01f, cellSize));
                }

                // Coordinates (editor-only label via Handles, small text overhead)
#if UNITY_EDITOR
                if (showCoordinates)
                {
                    UnityEditor.Handles.color = Color.white;
                    UnityEditor.Handles.Label(
                        centre + Vector3.up * 0.3f,
                        $"{x},{z}",
                        new GUIStyle { fontSize = 9, normal = { textColor = Color.white } });
                }
#endif
            }
        }

        // ── Grid bounds outline ───────────────────────────────────────────────
        if (showBounds)
        {
            Gizmos.color = colBounds;

            // Bottom-left to bottom-right
            Vector3 bl = new Vector3(-cellSize * 0.5f, 0.1f, -cellSize * 0.5f);
            Vector3 br = new Vector3(w * cellSize - cellSize * 0.5f, 0.1f, -cellSize * 0.5f);
            Vector3 tl = new Vector3(-cellSize * 0.5f, 0.1f, h * cellSize - cellSize * 0.5f);
            Vector3 tr = new Vector3(w * cellSize - cellSize * 0.5f, 0.1f, h * cellSize - cellSize * 0.5f);

            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);
            Gizmos.DrawLine(tl, bl);

            // Corner dots
            float r = cellSize * 0.07f;
            Gizmos.DrawSphere(bl, r);
            Gizmos.DrawSphere(br, r);
            Gizmos.DrawSphere(tl, r);
            Gizmos.DrawSphere(tr, r);
        }

        // ── Blocked edges ─────────────────────────────────────────────────────
        if (showBlockedEdges && blockedEdges != null)
        {
            Gizmos.color = colBlockedEdge;

            foreach (GridEdge edge in blockedEdges)
            {
                float yA = hasBake ? bakeData.GetWorldY(edge.ax, edge.az) : 0f;
                float yB = hasBake ? bakeData.GetWorldY(edge.bx, edge.bz) : 0f;
                float y = Mathf.Max(yA, yB) + 0.15f;

                Vector3 worldA = new Vector3(edge.ax * cellSize, y, edge.az * cellSize);
                Vector3 worldB = new Vector3(edge.bx * cellSize, y, edge.bz * cellSize);

                Vector3 mid = (worldA + worldB) * 0.5f;
                Vector3 dir = (worldB - worldA).normalized;
                Vector3 perp = new Vector3(-dir.z, 0f, dir.x);

                Vector3 lineStart = mid + perp * (cellSize * 0.45f);
                Vector3 lineEnd = mid - perp * (cellSize * 0.45f);

                Gizmos.DrawLine(lineStart, lineEnd);
                Gizmos.DrawSphere(mid, cellSize * 0.04f);
            }
        }
    }
}