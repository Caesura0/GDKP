using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class ShotgunAction : BaseAttackAction
{

    public static event EventHandler<OnAttackEventArgs> onAnyAttack;
    public event EventHandler<OnAttackEventArgs> onShoot;




    bool IsKnockedBack;
    Vector3 knockbackLocation;
    IDestructable destructableItem;

    void Update()
    {
        if (!isActive) { return; }

        stateTimer -= Time.deltaTime;
        switch (state)
        {
            case State.Aiming:
                float rotateSpeed = 6.5f;
                Vector3 aimDirection = Vector3.zero;
                if (targetUnit != null) 
                {
                    aimDirection = (targetUnit.GetWorldPosition() - transform.position).normalized;
                }
                //cache this one maybe? to use for hitting behind the next unit
                if (destructableItem != null)
                {
                    aimDirection = (destructableItem.GetWorldPosition() - transform.position).normalized;
                }

                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * rotateSpeed);
                break;
            case State.Shooting:
                if (canShootBullet)
                {
                    Shoot();
                    canShootBullet = false;
                }
                break;
            case State.Cooloff:
                break;
        }
        if (stateTimer <= 0f)
        {
            NextState();
        }


    }




    private void Shoot()
    {



        onAnyAttack?.Invoke(this, new OnAttackEventArgs
        {
            targetUnit = this.targetUnit,
            shootingUnit = unit,
            weaponType = weaponType,
            destructable = destructableItem,
        });

        onShoot?.Invoke(this, new OnAttackEventArgs
        {
            targetUnit = this.targetUnit,
            shootingUnit = unit,
            weaponType = weaponType,
            destructable = destructableItem,
        });

        if(targetUnit != null)
        {
            float hitChance = unit.CalculateAccuracy(targetUnit.GetGridPosition(), this);

            currentAmmo--;
            //Debug.Log(hitChance + " HitChance");

            if (UnityEngine.Random.Range(1, 101) < hitChance)
            {
                targetUnit.Damage(damage);

            }

            else
            {
                Debug.Log("Missed! " + hitChance + " HitChance");
            }
        }

        if(destructableItem != null)
        {
            destructableItem.Damage();
        }


    }


    public override int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList(gridPosition).Count;
    }

    protected override void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.Shooting;
                stateTimer = shootingStateTime;
                break;
            case State.Shooting:
                state = State.Cooloff;
                stateTimer = coolOffStateTime;
                break;
            case State.Cooloff:
                ActionComplete();
                break;
        }
    }


    public override string GetActionName()
    {
        return "Shotgun";
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


    public override List<GridPosition> GetValidActionGridPositionList(GridPosition unitGridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();


        for (int x = -maxAttackDistance; x <= maxAttackDistance; x++)
        {
            for (int z = -maxAttackDistance; z <= maxAttackDistance; z++)
            {
                //this is the combination of x and z that shows how far from the unit we are testing
                GridPosition offsetGridPosition = new GridPosition(x, z);
                //adds the difference to the unit position to get the new grid position to test
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                //checks if the the position is part of the level grid or just background space
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) { continue; }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);

                if (testDistance > maxAttackDistance) { continue; }

                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition) && !LevelGrid.Instance.HasAnyIDestructableOnGridPosition(testGridPosition)) { continue; }


                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);
                    
                    //compares if the team alignment of the attacking unit matches the allignment of the target unit
                    if (targetUnit.IsEnemy() == unit.IsEnemy())
                    {
                        continue;
                    }

                    Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition);
                    //how can i get this to push, maybe i need to cache this at the top
                    Vector3 shootDir = (targetUnit.GetWorldPosition() - unitWorldPosition).normalized;
                    //unitShoulderHeight should be on the unit itself if i have different heights
                    float unitShoulderHeight = 1.7f;


                    //duplicate this to look for units in the way and hit them instead?
                    if (Physics.Raycast(
                        unitWorldPosition + Vector3.up *
                        unitShoulderHeight,
                        shootDir,
                        Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()),
                        obstaclesLayerMask))
                    {
                        continue;
                    }
                }


                validGridPositionList.Add(testGridPosition);
            }

        }
        foreach (var gridPosition in validGridPositionList)
        {
            Debug.Log(gridPosition);
        }
        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {

        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);
        destructableItem = LevelGrid.Instance.GetAnyIDestructableOnGridPosition(gridPosition);

        if(targetUnit != null )
        {
            Debug.Log(targetUnit.unitName = " take action set");
        }

        state = State.Aiming;
        stateTimer = aimingStateTime;
        canShootBullet = true;
        ActionStart(onActionComplete);
    }

    public override void TakeReloadAction(Action onActionComplete)
    {
        state = State.Reloading;
        stateTimer = reloadingStateTime;
        currentAmmo = maxAmmo;
        ActionStart(onActionComplete);
    }



    public override bool IsValidActionThisTurn()
    {
        if (unit.GetIsStunned()) { return false; }

        else if (currentAmmo > 0) { return true; }

        else { return false; }
    }
}
