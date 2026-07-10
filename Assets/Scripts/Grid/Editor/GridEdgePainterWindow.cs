// Assets/Grid/Editor/GridEdgePainterWindow.cs
// Open via: Window > Grid Edge Painter
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridEdgePainterWindow : EditorWindow
{
    // ── State ─────────────────────────────────────────────────────────────────
    private LevelGrid levelGrid;
    private GridEdgeBlockerData blockerData;
    private PathFinding pathFinding;

    private int gridWidth;
    private int gridHeight;
    private float cellSize;

    // Bake status
    private string bakeStatus = "";
    private bool bakeIsError = false;
    private double bakeStatusAt = 0;

    // How large each cell is drawn in the window (px)
    private const float CELL_PX = 40f;
    private const float PADDING = 20f;
    private const float EDGE_HIT_PX = 10f;

    private Vector2 scrollPos;

    // ── Colours ───────────────────────────────────────────────────────────────
    private static readonly Color COL_WALKABLE = new Color(0.22f, 0.65f, 0.30f, 1f);
    private static readonly Color COL_UNWALKABLE = new Color(0.75f, 0.18f, 0.18f, 1f);
    private static readonly Color COL_CELL_DEFAULT = new Color(0.25f, 0.25f, 0.30f, 1f);
    private static readonly Color COL_CELL_HOVER = new Color(0.35f, 0.35f, 0.42f, 1f);
    private static readonly Color COL_GRID_LINE = new Color(0.50f, 0.50f, 0.55f, 1f);
    private static readonly Color COL_BLOCKED = new Color(0.90f, 0.20f, 0.20f, 1f);
    private static readonly Color COL_BLOCKED_HOV = new Color(1.00f, 0.50f, 0.50f, 1f);
    private static readonly Color COL_COORD = new Color(0.85f, 0.85f, 0.90f, 1f);
    private static readonly Color COL_BOUNDS = new Color(1.00f, 0.85f, 0.20f, 1f);

    // ── Menu item ─────────────────────────────────────────────────────────────
    [MenuItem("Window/Grid Edge Painter")]
    public static void Open()
    {
        var window = GetWindow<GridEdgePainterWindow>("Grid Edge Painter");
        window.minSize = new Vector2(400, 320);
        window.Show();
    }

    // ── GUI ───────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        DrawToolbar();
        if (!ValidateSetup()) return;

        DrawBakeBar();

        float canvasW = gridWidth * CELL_PX + PADDING * 2;
        float canvasH = gridHeight * CELL_PX + PADDING * 2;

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        Rect canvasRect = GUILayoutUtility.GetRect(canvasW, canvasH);
        DrawCanvas(canvasRect);
        HandleInput(canvasRect); // Handle input while still in scroll view coordinates
        EditorGUILayout.EndScrollView();

        DrawLegend();
    }

    // ── Toolbar ───────────────────────────────────────────────────────────────
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        EditorGUI.BeginChangeCheck();
        levelGrid = (LevelGrid)EditorGUILayout.ObjectField(
            "Level Grid", levelGrid, typeof(LevelGrid), true,
            GUILayout.Width(320));
        if (EditorGUI.EndChangeCheck()) RefreshFromLevelGrid();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(65)))
            RefreshFromLevelGrid();

        GUI.enabled = blockerData != null;
        if (GUILayout.Button("Clear All", EditorStyles.toolbarButton, GUILayout.Width(65)))
        {
            if (EditorUtility.DisplayDialog("Clear All Edges",
                "Remove all blocked edges?", "Clear", "Cancel"))
            {
                Undo.RecordObject(blockerData, "Clear All Grid Edges");
                blockerData.ClearAllEdges();
                EditorUtility.SetDirty(blockerData);
            }
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
    }

    // ── Bake bar ──────────────────────────────────────────────────────────────
    private void DrawBakeBar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        // Bake button
        var bakeStyle = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            fixedHeight = 22f
        };

        bool canBake = levelGrid != null && blockerData != null && pathFinding != null;
        GUI.enabled = canBake;

        if (GUILayout.Button("▶  Bake Grid", bakeStyle, GUILayout.Width(110)))
            BakeGrid();

        GUI.enabled = true;

        // Bake data asset field
        if (blockerData != null)
        {
            EditorGUI.BeginChangeCheck();
            blockerData.bakeData = (GridBakeData)EditorGUILayout.ObjectField(
                blockerData.bakeData, typeof(GridBakeData), false);
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(blockerData);
        }

        GUILayout.FlexibleSpace();

        // Status message (fades after 4 seconds)
        if (!string.IsNullOrEmpty(bakeStatus))
        {
            double elapsed = EditorApplication.timeSinceStartup - bakeStatusAt;
            if (elapsed < 4.0)
            {
                var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = bakeIsError ? new Color(1f, 0.4f, 0.4f) : new Color(0.4f, 1f, 0.5f) }
                };
                GUILayout.Label(bakeStatus, statusStyle);
                Repaint(); // keep repainting so fade works
            }
            else
            {
                bakeStatus = "";
            }
        }

        EditorGUILayout.EndHorizontal();

        // Warn if no bake data asset assigned
        if (blockerData != null && blockerData.bakeData == null)
        {
            EditorGUILayout.HelpBox(
                "No GridBakeData asset assigned. Create one via Assets > Create > Grid > Grid Bake Data, " +
                "then drag it into the field above. Baking will populate it.",
                MessageType.Warning);
        }
    }

    // ── Bake logic ────────────────────────────────────────────────────────────
    private void BakeGrid()
    {
        if (blockerData.bakeData == null)
        {
            SetBakeStatus("Assign a GridBakeData asset first.", true);
            return;
        }

        // Read layer masks directly from the components
        LayerMask walkableMask = levelGrid.GetWalkableLayers();
        LayerMask obstaclesMask = pathFinding.GetObstaclesLayerMask();

        Undo.RecordObject(blockerData.bakeData, "Bake Grid Data");

        var data = blockerData.bakeData;
        data.bakedWidth = gridWidth;
        data.bakedHeight = gridHeight;
        data.bakedCellSize = cellSize;
        data.cells.Clear();

        float raycastHeight = 5f;
        int unwalkable = 0;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 flatPos = new Vector3(x * cellSize, 0f, z * cellSize);
                Vector3 rayOrigin = flatPos + Vector3.up * raycastHeight;

                // ── Terrain Y (same logic as LevelGrid.SampleWalkableHeights) ──
                float worldY = 0f;
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit terrainHit,
                    raycastHeight * 2f, walkableMask))
                {
                    worldY = terrainHit.point.y;
                }

                // ── Walkability (same logic as PathFinding.Setup) ──────────────
                float raycastOffset = 4f;
                bool hasObstacle = Physics.Raycast(
                    flatPos + Vector3.down * raycastOffset,
                    Vector3.up,
                    raycastOffset * 2f,
                    obstaclesMask);

                bool isWalkable = !hasObstacle;
                if (!isWalkable) unwalkable++;

                data.cells.Add(new GridBakeData.CellData
                {
                    x = x,
                    z = z,
                    isWalkable = isWalkable,
                    worldY = worldY,
                });
            }
        }

        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();

        // Force Scene view to repaint so gizmos update immediately
        SceneView.RepaintAll();

        SetBakeStatus($"Baked {gridWidth}×{gridHeight}  ({unwalkable} blocked cells)", false);
        Repaint();
    }

    private void SetBakeStatus(string msg, bool isError)
    {
        bakeStatus = msg;
        bakeIsError = isError;
        bakeStatusAt = EditorApplication.timeSinceStartup;
    }

    // ── Validation ────────────────────────────────────────────────────────────
    private bool ValidateSetup()
    {
        if (levelGrid == null)
        {
            EditorGUILayout.HelpBox(
                "Select a GameObject with a LevelGrid component above.",
                MessageType.Info);
            return false;
        }

        if (blockerData == null)
        {
            EditorGUILayout.HelpBox(
                "No GridEdgeBlockerData found on the LevelGrid GameObject.",
                MessageType.Warning);
            if (GUILayout.Button("Add GridEdgeBlockerData"))
            {
                Undo.AddComponent<GridEdgeBlockerData>(levelGrid.gameObject);
                RefreshFromLevelGrid();
            }
            return false;
        }

        if (pathFinding == null)
        {
            EditorGUILayout.HelpBox(
                "No PathFinding component found in the scene. Baking requires it.",
                MessageType.Warning);
            return false;
        }

        return true;
    }

    // ── Canvas drawing ────────────────────────────────────────────────────────
    private void DrawCanvas(Rect canvasRect)
    {
        EditorGUI.DrawRect(canvasRect, new Color(0.13f, 0.13f, 0.16f, 1f));

        Vector2 mouse = Event.current.mousePosition;
        bool hasBake = blockerData?.bakeData != null &&
                          blockerData.bakeData.cells?.Count > 0 &&
                          blockerData.bakeData.bakedWidth == gridWidth &&
                          blockerData.bakeData.bakedHeight == gridHeight;

        var hoveredEdge = FindNearestEdge(mouse, canvasRect);

        // ── Cells ─────────────────────────────────────────────────────────────
        var coordStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 8,
            normal = { textColor = COL_COORD }
        };

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Rect cellRect = GetCellRect(canvasRect, x, z);
                bool hovered = cellRect.Contains(mouse);
                if (hovered)
                {
                    Debug.Log($"Hovering cell {x},{z}");
                }

                Color fill;
                if (hasBake)
                {
                    bool walkable = blockerData.bakeData.IsWalkable(x, z);
                    fill = walkable
                        ? (hovered ? Lighten(COL_WALKABLE, 0.15f) : COL_WALKABLE)
                        : (hovered ? Lighten(COL_UNWALKABLE, 0.15f) : COL_UNWALKABLE);
                }
                else
                {
                    fill = hovered ? COL_CELL_HOVER : COL_CELL_DEFAULT;
                }

                EditorGUI.DrawRect(cellRect, fill);

                // Y label if baked
                string label = hasBake
                    ? $"{x},{z}\ny:{blockerData.bakeData.GetWorldY(x, z):F1}"
                    : $"{x},{z}";
                GUI.Label(cellRect, label, coordStyle);
            }
        }

        // ── Grid lines ────────────────────────────────────────────────────────
        DrawGridLines(canvasRect);

        // ── Bounds highlight ──────────────────────────────────────────────────
        DrawBoundsHighlight(canvasRect);

        // ── Blocked edges ─────────────────────────────────────────────────────
        DrawBlockedEdges(canvasRect);

        // ── Hovered edge preview ──────────────────────────────────────────────
        if (hoveredEdge.a.x >= 0)
        {
            bool blocked = blockerData != null && blockerData.HasEdge(hoveredEdge.a, hoveredEdge.b);
            DrawEdgeLine(canvasRect, hoveredEdge.a, hoveredEdge.b, hoveredEdge.isHorizontal,
                blocked ? COL_BLOCKED_HOV : new Color(1f, 1f, 0.4f, 0.9f), 3f);
        }

        if (canvasRect.Contains(mouse)) Repaint();
    }

    private void DrawGridLines(Rect canvasRect)
    {
        Handles.color = COL_GRID_LINE;
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 s = new Vector3(canvasRect.x + PADDING + x * CELL_PX, canvasRect.y + PADDING);
            Vector3 e = new Vector3(s.x, canvasRect.y + PADDING + gridHeight * CELL_PX);
            Handles.DrawLine(s, e);
        }
        for (int z = 0; z <= gridHeight; z++)
        {
            Vector3 s = new Vector3(canvasRect.x + PADDING, canvasRect.y + PADDING + z * CELL_PX);
            Vector3 e = new Vector3(canvasRect.x + PADDING + gridWidth * CELL_PX, s.y);
            Handles.DrawLine(s, e);
        }
    }

    private void DrawBoundsHighlight(Rect canvasRect)
    {
        // Draw a coloured border just inside the outermost grid line
        float x0 = canvasRect.x + PADDING;
        float y0 = canvasRect.y + PADDING;
        float x1 = x0 + gridWidth * CELL_PX;
        float y1 = y0 + gridHeight * CELL_PX;

        Handles.color = COL_BOUNDS;
        float t = 2f;
        Handles.DrawAAPolyLine(t, new Vector3(x0, y0), new Vector3(x1, y0));
        Handles.DrawAAPolyLine(t, new Vector3(x1, y0), new Vector3(x1, y1));
        Handles.DrawAAPolyLine(t, new Vector3(x1, y1), new Vector3(x0, y1));
        Handles.DrawAAPolyLine(t, new Vector3(x0, y1), new Vector3(x0, y0));
    }

    private void DrawBlockedEdges(Rect canvasRect)
    {
        if (blockerData == null) return;
        
        // Loop through all saved edges in the blocker data component
        foreach (var edge in blockerData.GetBlockedEdges())
        {
            // Determine if the adjacent cells are horizontal neighbors (share the same Z value)
            // If they are horizontal neighbors, the line drawn between them on-screen will be vertical.
            bool isHoriz = (edge.az == edge.bz);
            DrawEdgeLine(canvasRect, edge.A, edge.B, isHoriz, COL_BLOCKED, 3f);
        }
    }

    private void DrawEdgeLine(Rect canvasRect, GridPosition a, GridPosition b,
                              bool isHorizontalEdge, Color color, float thickness)
    {
        // Get the visual screen rects for the two adjacent cells
        Rect rectA = GetCellRect(canvasRect, a.x, a.z);
        Rect rectB = GetCellRect(canvasRect, b.x, b.z);

        Vector3 p1, p2;
        
        // isHorizontalEdge means the cells are neighbors along the X axis
        if (isHorizontalEdge)
        {
            // Calculate the exact vertical line separating them on screen
            float borderX = (rectA.xMax + rectB.xMin) * 0.5f;
            
            // Draw from top to bottom, slightly shrunk by 4px so corners don't overlap awkwardly
            p1 = new Vector3(borderX, Mathf.Min(rectA.yMin, rectB.yMin) + 4f);
            p2 = new Vector3(borderX, Mathf.Max(rectA.yMax, rectB.yMax) - 4f);
        }
        else // Cells are neighbors along the Z axis
        {
            // Calculate the exact horizontal line separating them on screen
            float borderZ = (rectA.yMax + rectB.yMin) * 0.5f;
            
            // Draw from left to right, slightly shrunk by 4px
            p1 = new Vector3(Mathf.Min(rectA.xMin, rectB.xMin) + 4f, borderZ);
            p2 = new Vector3(Mathf.Max(rectA.xMax, rectB.xMax) - 4f, borderZ);
        }

        // Draw the line using Handles for a clean anti-aliased look
        Handles.color = color;
        Handles.DrawAAPolyLine(thickness, p1, p2);
    }

    //TODO:remove this later
    private Rect GetCellRect(Rect canvasRect, int x, int z)
    {
        float px = canvasRect.x + PADDING + x * CELL_PX;
        float py = canvasRect.y + PADDING + (gridHeight - 1 - z) * CELL_PX;

        return new Rect(px, py, CELL_PX, CELL_PX);
    }
    // ── Input ─────────────────────────────────────────────────────────────────

    private void HandleInput(Rect canvasRect)
    {
        Event e = Event.current;
        // Only process left mouse clicks (button 0)
        if (e.type != EventType.MouseDown || e.button != 0) return;

        Vector2 localMouse = e.mousePosition;

        // Find the closest edge to the mouse click
        var (a, b, _) = FindNearestEdge(localMouse, canvasRect);

        // If x < 0, the click was outside the valid grid bounds, so ignore
        if (a.x < 0) return;

        // Record undo state so the user can CTRL+Z their edge painting
        Undo.RecordObject(blockerData, "Toggle Grid Edge");

        // Toggle logic: if edge is currently blocked, remove it, otherwise add it
        if (blockerData.HasEdge(a, b))
            blockerData.RemoveEdge(a, b);
        else
            blockerData.AddEdge(a, b);

        // Mark the blocker data as dirty so Unity saves the changes to the asset/scene
        EditorUtility.SetDirty(blockerData);
        
        // Repaint the scene view immediately so the red wall gizmos update in real time
        SceneView.RepaintAll(); 
        
        // Consume the event so it doesn't propagate further (e.g., to prevent background deselects)
        e.Use();
        Repaint();
    }

    // ── Edge detection ────────────────────────────────────────────────────────
    private (GridPosition a, GridPosition b, bool isHorizontal) FindNearestEdge(
    Vector2 mouse, Rect canvasRect)
    {
        // Convert screen mouse position into coordinates relative to the start of the grid canvas
        float localX = mouse.x - (canvasRect.x + PADDING);
        float localY = mouse.y - (canvasRect.y + PADDING);

        // If the mouse is outside the left or top bounds of the grid, return invalid
        if (localX < 0 || localY < 0)
            return (new GridPosition(-1, -1), new GridPosition(-1, -1), false);

        // Calculate which cell X coordinate we are hovering over
        int cellX = Mathf.FloorToInt(localX / CELL_PX);

        // The grid is drawn upside down in the editor window (y=0 is at the top of the screen),
        // but our grid data uses z=0 at the bottom. 
        // We calculate the visual row from the top, and invert it to get the actual grid Z coordinate.
        int visualRow = Mathf.FloorToInt(localY / CELL_PX);
        int cellZ = gridHeight - 1 - visualRow;

        // Check if the calculated cell is outside the grid bounds
        if (cellX < 0 || cellX >= gridWidth ||
            cellZ < 0 || cellZ >= gridHeight)
        {
            return (new GridPosition(-1, -1), new GridPosition(-1, -1), false);
        }

        // Find exactly where the mouse is INSIDE the current cell (0 to CELL_PX)
        float cellLocalX = localX % CELL_PX;
        float cellLocalY = localY % CELL_PX;

        // Calculate the distance from the mouse to all 4 edges of the cell
        float distLeft = cellLocalX;
        float distRight = CELL_PX - cellLocalX;
        float distTop = cellLocalY; // Top visual edge on screen
        float distBottom = CELL_PX - cellLocalY; // Bottom visual edge on screen

        // Find the smallest distance to determine which edge we are closest to
        float minDist = Mathf.Min(distLeft, distRight, distTop, distBottom);

        // LEFT EDGE:
        // If closest to left, the edge is shared with the cell to the left (cellX - 1)
        if (minDist == distLeft && cellX > 0)
        {
            return (
                new GridPosition(cellX - 1, cellZ), // Cell A
                new GridPosition(cellX, cellZ),     // Cell B
                true // isHorizontal = true means cells share a Z coordinate (horizontal neighbors)
            );
        }

        // RIGHT EDGE:
        // If closest to right, the edge is shared with the cell to the right (cellX + 1)
        if (minDist == distRight && cellX < gridWidth - 1)
        {
            return (
                new GridPosition(cellX, cellZ),
                new GridPosition(cellX + 1, cellZ),
                true
            );
        }

        // TOP EDGE:
        // If closest to the visual top, we are bordering the cell with the HIGHER Z value
        // (because Z increases as we go up visually in the world, which is 'up' on screen)
        if (minDist == distTop && cellZ < gridHeight - 1)
        {
            return (
                new GridPosition(cellX, cellZ),
                new GridPosition(cellX, cellZ + 1),
                false // isHorizontal = false means cells share an X coordinate (vertical neighbors)
            );
        }

        // BOTTOM EDGE:
        // If closest to the visual bottom, we are bordering the cell with the LOWER Z value
        if (minDist == distBottom && cellZ > 0)
        {
            return (
                new GridPosition(cellX, cellZ - 1),
                new GridPosition(cellX, cellZ),
                false
            );
        }

        // Fallback case if no valid edge was found (e.g. clicking an outer boundary)
        return (
            new GridPosition(-1, -1),
            new GridPosition(-1, -1),
            false
        );
    }

    private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a, ap = p - a;
        float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / ab.sqrMagnitude);
        return Vector2.Distance(p, a + t * ab);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    //private Rect GetCellRect(Rect canvasRect, int x, int z)
    //{
    //    float px = canvasRect.x + PADDING + x * CELL_PX;
    //    float py = canvasRect.y + PADDING + (gridHeight - 1 - z) * CELL_PX;
    //    return new Rect(px, py, CELL_PX, CELL_PX);
    //}

    private void RefreshFromLevelGrid()
    {
        if (levelGrid == null) { blockerData = null; pathFinding = null; return; }

        blockerData = levelGrid.GetComponent<GridEdgeBlockerData>();

        // PathFinding only exists at runtime — null is fine, bake button will warn
        pathFinding = FindObjectOfType<PathFinding>();

        // LevelGrid.gridSystem is null in edit mode — read the serialized backing
        // fields directly via SerializedObject so we never call into gridSystem
        var so = new SerializedObject(levelGrid);
        gridWidth = so.FindProperty("width").intValue;
        gridHeight = so.FindProperty("height").intValue;
        cellSize = so.FindProperty("cellSize").floatValue;

        Repaint();
    }


    private void DrawLegend()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        DrawSwatch(COL_WALKABLE); GUILayout.Label("Walkable", EditorStyles.miniLabel);
        GUILayout.Space(6);
        DrawSwatch(COL_UNWALKABLE); GUILayout.Label("Blocked", EditorStyles.miniLabel);
        GUILayout.Space(6);
        DrawSwatch(COL_BLOCKED); GUILayout.Label("Edge fence", EditorStyles.miniLabel);
        GUILayout.Space(6);
        DrawSwatch(COL_BOUNDS); GUILayout.Label("Bounds", EditorStyles.miniLabel);
        GUILayout.FlexibleSpace();
        GUILayout.Label("Click a cell border to toggle fence  |  Undo supported", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
    }

    private static void DrawSwatch(Color c)
    {
        Rect r = GUILayoutUtility.GetRect(12, 12, GUILayout.Width(12), GUILayout.Height(12));
        EditorGUI.DrawRect(r, c);
        GUILayout.Space(2);
    }

    private static Color Lighten(Color c, float amount) =>
        new Color(Mathf.Clamp01(c.r + amount), Mathf.Clamp01(c.g + amount),
                  Mathf.Clamp01(c.b + amount), c.a);
}
#endif