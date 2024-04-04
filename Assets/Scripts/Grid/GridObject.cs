using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Holds all the active data of the Grid<br/>
/// Units, Interactable, Destructable, GridPosition
/// </summary>
public class GridObject
{
    GridSystem<GridObject> gridSystem;
    GridPosition gridPosition;
    IInteractable interactable;
    IDestructable destructable;
    CoverType coverType = CoverType.None;
    List<Unit> unitList;

    public GridObject(GridSystem<GridObject> gridSystem, GridPosition gridPosition)
    {
        this.gridSystem = gridSystem;
        this.gridPosition = gridPosition;
        unitList = new List<Unit>();
    }

    public override string ToString()
    {
        string unitString = "";
        foreach (Unit unit in unitList)
        {
            unitString += unit + "\n";
        }
        return gridPosition.ToString() + "\n" + unitString;
    }

    public List<Unit> GetUnitList()
    {
        return unitList;
    }

    public void AddUnit(Unit unit)
    {
        unitList.Add(unit);
    }

    public void RemoveUnit(Unit unit)
    {
        unitList.Remove(unit);
    }

    public bool HasAnyUnit()
    {
        return unitList.Count > 0;
    }

    public Unit GetUnit()
    {
        if (HasAnyUnit())
        {
            return unitList[0];
        }
        else
        {
            return null;
        }
    }

    public IInteractable GetInteractable()
    {
        return interactable;
    }



    public void SetInteractable(IInteractable interactable)
    {
        this.interactable = interactable;
    }

    public void ClearInteractable()
    {
        this.interactable = null;
    }

    public IDestructable GetDestructable()
    {
        return destructable;
    }

    public CoverType GetCoverType()
    {
        return coverType;
    }

    public void SetCoverObject(CoverType coverType)
    {
        this.coverType = coverType;
    }


    public void SetDestructable(IDestructable destructable)
    {
        this.destructable = destructable;
    }

    public void ClearDestructable()
    {
        this.destructable = null;
    }




}

