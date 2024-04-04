using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassAction : BaseAction
{
    bool passActionTaken = false;
    float stateTimer;
    [SerializeField] float passDelay = 1.2f;

    private void Update()
    {
        if (passActionTaken)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0)
            {
                ActionComplete();
                passActionTaken = false;
            }


        }
    }


    public override string GetActionName()
    {
        return "Pass Turn";
    }

    public override int GetActionPointsCost()
    {
        if (unit.GetActionPoints() > 0)
        {
            return unit.GetActionPoints();
        }
        else
        {
             return 1;
        }
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPostion)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPostion,
            actionValue = baseAiActionWeight,
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List < GridPosition > gridPositions= new List < GridPosition >();
        gridPositions.Add( unit.GetGridPosition() );
        return gridPositions;
    }

    public override bool IsValidActionThisTurn()
    {
        return true;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        //Debug.Log("PassAction");
        unit.ClearAllActionPoints();
        ActionStart(onActionComplete);
        stateTimer = passDelay;
        passActionTaken = true;
        
    }


}
