using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public static PathFinding Instance { private set; get; }


    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    [SerializeField] Transform gridDebugObjectPrefab;
    [SerializeField] LayerMask obstaclesLayermask;
    [SerializeField] bool pathfindingDebug = false;

    public void SetIsWalkableGridPosition(GridPosition gridPosition, bool v)
    {
        gridSystem.GetGridObject(gridPosition).SetIsWalkable(v);
    }

    public LayerMask GetObstaclesLayerMask() => obstaclesLayermask;

    int width;
    int height;
    float cellSize;
    GridSystem<PathNode> gridSystem;

    
    private void Awake()
    {

            if (Instance != null)
            {
                Debug.Log("There's more then one pathfinding" + transform + " - " + Instance);
                Destroy(gameObject);
                return;
            }
            Instance = this;


    }

    public void Setup(int width, int height, float cellSize)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        gridSystem = new GridSystem<PathNode>
            (width, height, cellSize, (GridSystem<PathNode> gameObject, GridPosition gridPosition) =>
            new PathNode(gridPosition));

        if (pathfindingDebug) { gridSystem.CreateDebugObjects(gridDebugObjectPrefab); }
        

        for (int x = 0; x < gridSystem.GetWidth(); x++)
        {
            for (int z = 0; z < gridSystem.GetHeight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x,z);
                Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
                float raycastOffsetDistance = 4f;
                if(Physics.Raycast(
                    worldPosition + Vector3.down * raycastOffsetDistance, 
                    Vector3.up, 
                    raycastOffsetDistance * 2,
                    obstaclesLayermask))
                {
                    GetNode(x, z).SetIsWalkable(false);
                }
            }
        }
    }
    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength)
    {
        List<PathNode> openList = new List<PathNode>();
        List<PathNode> closedList = new List<PathNode>();


        PathNode startNode = gridSystem.GetGridObject(startGridPosition);
        PathNode endNode = gridSystem.GetGridObject(endGridPosition);
        openList.Add(startNode);


        for(int x = 0; x < gridSystem.GetWidth(); x++)
        {
            for(int z = 0; z < gridSystem.GetHeight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                PathNode pathNode = gridSystem.GetGridObject(gridPosition);

                pathNode.SetGCost(int.MaxValue);
                pathNode.SetHCost(0);
                pathNode.CalculateFCost();
                pathNode.ResetCameFromPathNode();
            }
        }


        startNode.SetGCost(0);
        startNode.SetHCost(CalculateDistance(startGridPosition, endGridPosition));
        startNode.CalculateFCost();


        while(openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostPathNode(openList); 
            if(currentNode == endNode)
            {
                //reached final node;
                pathLength = endNode.GetFCost();
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode))
                {
                    continue;
                }

                if (!neighbourNode.GetIsWalkable())
                {
                    closedList.Add(neighbourNode);
                    continue;
                }
                // ── Edge blocker check ──────────────────────────────────────
                // If this specific edge between current and neighbour is blocked
                // (e.g. a fence between them), skip it as if it were a wall.
                if (currentNode.IsNeighbourBlocked(neighbourNode.GetGridPosition()))
                {
                    continue;
                }

                int tentativeGcost = currentNode.GetGCost() + CalculateDistance(currentNode.GetGridPosition(), neighbourNode.GetGridPosition());
                if(tentativeGcost < neighbourNode.GetGCost())
                {
                    neighbourNode.SetCameFromPathNode(currentNode);
                    neighbourNode.SetGCost(tentativeGcost);
                    neighbourNode.SetHCost(CalculateDistance(neighbourNode.GetGridPosition(), endGridPosition));
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
     
        }

        //no path found
        pathLength = 0;
        return null;

    }

    //
    private List<GridPosition> CalculatePath(PathNode endNode)
    {
        PathNode currentNode = endNode;

        List<GridPosition> gridPositions = new List<GridPosition>();
        gridPositions.Add(currentNode.GetGridPosition());

        while (currentNode.GetCameFromPathNode() != null)
        {
            gridPositions.Add(currentNode.GetCameFromPathNode().GetGridPosition());
            currentNode = currentNode.GetCameFromPathNode();
        }

        gridPositions.Reverse();
        return gridPositions;
    }

    //private List<GridPosition> CalculatePath(PathNode endNode)
    //{
    //    List<PathNode> pathNodeList = new List<PathNode>();
    //    pathNodeList.Add(endNode);
    //    PathNode currentNode = endNode;
    //    while(currentNode.GetCameFromPathNode() != null)
    //    {
    //        pathNodeList.Add(currentNode.GetCameFromPathNode());
    //        currentNode = currentNode.GetCameFromPathNode();
    //    }
    //    pathNodeList.Reverse();

    //    List<GridPosition> gridPositionList = new List<GridPosition>();
    //    foreach(PathNode pathNode in pathNodeList)
    //    {
    //        gridPositionList.Add(pathNode.GetGridPosition());
    //    }

    //    return gridPositionList;

    //}


    /// <summary>
    /// Registers a blocked cardinal edge between two adjacent cells (bidirectionally),
    /// then checks all four diagonal corners that share this edge and caches any
    /// that are now fully pinched by two blocked cardinal edges.
    /// </summary>
    public void BlockEdge(GridPosition a, GridPosition b)
    {
        PathNode nodeA = GetNode(a.x, a.z);
        PathNode nodeB = GetNode(b.x, b.z);
        if (nodeA == null || nodeB == null) return;

        // Register the cardinal edge on both sides
        nodeA.BlockNeighbour(b);
        nodeB.BlockNeighbour(a);

        // Update diagonal caches for the two cells that are ENDPOINTS of this edge.
        // Each endpoint can see two corners involving this edge.
        UpdateDiagonalCaches(nodeA, nodeB, isBlocking: true);
        UpdateDiagonalCaches(nodeB, nodeA, isBlocking: true);

        // Update diagonal caches for the two cells that are PERPENDICULAR to this edge
        // (the cells that would cut through the corner from the other side).
        // These are NOT endpoints of the edge, so the above calls miss them entirely.
        UpdateOppositeCornerCaches(a, b, isBlocking: true);
    }

    /// <summary>
    /// Removes a blocked cardinal edge between two adjacent cells (bidirectionally),
    /// then evicts any cached diagonal blocks that were relying on this edge.
    /// Safe to call for edges that were never blocked.
    /// </summary>
    public void UnblockEdge(GridPosition a, GridPosition b)
    {
        PathNode nodeA = GetNode(a.x, a.z);
        PathNode nodeB = GetNode(b.x, b.z);
        if (nodeA == null || nodeB == null) return;

        // Remove the cardinal edge on both sides
        nodeA.UnblockNeighbour(b);
        nodeB.UnblockNeighbour(a);

        // Evict diagonal caches for the endpoint nodes
        UpdateDiagonalCaches(nodeA, nodeB, isBlocking: false);
        UpdateDiagonalCaches(nodeB, nodeA, isBlocking: false);

        // Evict diagonal caches for the perpendicular (opposite corner) nodes
        UpdateOppositeCornerCaches(a, b, isBlocking: false);
    }

    /// <summary>
    /// Given a node and one of its cardinal neighbours (the edge that just changed),
    /// evaluates all four diagonal corners that share that cardinal direction and
    /// either caches or evicts the corresponding diagonal block on 'node'.
    ///
    /// Four corners are checked from 'node's perspective:
    ///   The changed cardinal (node → cardinal) pairs with each perpendicular cardinal
    ///   (node → perpA) and (node → perpB) to form two diagonals.
    ///   The same logic is mirrored for the opposite cardinal direction.
    /// </summary>
    private void UpdateDiagonalCaches(PathNode node, PathNode changedCardinal, bool isBlocking)
    {
        GridPosition p    = node.GetGridPosition();
        GridPosition card = changedCardinal.GetGridPosition();

        // Work out which axis this cardinal edge is on
        int dx = card.x - p.x;  // will be -1, 0, or +1
        int dz = card.z - p.z;  // will be -1, 0, or +1

        if (dx != 0 && dz == 0)
        {
            // Horizontal cardinal (left or right). The two diagonals it participates in
            // are the ones that also step ±1 in Z. The other bracket edge for each is
            // the straight up/down cardinal from 'node'.
            TryUpdateDiagonal(node, card, new GridPosition(p.x,      p.z + 1), new GridPosition(card.x, p.z + 1), isBlocking);
            TryUpdateDiagonal(node, card, new GridPosition(p.x,      p.z - 1), new GridPosition(card.x, p.z - 1), isBlocking);
        }
        else if (dz != 0 && dx == 0)
        {
            // Vertical cardinal (up or down). The two diagonals it participates in
            // are the ones that also step ±1 in X. The other bracket edge for each is
            // the straight left/right cardinal from 'node'.
            TryUpdateDiagonal(node, card, new GridPosition(p.x + 1, p.z),      new GridPosition(p.x + 1, card.z), isBlocking);
            TryUpdateDiagonal(node, card, new GridPosition(p.x - 1, p.z),      new GridPosition(p.x - 1, card.z), isBlocking);
        }
    }

    /// <summary>
    /// Evaluates a single diagonal corner from 'node':
    ///   cardinal1Pos — one bracketing cardinal neighbour (the edge that just changed)
    ///   cardinal2Pos — the other bracketing cardinal neighbour (perpendicular)
    ///   diagonalPos  — the diagonal cell formed by both
    ///
    /// If isBlocking: caches the diagonal if EITHER cardinal is blocked.
    ///   Since cardinal1 was just blocked by the caller, this always caches the diagonal.
    /// If !isBlocking: only evicts the diagonal if cardinal2 is also clear.
    ///   If cardinal2 is still blocked, the corner is still walled and diagonal stays blocked.
    /// </summary>
    private void TryUpdateDiagonal(PathNode node,
                                   GridPosition cardinal1Pos,
                                   GridPosition cardinal2Pos,
                                   GridPosition diagonalPos,
                                   bool isBlocking)
    {
        // Make sure both cardinal neighbours and the diagonal cell actually exist on the grid
        if (GetNode(cardinal2Pos.x, cardinal2Pos.z) == null) return;
        if (GetNode(diagonalPos.x,  diagonalPos.z)  == null) return;

        if (isBlocking)
        {
            // Cardinal1 was just blocked. Block the diagonal if EITHER bracketing
            // cardinal is walled — touching any wall at a corner prevents cutting through.
            // Since cardinal1 is now blocked, this is always true, so always cache.
            node.BlockNeighbour(diagonalPos);
        }
        else
        {
            // Cardinal1 was just unblocked. Only evict the diagonal if cardinal2 is
            // also clear — if cardinal2 is still blocked, the corner is still walled
            // and diagonal movement through it must remain blocked.
            if (!node.IsNeighbourBlocked(cardinal2Pos))
                node.UnblockNeighbour(diagonalPos);
        }

    }

    /// <summary>
    /// For each corner that edge A-B participates in, updates the diagonal cache of the
    /// two cells that sit OPPOSITE the edge — i.e. the cells that would cut through
    /// that corner diagonally without being an endpoint of the edge itself.
    ///
    /// Every cardinal edge borders two corners (one on each lateral side). In each
    /// corner, four cells meet. UpdateDiagonalCaches covers the two endpoint cells;
    /// this method covers the other two.
    /// </summary>
    private void UpdateOppositeCornerCaches(GridPosition a, GridPosition b, bool isBlocking)
    {
        int dx = b.x - a.x;
        int dz = b.z - a.z;

        if (dx != 0 && dz == 0)
        {
            // Horizontal edge (A and B share the same Z, differ in X).
            // Perpendicular cells are the ones directly above and below each endpoint.
            int az = a.z;

            // Bottom corner (z - 1):
            //   node(a.x, az-1) diagonals to B, bracketed by A and (b.x, az-1)
            //   node(b.x, az-1) diagonals to A, bracketed by B and (a.x, az-1)
            UpdatePerpendicularCornerDiagonal(new GridPosition(a.x, az - 1), b, a, new GridPosition(b.x, az - 1), isBlocking);
            UpdatePerpendicularCornerDiagonal(new GridPosition(b.x, az - 1), a, b, new GridPosition(a.x, az - 1), isBlocking);

            // Top corner (z + 1):
            UpdatePerpendicularCornerDiagonal(new GridPosition(a.x, az + 1), b, a, new GridPosition(b.x, az + 1), isBlocking);
            UpdatePerpendicularCornerDiagonal(new GridPosition(b.x, az + 1), a, b, new GridPosition(a.x, az + 1), isBlocking);
        }
        else if (dz != 0 && dx == 0)
        {
            // Vertical edge (A and B share the same X, differ in Z).
            // Perpendicular cells are the ones directly left and right of each endpoint.
            int ax = a.x;

            // Left corner (x - 1):
            //   node(ax-1, a.z) diagonals to B, bracketed by A and (ax-1, b.z)
            //   node(ax-1, b.z) diagonals to A, bracketed by B and (ax-1, a.z)
            UpdatePerpendicularCornerDiagonal(new GridPosition(ax - 1, a.z), b, a, new GridPosition(ax - 1, b.z), isBlocking);
            UpdatePerpendicularCornerDiagonal(new GridPosition(ax - 1, b.z), a, b, new GridPosition(ax - 1, a.z), isBlocking);

            // Right corner (x + 1):
            UpdatePerpendicularCornerDiagonal(new GridPosition(ax + 1, a.z), b, a, new GridPosition(ax + 1, b.z), isBlocking);
            UpdatePerpendicularCornerDiagonal(new GridPosition(ax + 1, b.z), a, b, new GridPosition(ax + 1, a.z), isBlocking);
        }
    }

    /// <summary>
    /// Updates the diagonal cache entry for perpPos → diagTarget.
    ///   bracket1 — the corner cell that is an endpoint of the changed edge
    ///   bracket2 — the other lateral cell framing the corner from perpPos's side
    ///
    /// If blocking:   always caches the diagonal (any wall = corner is walled).
    /// If unblocking: only evicts if BOTH brackets are also clear from perpPos's
    ///                perspective, because either bracket still being blocked means
    ///                the corner is still walled from perpPos's side.
    /// </summary>
    private void UpdatePerpendicularCornerDiagonal(
        GridPosition perpPos, GridPosition diagTarget,
        GridPosition bracket1,  GridPosition bracket2,
        bool isBlocking)
    {
        PathNode perpNode = GetNode(perpPos.x, perpPos.z);
        if (perpNode == null) return;
        if (GetNode(diagTarget.x, diagTarget.z) == null) return;

        if (isBlocking)
        {
            // Any wall touching a corner blocks the diagonal through it.
            perpNode.BlockNeighbour(diagTarget);
        }
        else
        {
            // Only evict the diagonal if neither bracketing cardinal is still blocked
            // from perpPos's perspective. If either bracket is still walled, the
            // corner is still closed and perpPos must not be allowed to cut through.
            if (!perpNode.IsNeighbourBlocked(bracket1) && !perpNode.IsNeighbourBlocked(bracket2))
                perpNode.UnblockNeighbour(diagTarget);
        }
    }


    public int CalculateDistance(GridPosition gridPositionA, GridPosition gridPositionB)
    {
        GridPosition gridPositionDistance = gridPositionA - gridPositionB;
        int xDistance = Mathf.Abs(gridPositionDistance.x);
        int zDistance = Mathf.Abs(gridPositionDistance.z);
        int remaining = Mathf.Abs(xDistance - zDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + MOVE_STRAIGHT_COST * remaining;
    }


    private PathNode GetLowestFCostPathNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostPathNode = pathNodeList[0];

        for (int i = 0; i < pathNodeList.Count; i++)
        {
            if(pathNodeList[i].GetFCost() < lowestFCostPathNode.GetFCost())
            {
                lowestFCostPathNode = pathNodeList[i];
            }
        }
        return lowestFCostPathNode;
    }


    public void CalculateGridDirection(GridPosition gridPositionTo, GridPosition gridPositionFrom)
    {

    }


    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        GridPosition gridPosition = currentNode.GetGridPosition();


        if (gridPosition.x - 1 >= 0)
        {
            //left
            neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 0));
            if (gridPosition.z - 1 >= 0)
            {
                //leftDown
                neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + -1));
            }
            if (gridPosition.z + 1 < gridSystem.GetHeight())
            {
                //leftUp
                neighbourList.Add(GetNode(gridPosition.x - 1, gridPosition.z + +1));
            }
        }
        if (gridPosition.x + 1 < gridSystem.GetWidth())
        {
            //right
            neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 0));

            if (gridPosition.z - 1 >= 0)
            {
                //rightDown
                neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + -1));
            }
            if (gridPosition.z + 1 < gridSystem.GetHeight())
            {
                //rightup
                neighbourList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 1));
            }

            if (gridPosition.z - 1 >= 0)
            {
                //down
                neighbourList.Add(GetNode(gridPosition.x + 0, gridPosition.z - 1));
            }

            if (gridPosition.z + 1 < gridSystem.GetHeight())
            {
                //up
                neighbourList.Add(GetNode(gridPosition.x + 0, gridPosition.z + 1));
            }
        }
            return neighbourList;
        }


    private PathNode GetNode(int x, int z)
    {
        return gridSystem.GetGridObject(new GridPosition(x, z));
    }

    public bool IsWalkableGridPosition(GridPosition gridPosition)
    {
        return gridSystem.GetGridObject(gridPosition).GetIsWalkable();
    }

    public bool HasPath(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        return FindPath(startGridPosition, endGridPosition, out int pathLength) != null;
    }

    public int GetPathLength(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        FindPath(startGridPosition, endGridPosition, out int pathLength);
        return pathLength;
    }
}
