using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeAction : BaseAction
{

    public event EventHandler OnGrenadeActionStarted;
    public event EventHandler OnGrenadeActionComplete;

    [SerializeField] LayerMask obstaclesLayerMask;
    [SerializeField] Transform grenadeProjectilePrefab;
    [SerializeField] float damageRadius = 4f;
    [SerializeField] int maxThrowDistance = 6;
    [SerializeField] int startingGrenades = 3;
     
    [SerializeField] float windUpTime = 1.5f;
    GridPosition targetGridPosition;
    float stateTimer;
    State state;

    bool canThrowGrenade;

    int currentGrenades;


    private enum State
    {
        Aiming,
        Throwing,
        WindUp,
        Cooloff
    }

    private void Start()
    {
        currentGrenades = startingGrenades;
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
            case State.WindUp:
                
                break;
            case State.Throwing:
                if (canThrowGrenade)
                {
                    ThrowGrenade(targetGridPosition);
                    canThrowGrenade = false;
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

    void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.WindUp;
                float aimingStateTime = 1.65f;
                stateTimer = aimingStateTime;
                OnGrenadeActionStarted?.Invoke(this, EventArgs.Empty);
                break;
            case State.WindUp:
                state = State.Throwing;
                stateTimer = windUpTime;
                break;
            case State.Throwing:
                state = State.Cooloff;
                float coolOffStateTime = .5f;
                stateTimer = coolOffStateTime;
                break;
            case State.Cooloff:
                ActionComplete();
                break;
        }
    }

    public override string GetActionName()
    {
        return "Grenade";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPostion)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPostion,
            actionValue = 0 // make it so if you are going to hit 2 or 3(decide later based on damage) score this one better. subtract if you will be hitting your own units
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        GridPosition unitGridPosition = unit.GetGridPosition();
        for (int x = -maxThrowDistance; x <= maxThrowDistance; x++)
        {
            for (int z = -maxThrowDistance; z <= maxThrowDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) { continue; }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);

                if (testDistance > maxThrowDistance) { continue; }


                validGridPositionList.Add(testGridPosition);
            }

        }
        
        return validGridPositionList;
    }


    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        
        targetGridPosition = gridPosition;
        state = State.Aiming;
        float aimingStateTime = 1f;
        stateTimer = aimingStateTime;
        canThrowGrenade = true;
        ActionStart(onActionComplete);
    }


    private void ThrowGrenade(GridPosition gridPosition)
    {
        Transform grenadeProjectileTransform = Instantiate(grenadeProjectilePrefab, unit.GetWorldPosition(), Quaternion.identity);
        GrenadeProjectile grenadeProjectile = grenadeProjectileTransform.GetComponent<GrenadeProjectile>();
        grenadeProjectile.Setup(gridPosition, OnGrenadeBehaviorComplete, damageRadius);
        currentGrenades = Mathf.Clamp(currentGrenades - 1, 0, currentGrenades);
    }


    void OnGrenadeBehaviorComplete()
    {
        OnGrenadeActionComplete?.Invoke(this, EventArgs.Empty);
        ActionComplete();
    }

    public float GetDamageRadius()
    {
        return damageRadius;
    }

    public override bool IsValidActionThisTurn()
    {
        if (unit.GetIsStunned()) { return false; }

        if(currentGrenades == 0 ) { return false; }

        return true;
    }

    public int GetCurrentGrendades()
    {
        return currentGrenades;
    }

    
    public void PickUpGrenade()
    {
        currentGrenades++;
    }


}
