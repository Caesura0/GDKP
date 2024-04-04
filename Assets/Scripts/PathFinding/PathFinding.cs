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
