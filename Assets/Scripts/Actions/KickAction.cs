using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class KickAction : BaseAttackAction
{
    public static event EventHandler<OnAttackEventArgs> onAnyAttack;
    public static event EventHandler OnAnyKickHit;

    public event EventHandler OnKickactionStarted;
    public event EventHandler OnKickActionComplete;

    [SerializeField] int knockbackMultiplier = 3;


    [SerializeField] int meleeDamage = 20;
    [SerializeField] float afterHitStateTime = 0.5f;
    [SerializeField] float beforeHitStateTime = 0.7f;

    int maxKickDistance = 1;




    void Update()
    {
        if (!isActive) { return; }

        stateTimer -= Time.deltaTime;
        switch (state)
        {
            case State.KickBeforeHit:
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
            case State.KickBeforeHit:
                state = State.SwingingSwordAfterHit;
                stateTimer = afterHitStateTime;
                Kick(meleeDamage);//maybe i should pass ActionComplete() as a delegate here
                OnAnyKickHit?.Invoke(this, EventArgs.Empty);
                break;
            case State.SwingingSwordAfterHit:
                OnKickActionComplete?.Invoke(this, EventArgs.Empty);
                onAnyAttack?.Invoke(this, new OnAttackEventArgs
                {
                    targetUnit = this.targetUnit,
                    shootingUnit = unit,
                    weaponType = weaponType
                });
                ActionComplete();
                break;

        }
    }

    public override string GetActionName()
    {
        return "Front Kick";
    }



    public override int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList().Count;
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
        state = State.KickBeforeHit;

        stateTimer = beforeHitStateTime;

        OnKickactionStarted?.Invoke(this, EventArgs.Empty);

        ActionStart(onActionComplete);
    }


    public override bool IsValidActionThisTurn()
    {
        if (unit.GetIsStunned()) { return false; }

        return true;

    }

    private void Kick(int Damage)
    {

        Vector3 aimDirection = (targetUnit.GetWorldPosition() - transform.position).normalized;

        //this needs to be grabbed from the grid/pathfinding scripts
        int gridMultiplier = 2;
        Vector3 rawKnockbackLocation = targetUnit.GetWorldPosition() + aimDirection * knockbackMultiplier * gridMultiplier;
        GridPosition knockbackGridPosition = LevelGrid.Instance.GetGridPosition(rawKnockbackLocation);

        targetUnit.Damage(Damage);

        if (LevelGrid.Instance.IsValidGridPosition(knockbackGridPosition) && !LevelGrid.Instance.HasAnyUnitOnGridPosition(knockbackGridPosition))
        {
            targetUnit.TriggerKnockback(LevelGrid.Instance.GetWorldPosition(knockbackGridPosition), aimDirection, knockbackGridPosition, targetUnit.GetGridPosition() );
        }
        else if (LevelGrid.Instance.HasAnyUnitOnGridPosition(knockbackGridPosition))
        {
            Debug.Log("do stun and damage behavior");
        }
    }


    

    public int GetMaxKickDistance()
    {
        return maxKickDistance;
    }

    public override void TakeReloadAction(Action onActionComplete)
    {
        state = State.Reloading;
        stateTimer = reloadingStateTime;
        currentAmmo = maxAmmo;
        ActionStart(onActionComplete);
    }



    public override List<GridPosition> GetValidActionGridPositionList(GridPosition gridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        for (int x = -maxKickDistance; x <= maxKickDistance; x++)
        {
            for (int z = -maxKickDistance; z <= maxKickDistance; z++)
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
