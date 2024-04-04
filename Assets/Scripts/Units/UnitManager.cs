using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{

    public static UnitManager Instance { private set; get; }
    List<Unit> unitList;
    List<Unit> enemyUnitList;
    List<Unit> friendlyUnitList;
    List<Unit> friendlyUnitsWithAPList;
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There's more then one unitactionsystem" + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        unitList = new List<Unit>();
        enemyUnitList = new List<Unit>();
        friendlyUnitList = new List<Unit>();
        friendlyUnitsWithAPList = new List<Unit>();
    }
    private void Start()
    {
        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;
        Unit.OnAnyActionPointsChanged += Unit_OnAnyActionPointsChanged;
        UpdateFriendlyUnitsWithAPList();
    }

    private void Unit_OnAnyActionPointsChanged(object sender, EventArgs e)
    {
        //TODO: look at on any unit spawned you dummy, way simpler way to do it

        UpdateFriendlyUnitsWithAPList();
    }

    private void UpdateFriendlyUnitsWithAPList()
    {
        friendlyUnitsWithAPList.Clear();
        foreach (Unit unit in friendlyUnitList)
        {
            if (unit.GetActionPoints() > 0) friendlyUnitsWithAPList.Add(unit);
        }
    }

    private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;
        unitList.Remove(unit);

        if (unit.IsEnemy())
        {
            enemyUnitList.Remove(unit);
        }
        else
        {
            friendlyUnitList.Remove(unit);
            UpdateFriendlyUnitsWithAPList();

        }
    }

    private void Unit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;

        unitList.Add(unit);

        if (unit.IsEnemy())
        {
            enemyUnitList.Add(unit);
        }
        else
        {
            friendlyUnitList.Add(unit);
            UpdateFriendlyUnitsWithAPList();
        }
    }

    public List<Unit> GetUnitList()
    {
        return unitList;
    }
    public List<Unit> GetFriendlyUnitList()
    {
        return friendlyUnitList;
    }    
    public List<Unit> GetFriendlyUnitsWithAPList()
    {
        return friendlyUnitsWithAPList;
    }
    public List<Unit> GetEnemyUnitList()
    {
        return enemyUnitList;
    }
}
