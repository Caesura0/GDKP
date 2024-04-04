using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAction : BaseAttackAction
{
    
    public static event EventHandler OnAnySwordHit;

    public event EventHandler OnSwordActionStarted;
    public event EventHandler OnSwordActionComplete;


    [SerializeField] int meleeDamage = 100;
    [SerializeField] float afterHitStateTime = 0.5f;
    [SerializeField] float beforeHitStateTime = 0.7f;

    int maxSwordDistance = 1;


    void Update()
    {
        if (!isActive) { return; }

        stateTimer -= Time.deltaTime;
        switch (state)
        {
            case State.SwingingSwordBeforeHit:
                float rotateSpeed = 6.5f;
                Vector3 aimDirection = (targetUnit.GetWorldPosition() - transform.position).normalized;
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * rotateSpeed);
                break;
            case State.SwingingSwordAfterHit:

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
            case State.SwingingSwordBeforeHit:
                state = State.SwingingSwordAfterHit;
                stateTimer = afterHitStateTime;
                targetUnit.Damage(meleeDamage);
                OnAnySwordHit?.Invoke(this, EventArgs.Empty);
                break;
            case State.SwingingSwordAfterHit:
                OnSwordActionComplete?.Invoke(this, EventArgs.Empty);
                ActionComplete();
                break;

        }
    }

    public override string GetActionName()
    {
        return "Smash";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPostion)
    {
        Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPostion);

        if (targetUnit.GetCurrentHealth() <= damage)
        {
            return new EnemyAIAction
            {
                gridPosition = gridPostion,
                actionValue = baseAiActionWeight + baseAiKillWeight + Mathf.RoundToInt((targetUnit.GetHealthNormalized()) * 100),
            };
        }

        return new EnemyAIAction
        {
            gridPosition = gridPostion,
            actionValue = baseAiActionWeight + Mathf.RoundToInt((1 - targetUnit.GetHealthNormalized()) * 100),
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {

        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        state = State.SwingingSwordBeforeHit;
        
        stateTimer = beforeHitStateTime;

        OnSwordActionStarted?.Invoke(this, EventArgs.Empty);

        ActionStart(onActionComplete);
    }

    public int GetMaxSwordDistance()
    {
        return maxSwordDistance;
    }

    public override bool IsValidActionThisTurn()
    {
        return unit.GetIsStunned();
    }

    public override void TakeReloadAction(Action onActionComplete)
    {
        Debug.Log("Mistakes were made in coding");
    }

    public override int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList().Count;
    }

    public override List<GridPosition> GetValidActionGridPositionList(GridPosition gridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        for (int x = -maxSwordDistance; x <= maxSwordDistance; x++)
        {
            for (int z = -maxSwordDistance; z <= maxSwordDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = gridPosition + offsetGridPosition;
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) { continue; }

                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) { continue; }

                Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);

                if (targetUnit.IsEnemy() == unit.IsEnemy())
                {
                    continue;
                }


                validGridPositionList.Add(testGridPosition);
            }

        }

        return validGridPositionList;
    }
}
