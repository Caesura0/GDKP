using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode 
{
    GridPosition gridPosition;
    int gCost;
    int hCost;
    int fCost;

    bool isWalkable = true;

    PathNode cameFromPathNode;

    public PathNode(GridPosition gridPosition)
    {
        this.gridPosition = gridPosition;
    }



    public override string ToString()
    {
        return gridPosition.ToString();
    }

    public int GetGCost()
    {
        return gCost;
    }
    public int GetHCost()
    {
        return hCost;
    }
    public int GetFCost()
    {
        return fCost;
    }

    public void SetGCost(int cost)
    {
        gCost = cost;
    }
    public void SetHCost(int cost)
    {
        hCost = cost;
    }
    public void CalculateFCost()
    {
        fCost = hCost + gCost;
    }

    public void ResetCameFromPathNode()
    {
        cameFromPathNode = null;
    }

    public void SetCameFromPathNode(PathNode node)
    {
        cameFromPathNode = node;
    }
    public PathNode GetCameFromPathNode()
    {
        return cameFromPathNode;
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public bool GetIsWalkable()
    {
        return isWalkable;
    }

    public void SetIsWalkable(bool walkable)
    {
        isWalkable = walkable;
    }
}
