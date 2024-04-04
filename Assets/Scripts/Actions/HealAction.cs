using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealAction : BaseAction
{
    [SerializeField] ParticleSystem healVFX;

    public event EventHandler OnHealActionStarted;
    public event EventHandler OnHealActionComplete;

    [SerializeField] int maxHealDistance = 1;
    [SerializeField] int healAmount = -50;

    Unit targetUnit;
    State state;

    float stateTimer;

    private enum State
    {
        BeforeHealing,
        AfterHealing
    }


    private void Update()
    {
        if (!isActive) { return; }

        stateTimer -= Time.deltaTime;
        switch (state)
        {
            case State.BeforeHealing:
                float rotateSpeed = 6.5f;
                Vector3 aimDirection = (targetUnit.GetWorldPosition() - transform.position).normalized;
                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * rotateSpeed);
                break;
            case State.AfterHealing:

                break;

        }

        if (stateTimer <= 0f)
        {
            NextState();
        }
    }


    public override string GetActionName()
    {
        return "Patch up";
    }

    void NextState()
    {
        switch (state)
        {
            case State.BeforeHealing:
                state = State.AfterHealing;
                float afterHitStateTime = 0.5f;
                stateTimer = afterHitStateTime;
                targetUnit.Damage(healAmount);
                Instantiate(healVFX, targetUnit.GetWorldPosition(), Quaternion.Euler(-90, 0, 0));
                OnHealActionStarted?.Invoke(this, EventArgs.Empty);
                break;
            case State.AfterHealing:
                OnHealActionComplete?.Invoke(this, EventArgs.Empty);
                ActionComplete();
                break;

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
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();
        for (int x = -maxHealDistance; x <= maxHealDistance; x++)
        {
            for (int z = -maxHealDistance; z <= maxHealDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) { continue; }

                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) { continue; }

                Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
                if(targetUnit.GetHealthNormalized() == 1)
                {
                    continue;
                }
                if (targetUnit.IsEnemy() != unit.IsEnemy())
                {
                    continue;
                }


                validGridPositionList.Add(testGridPosition);
            }

        }

        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        state = State.BeforeHealing;
        float beforeHitStateTime = 0.7f;
        stateTimer = beforeHitStateTime;
        ActionStart(onActionComplete);
        
        
    }

    public override bool IsValidActionThisTurn()
    {
        return unit.GetIsStunned();
    }


}
