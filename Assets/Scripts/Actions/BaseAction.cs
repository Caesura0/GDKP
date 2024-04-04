using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour
{
    public static event EventHandler OnAnyActionStart;
    public static event EventHandler OnAnyActionCompleted;

    [SerializeField] protected int baseAiActionWeight = 100;

    protected Unit unit;
    protected bool isActive;
    protected Action onActionComplete;

    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public abstract string GetActionName();

    public abstract void TakeAction(GridPosition gridPosition, Action onActionComplete);

    public abstract List<GridPosition> GetValidActionGridPositionList();



    public abstract EnemyAIAction GetEnemyAIAction(GridPosition gridPostion);

    public virtual bool IsValidActionGridPosition(GridPosition gridPosition)
    {
        List<GridPosition> validGridPositionList = GetValidActionGridPositionList();
        return validGridPositionList.Contains(gridPosition);
    }


    public virtual int GetActionPointsCost()
    {
        return 1;
    }

    protected void ActionStart(Action onActionComplete)
    {
        isActive = true;
        this.onActionComplete = onActionComplete;
        OnAnyActionStart?.Invoke(this, EventArgs.Empty);
    }



    protected void ActionComplete()
    {
        isActive = false;
        onActionComplete();
        unit.UpdateCoverType();
        OnAnyActionCompleted?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetUnit()
    {
        return unit;
    }


    public abstract bool IsValidActionThisTurn();


    /// <summary>
    /// Loops through all valid Action Grid Positions for the action and gets the EnemyAIAction for that Grid Postion and action to add to a list. \n 
    /// Then will then compare all the actions in the list and find the highest rating
    /// Actual Calculation for EnemyAIAction is done in the action class itself
    /// </summary>

    public EnemyAIAction GetBestEnemyAIAction()
    {
        List<EnemyAIAction> enemyAIActionList = new List<EnemyAIAction>();

        List<GridPosition> validActionGridPostionList = GetValidActionGridPositionList();

        foreach(GridPosition gridPostion in validActionGridPostionList)
        {
            EnemyAIAction enemyAIAction = GetEnemyAIAction(gridPostion);
            enemyAIActionList.Add(enemyAIAction);
        }

        if(enemyAIActionList.Count > 0)
        {
            enemyAIActionList.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue - a.actionValue);
            return enemyAIActionList[0];
        }
        else
        {
            return null;
        }
    }

    public bool GetIsActive()
    {
        return isActive;
    }



    /// <summary>
    /// Loops through all valid Action Grid Positions for the action and gets the EnemyAIAction for that Grid Postion and action to add to a list. \n 
    /// Then will then compare all the actions in the list and find the highest rating
    /// Actual Calculation for EnemyAIAction is done in the action class itself
    /// </summary>

    //public EnemyAIAction GetBestEnemyAIAction(AIType aIType)
    //{
    //    List<EnemyAIAction> enemyAIActionList = new List<EnemyAIAction>();

    //    List<GridPosition> validActionGridPostionList = GetValidActionGridPositionList();

    //    foreach (GridPosition gridPostion in validActionGridPostionList)
    //    {
    //        EnemyAIAction enemyAIAction = GetEnemyAIAction(gridPostion);
    //        enemyAIActionList.Add(enemyAIAction);
    //    }

    //    if (enemyAIActionList.Count > 0)
    //    {
    //        enemyAIActionList.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue - a.actionValue);
    //        return enemyAIActionList[0];
    //    }
    //    else
    //    {
    //        return null;
    //    }
    //}

}
