using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAction : BaseAction
{

    //look at lesson 'Single Active Action' when you need to review this
    private float totalSpinAmount;


    void Update()
    {
        if (!isActive) { return; }


        float spinAddAmount = 360f * Time.deltaTime;
        transform.eulerAngles += new Vector3(0, spinAddAmount, 0);
        totalSpinAmount += spinAddAmount;

        if (totalSpinAmount >= 360f)
        {
            ActionComplete();
        }
    }


    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {

        totalSpinAmount = 0f;
        ActionStart(onActionComplete);
    }

    public override string GetActionName()
    {
        return "Spin";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();

        return new List<GridPosition> {unitGridPosition};
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPostion)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPostion,
            actionValue = 0
        };
    }

    public override bool IsValidActionThisTurn()
    {
        return unit.GetIsStunned();
    }

}