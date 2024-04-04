using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverwatchAction : BaseAttackAction
{

    [SerializeField] int coveringRange;
    [SerializeField] int coveringAreaSize;

    bool isCovering;
    public List<GridPosition> overwatchGridPositionList;
    float afterHitStateTime = 1f;

    GridPosition targetGridPosition;

    private void Start()
    {
        LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMovedGridPosition;
    }

    void Update()
    {
        if (!isActive) { return; }

        stateTimer -= Time.deltaTime;
        switch (state)
        {
            case State.Aiming:
                float rotateSpeed = 6.5f;
                Vector3 aimDirection = (LevelGrid.Instance.GetWorldPosition(targetGridPosition) - transform.position).normalized;
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * rotateSpeed);
                break;
            case State.Cooloff:

                break;
        }

        if (stateTimer <= 0f)
        {
            NextState();
        }
    }

    protected override void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.Cooloff;
                stateTimer = afterHitStateTime;
                break;
            case State.Cooloff:
                ActionComplete();
                break;

        }
    }

    public int GetCoverAreaSize()
    {
        return coveringAreaSize;
    }

    private void LevelGrid_OnAnyUnitMovedGridPosition(object sender, LevelGrid.OnAnyUnitMovedGridPositionEventArgs e)
    {
        if (!isCovering) { return; }

        Unit targetUnit = e.unit as Unit;
        Vector3 unitWorldPosition = unit.GetWorldPosition();
        Debug.Log(targetUnit.name + " sender " + unit.GetWorldPosition());
        if (targetUnit != null)
        {
            if (overwatchGridPositionList.Contains(targetUnit.GetGridPosition()))
            {
                Debug.Log("overwatch location detected");
                Vector3 shootDir = (targetUnit.GetWorldPosition() - unitWorldPosition).normalized;
                float unitShoulderHeight = 1.7f;
                if (!Physics.Raycast(
                    unitWorldPosition + Vector3.up *
                    unitShoulderHeight,
                    shootDir,
                    Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()),
                    obstaclesLayerMask))
                {
                    Debug.Log("shoot");
                    targetUnit.Damage(damage);
                    //freeze time
                    //check if you hit the enemy
                    //cancel remaining unit movement
                    isCovering = false;
                    overwatchGridPositionList.Clear();
                }
            }
        }
    }

    public override string GetActionName()
    {
        return "Cover Area";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPostion)
    {

        return new EnemyAIAction
        {
            gridPosition = gridPostion,
            actionValue = 10,
        };


        //Should this just be the final option instead of pass?
    }

    public override int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return 0;
    }


    public override List<GridPosition> GetValidActionGridPositionList(GridPosition gridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();
        for (int x = -coveringRange; x <= coveringRange; x++)
        {
            for (int z = -coveringRange; z <= coveringRange; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) { continue; }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);

                if (testDistance > coveringRange) { continue; }


                validGridPositionList.Add(testGridPosition);
            }

        }

        return validGridPositionList;
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {

        return GetValidActionGridPositionList(unit.GetGridPosition()) ;
    }



    public override bool IsValidActionThisTurn()
    {
        throw new NotImplementedException();
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        overwatchGridPositionList = GetCoveringGridPositions(gridPosition);
        ActionStart(onActionComplete);
        isCovering = true;
        stateTimer = 1f;
    }

    public override void TakeReloadAction(Action onActionComplete)
    {
        Debug.Log("Not Needed");
    }




    public bool GetIsCovering()
    {
        return isCovering;
    }


    public List<GridPosition> GetCoveringGridPositions(GridPosition gridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        //GridPosition unitGridPosition = unit.GetGridPosition();
        for (int x = -coveringAreaSize; x <= coveringAreaSize; x++)
        {
            for (int z = -coveringAreaSize; z <= coveringAreaSize; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = gridPosition + offsetGridPosition;
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) { continue; }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);

                if (testDistance > coveringAreaSize) { continue; }


                validGridPositionList.Add(testGridPosition);
                Debug.Log(testGridPosition + " Added to list");
            }

        }

        return validGridPositionList;
    }

}
