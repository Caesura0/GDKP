using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    
    /// <summary>
    /// High level class that stores the width and height of the grid system <b/>
    /// Builds, grid at start and acts as 'pass through' to get information on level grid and grid positions
    /// </summary>
    public static LevelGrid Instance { private set; get; }

    public event EventHandler OnAnyActionMoveGridPosition;

    public event EventHandler<OnAnyUnitMovedGridPositionEventArgs> OnAnyUnitMovedGridPosition;


    public class OnAnyUnitMovedGridPositionEventArgs : EventArgs
    {
        public Unit unit;
        public GridPosition fromGridPosition;
        public GridPosition toGridPosition;
    }

    [SerializeField] Transform gridDebugObjectPrefab;

    GridSystem<GridObject> gridSystem;

    [SerializeField] int width = 10;
    [SerializeField] int height = 10;
    [SerializeField] float cellSize= 2f;
    [SerializeField] bool debugGrid = false;

    [SerializeField] LayerMask walkableLayers;
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There's more then one LevelGrid " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;


        gridSystem = new GridSystem<GridObject>(height, width, cellSize, (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(g,gridPosition));
        if(debugGrid) gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
    }



    private void Start()
    {
        PathFinding.Instance.Setup(height, width, cellSize);
    }

    public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.AddUnit(unit);

    }

    public Vector3 GetLevelGridTransform()
    {
        return transform.position;
    }



    public LayerMask GetWalkableLayers()
    {
        return walkableLayers;
    }

    public Unit GetUnitAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetUnit();
        
    }

    public void ClearInteractableAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.ClearInteractable();
    }

    public void ClearUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.RemoveUnit(unit);
    }
    /// <summary>
    /// Call when unit has moved to a new grid position
    /// Clears unit at grid position, add unit to new grid position
    /// Calls OnAnyActionMoveGridPosition and OnAnyUnitMovedGridPosition
    /// OnAnyUnitMovedGridPosition passes all three parameters in the eventArgs
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="fromGridPosition"></param>
    /// <param name="toGridPosition"></param>
    /// 

    public void UnitMovedGridPosition(Unit unit,GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        ClearUnitAtGridPosition(fromGridPosition, unit);
        AddUnitAtGridPosition(toGridPosition, unit);
        OnAnyActionMoveGridPosition?.Invoke(this, EventArgs.Empty);
        OnAnyUnitMovedGridPosition?.Invoke(this, new OnAnyUnitMovedGridPositionEventArgs
        {
            unit = unit,
            fromGridPosition = fromGridPosition,
            toGridPosition = toGridPosition,
        });
    }


    //shorthand for returning data for a passthrough function 
    //same as using { return ........} cleaner look
    public GridPosition GetGridPosition(Vector3 worldPosition) => gridSystem.GetGridPosition(worldPosition);
    public Vector3 GetWorldPosition(GridPosition gridPosition) => gridSystem.GetWorldPosition(gridPosition);


    /// <summary>
    /// Checks to see if the position is part of the grid, if the x, z are more 0 and less width, height
    /// </summary>
    /// <param name="gridPosition"></param>
    /// <returns></returns>
    public bool IsValidGridPosition(GridPosition gridPosition) => gridSystem.IsValidGridPosition(gridPosition);

    public int GetWidth() => gridSystem.GetWidth();
    public int GetHeight() => gridSystem.GetHeight();


    public bool HasAnyUnitOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.HasAnyUnit();
    }

    public bool HasAnyIDestructableOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetDestructable() != null;
    }

    public IDestructable GetAnyIDestructableOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetDestructable();
    }

    public List<Unit> GetAnyUnitAtGridPosition(GridPosition gridPosition)
    {
        return gridSystem.GetGridObject(gridPosition).GetUnitList();
    }


    public IInteractable GetInteractableAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetInteractable();
    }

    public void SetInteractableAtGridPosition(GridPosition gridPosition, IInteractable Interactable)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.SetInteractable(Interactable);
    }

    public void SetIDestructableAtGridPosition(GridPosition gridPosition, IDestructable destructable)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.SetDestructable(destructable);
    }
    public void SetCoverTypeAtGridPosition(GridPosition gridPosition, CoverType coverType)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.SetCoverObject(coverType);
    }

    public CoverType GetCoverTypeAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetCoverType();
    }



    /// <summary>
    /// Returns the overall best cover based on the grid position, /n Cover system does not currently care about being between you and the target, you only need to stand next to it
    /// </summary>
    /// <param name="gridPosition"></param>
    /// <returns></returns>
    public float GetCoverAccuracyReduction(GridPosition gridPosition)
    {
        CoverType coverType = GetBestCoverTypeAtGridPosition(gridPosition);
        //Debug.Log(coverType.ToString() + " returned in GetCoverAccuracyReduction");
        float coverAccuracyReduction = 0;
        switch (coverType)
        {
            case CoverType.Full:
                coverAccuracyReduction = 25;
                break;
            case CoverType.Half:
                coverAccuracyReduction = 10;
                break;
            case CoverType.None:
                coverAccuracyReduction = 0;
                break;
        }
        //Debug.Log("CoverType " + coverAccuracyReduction);
        return coverAccuracyReduction;
    }



    public CoverType GetBestCoverTypeAtGridPosition(GridPosition gridPosition)
    {
        int UnitCoverCheckRange = 1;
        CoverType coverType = CoverType.None;
        CoverType testCoverType = CoverType.None;


        for (int x = -UnitCoverCheckRange; x <= UnitCoverCheckRange; x++)
        {
            for (int z = -UnitCoverCheckRange; z <= UnitCoverCheckRange; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = gridPosition + offsetGridPosition;
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) { continue; }
                testCoverType = LevelGrid.Instance.GetCoverTypeAtGridPosition(testGridPosition);
                //Debug.Log("testCoverType " + testCoverType.ToString() + "   testGridPostion " + testGridPosition);


                if (testCoverType == CoverType.None) { continue; }



                if (testCoverType == CoverType.Full)
                {
                    return CoverType.Full;  // If full cover is found, immediately return it
                }
                else if (testCoverType == CoverType.Half)
                {
                    coverType = CoverType.Half;  // If half cover is found, set coverType to half
                }

            }

        }
        return coverType;
    }
}
