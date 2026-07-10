using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class ShootAction : BaseAttackAction
{
    

    public static event EventHandler<OnAttackEventArgs> onAnyShoot;
    public event EventHandler<OnAttackEventArgs> onShoot;




    void Update()
    {
        if (!isActive) { return; }

        stateTimer -= Time.deltaTime;
        switch (state)
        {
            case State.Aiming:
                float rotateSpeed = 6.5f;
                Vector3 aimDirection = (targetUnit.GetWorldPosition() - transform.position).normalized;
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



    private bool Shoot()
    {

        bool didHit = false;
        onAnyShoot?.Invoke(this, new OnAttackEventArgs
        {
            targetUnit = targetUnit,
            shootingUnit = unit,
            weaponType = weaponType
        });

        onShoot?.Invoke(this, new OnAttackEventArgs 
        {targetUnit = targetUnit,
        shootingUnit = unit
        });

        float hitChance = CalculateAccuracy(unit.GetGridPosition(), targetUnit.GetGridPosition());

        currentAmmo--;

        //Debug.Log(hitChance + " HitChance"); 

        if (UnityEngine.Random.value <= hitChance)
        {
            int finalDamage = CalculateDamage(targetUnit.GetGridPosition());
            targetUnit.Damage(finalDamage);
            didHit = true;

        }

        else
        {
            Debug.Log("Missed! " + hitChance + " HitChance");
            didHit = false;
        }
        return didHit;
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
        return "Shoot";
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
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) { continue; }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);

                if(testDistance > maxAttackDistance) { continue; }

                if(testDistance < minShootDistance) { continue; }

                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) { continue; }

                Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);

                if (targetUnit.IsEnemy() == unit.IsEnemy())
                {
                    continue;
                }
                //raycast to check if 
                Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition);
                Vector3 shootDir = (targetUnit.GetWorldPosition() - unitWorldPosition).normalized;
                float unitShoulderHeight = 1.7f;
                if(Physics.Raycast(
                    unitWorldPosition + Vector3.up *
                    unitShoulderHeight,
                    shootDir,
                    Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()),
                    obstaclesLayerMask))
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
        Debug.Log(targetUnit.unitName = " take action set in shoot");
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



    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPostion)
    {
        Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPostion);
        int finalDamage = CalculateDamage(targetUnit.GetGridPosition());
        float hitChance = CalculateAccuracy(unit.GetGridPosition(), gridPostion);

        if (targetUnit.GetCurrentHealth() <= finalDamage)
        {
            // Kill shot available: heavily favour, but still weight by hit chance
            int actionValue = Mathf.RoundToInt((baseAiActionWeight + baseAiKillWeight) * hitChance);
            return new EnemyAIAction { gridPosition = gridPostion, actionValue = actionValue };
        }

        // No kill: weight damage dealt against target health, scaled by hit chance
        int nonKillValue = Mathf.RoundToInt(baseAiActionWeight * (1 - targetUnit.GetHealthNormalized() + 0.1f) * hitChance);
        return new EnemyAIAction { gridPosition = gridPostion, actionValue = nonKillValue };
    }


    public override int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList(gridPosition).Count;
    }




    public override bool IsValidActionThisTurn()
    {
        if (unit.GetIsStunned()) { return false; }

        else if (currentAmmo > 0) { return true; }

        else { return false; }
    }

    //This one will be for later when i am calculating a more optimal move. Was a fun exercise to figure out though.
    public List<GridPosition> KillShotAvailableGridPostionList(GridPosition gridPosition, out Dictionary<GridPosition, int> killShotGridPositionDictionary )
    {
        List<GridPosition> bestActionGridPositionList = new List<GridPosition>();
        List<GridPosition> availbleGridPositionActionList = GetValidActionGridPositionList(gridPosition);
        Dictionary<GridPosition,int> gridPositionPoints = new Dictionary<GridPosition,int>();
        
        //ok, so i could build a list of enemyAiActions OR a new struct here. 
        //instead of storing int, store a bunch of information in a dictionary so i could do some comparisons
        foreach(GridPosition actionPosition in availbleGridPositionActionList)
        {
            Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(actionPosition);
            int finalDamage = CalculateDamage(actionPosition);
            int killDamageCalculation = (int)targetUnit.GetCurrentHealth() - finalDamage;
            if ( killDamageCalculation >= 0)
            {
                gridPositionPoints.Add(gridPosition, killDamageCalculation);           
            }
        }

        int maxValue = gridPositionPoints.Values.Max();

        foreach(var bestValues in gridPositionPoints)
        {
            if(bestValues.Value == maxValue)
            {
                bestActionGridPositionList.Add(bestValues.Key);
            }
        }
        killShotGridPositionDictionary = gridPositionPoints;
        return bestActionGridPositionList;
    }



}
