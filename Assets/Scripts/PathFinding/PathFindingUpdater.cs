using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingUpdater : MonoBehaviour
{
    private void OnDestroy()
    {
        // Get the grid position for this transform
        GridPosition gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);

        // Set the grid position to be walkable
        PathFinding.Instance.SetIsWalkableGridPosition(gridPosition, true);
    }
}
