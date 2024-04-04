using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class Unit : MonoBehaviour
{

    const int ACTION_POINTS_MAX = 2;

    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static event EventHandler OnAnyUnitDead;

    public static event EventHandler OnCoverTypeChanged;
    [SerializeField] LayerMask walkingLayers;

    [SerializeField] bool isEnemy;
    public string unitName;

    HealthSystem healthSystem;
    GridPosition gridPosition;
    BaseAction[] baseActionArray;
    int actionPoints;


    private CoverType coverType;
    bool isKnockedBack;
    bool isStunned;
    bool isCaughtByOverwatch;
    Vector3 knockbackLocation;
    Vector3 knockbackDirection;

    float unitShoulderHeight = 1.6f;

    [SerializeField] int collisionLayerMask;

    [SerializeField] AIType aIType = AIType.Waiting;
    [SerializeField] bool debugMode = false;



    private void Awake()
    {

        baseActionArray = GetComponents<BaseAction>();
        healthSystem = GetComponent<HealthSystem>();
        actionPoints = ACTION_POINTS_MAX;
    }

    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.AddUnitAtGridPosition(gridPosition, this);
        TurnSystem.Instance.OnTurnChange += TurnSystem_OnTurnChange;
        healthSystem.onDie += HealthSystem_OnDead;
        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);
        UpdateCoverType();
    }
    private void Update()
    {

        GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);

        if (isKnockedBack)
        {
            KnockbackCheck();
        }
        if (newGridPosition != gridPosition)
        {
            GridPosition oldGridPosition = gridPosition;
            gridPosition = newGridPosition;
            LevelGrid.Instance.UnitMovedGridPosition(this, oldGridPosition, newGridPosition);
        }

    }

    public LayerMask GetWalkableLayers()
    {
        return walkingLayers;
    }

    public void KeepUnitModelLevel(Vector3 unitPosition)
    {
        Vector3 nextPos = unitPosition;
        nextPos.y += .5f;
        RaycastHit hit;
        Physics.Raycast(nextPos, Vector3.down, out hit, walkingLayers);
        Vector3 newPosition = unitPosition;
        newPosition.y = hit.point.y;
        transform.position = newPosition;

    }


    public void Damage(int damageAmount)
    {
        //add hit animation
        //add secondary animation for knockback
        //maybe knockback should just be an optional part of this
        healthSystem.Damage(damageAmount);
    }

    public void UpdateCoverType()
    {
        coverType = LevelGrid.Instance.GetBestCoverTypeAtGridPosition(gridPosition);
        OnCoverTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    public CoverType GetCoverType()
    {
        return coverType;
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }

    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }

    public BaseAction[] GetBaseActionArray()
    {
        return baseActionArray;
    }

    public bool CanSpendActionPointsToTakeAction(BaseAction baseAction)
    {
        return (actionPoints >= baseAction.GetActionPointsCost() && !isStunned);

    }


    public bool TrySpendActionPointsToTakeAction(BaseAction baseAction)
    {

        if (CanSpendActionPointsToTakeAction(baseAction))
        {
            //TODO check if AttackAction, then open up menu
            if (baseAction is BaseAttackAction)
            {
                //Debug.Log("AttackAction");
            }
            SpendActionPoints(baseAction.GetActionPointsCost());
            return true;
        }
        else
        {
            return false;
        }
    }

    public void TurnSystem_OnTurnChange(object sender, EventArgs e)
    {
        if ((IsEnemy() && !TurnSystem.Instance.IsPlayerTurn()) ||
            (!IsEnemy() && TurnSystem.Instance.IsPlayerTurn()))
        {
            actionPoints = ACTION_POINTS_MAX;
            OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
        }

    }

    /// <summary>
    /// Returns first action of type "T"
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetAction<T>() where T : BaseAction
    {
        foreach (BaseAction baseAction in baseActionArray)
        {
            if (baseAction is T)
            {
                //this is ok, because we don't have more then 1 of the same action
                return (T)baseAction;
            }
        }
        return null;
    }



    public List<T> GetActionList<T>() where T : BaseAttackAction
    {
        List<T> actionList = new List<T>();
        foreach (BaseAction baseAction in baseActionArray)
        {
            if (baseAction is T)
            {
                //this is ok, because we don't have more then 1 of the same action
                actionList.Add((T)baseAction);
            }
        }
        return actionList;
    }



    public void ClearAllActionPoints()
    {
        actionPoints = 0;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void SpendActionPoints(int amount)
    {

        actionPoints -= amount;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetActionPoints()
    {

        return actionPoints;
    }

    public bool IsEnemy()
    {
        return isEnemy;
    }

    public bool GetIsStunned()
    {
        return isStunned;
    }

    public float GetHealthNormalized()
    {
        return healthSystem.GetHealthNormalized();
    }

    public float GetCurrentHealth()
    {
        return healthSystem.GetCurrentHealth();
    }

    void HealthSystem_OnDead(object sender, EventArgs e)
    {
        LevelGrid.Instance.ClearUnitAtGridPosition(gridPosition, this);
        Destroy(gameObject);
        OnAnyUnitDead?.Invoke(this, EventArgs.Empty);
    }

    public void TriggerKnockback(Vector3 knockbackPos, Vector3 knockbackDirection, GridPosition knockbackGridPosition, GridPosition originalGridPosition)
    {


        var knockbackLocation = GetKnockBackValidPath(knockbackPos, knockbackGridPosition, out Unit collidedUnit);

        this.knockbackLocation = LevelGrid.Instance.GetWorldPosition(knockbackLocation);
        this.knockbackDirection = knockbackDirection;
        isKnockedBack = true;
        if (collidedUnit != null)
        {
            collidedUnit.SetIsStunned(true);
            SetIsStunned(true);
        }
        //calculate the path to the knocked back location, include all locations in a straight line
        //use something similar to the move function to move to each node path
        //add conditions 
        //if(next position is wall, don't move, stun THIS, damage THIS
        //if(next position is unit, don't move, stun THIS and unit in target location, lightly damage THIS and unit in target location


    }
    //maybe this should be in pathfinding...
    private GridPosition GetKnockBackValidPath(Vector3 knockbackPos, GridPosition knockbackGridPosition, out Unit collidedUnit)
    {
        int gridSize = 2;

        GridPosition startPos = GetGridPosition();

        Debug.Log("StartPos: " + startPos.x + "," + startPos.z + "EndPos: " + knockbackGridPosition.x + "," + knockbackGridPosition.z);

        // Calculate the direction vector from the start to end position
        Vector3 knockbackDir = (knockbackPos - transform.position).normalized;

        Debug.Log(knockbackDir + " KnockbackDir");
        int steps = Mathf.FloorToInt(Vector3.Distance(transform.position, knockbackPos) / gridSize);
        Debug.Log(steps + " Steps");
        // Calculate the current position
        GridPosition currentPosition = GetGridPosition();

        List<GridPosition> validKnockBackList = new List<GridPosition>();
        GridPosition finalKnockbackLocation = currentPosition;
        collidedUnit = null;

        for (int i = 0; i <= steps; i++)
        {
            Debug.Log(i + " current i count");
            if (!LevelGrid.Instance.IsValidGridPosition(currentPosition))
            {
                Debug.Log("should not be the case " + currentPosition);
            }
            if (CheckGridPosition(currentPosition, out collidedUnit))
            {
                finalKnockbackLocation = currentPosition;
                validKnockBackList.Add(currentPosition);
            }
            else
            {
                Debug.Log("exiting early");
                return finalKnockbackLocation;
            }


            Debug.DrawRay(LevelGrid.Instance.GetWorldPosition(currentPosition), Vector3.up, Color.yellow);

            Debug.Log(Mathf.RoundToInt(knockbackDir.x));
            Debug.Log(Mathf.RoundToInt(knockbackDir.z));


            // Move to the next step along the direction vector
            currentPosition.x += Mathf.RoundToInt(knockbackDir.x);
            currentPosition.z += Mathf.RoundToInt(knockbackDir.z);
            Debug.Log("after current position");

        }
        Debug.Log("made it through GetKnockBackValidPath ");
        return finalKnockbackLocation;
    }

    bool CheckGridPosition(GridPosition gridPosition, out Unit collidedUnit)
    {
        bool isLocationClear = true;
        Debug.Log("Checking grid position: " + gridPosition);
        // Perform actions for each grid position (e.g., check for obstacles, apply knockback, etc.)
        Unit unit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition); //LevelGrid.Instance.GetAnyUnitAtGridPosition(gridPosition)[0];
        if (unit == this) unit = null;
        if (unit != null)
        {
            Debug.Log("unit in location " + gridPosition);
            isLocationClear = false;
        }
        if (!PathFinding.Instance.IsWalkableGridPosition(gridPosition))
        {
            Debug.Log("Location not walkable " + gridPosition);
            isLocationClear = false;
        }

        // Add your logic here based on the grid position
        collidedUnit = unit;
        return isLocationClear;
    }

    private void KnockbackCheck()
    {
        float stoppingDistance = .1f;
        float distance = Vector3.Distance(transform.position, knockbackLocation);
        if (Physics.Raycast(
                    transform.position + Vector3.up *
                    unitShoulderHeight,
                    knockbackDirection,
                    .25f,
                    collisionLayerMask))
        {
            Debug.Log("collided");
        }

        if (distance > stoppingDistance)
        {

            float moveSpeed = 10f;
            var step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, knockbackLocation, step);

        }
        else
        {
            isKnockedBack = false;
        }

    }

    public AIType GetAIType()
    {
        return aIType;
    }

    public float CalculateAccuracy(GridPosition unitGridPosition, BaseAttackAction attackAction)
    {
        PathFinding.Instance.FindPath(GetGridPosition(), unitGridPosition, out int pathLength);
        if (debugMode) { Debug.LogWarning("Path " + pathLength); }
        if (debugMode) { Debug.LogWarning("UnitGridPosition " + unitGridPosition); }

        float enemyCoverPercentReduction = (100f - LevelGrid.Instance.GetCoverAccuracyReduction(unitGridPosition)) / 100f;
        if (debugMode) { Debug.LogWarning("enemyCoverPercentReduction " + enemyCoverPercentReduction); }
        float distanceModifier = pathLength / 10; //calculates the path score and divides it by 10, gets the actual spaces
        if (debugMode) { Debug.LogWarning("pathLength " + pathLength / 10); }
        float accuarcyReduction = distanceModifier / attackAction.GetMaxAttackDistance();  //returns a value between 0 and 1, 1 will apply the max"distance penilty"
        if (debugMode) { Debug.LogWarning("accuarcyReduction " + accuarcyReduction); }
        if (debugMode) { Debug.LogWarning("GetMaxAttackDistance " + attackAction.GetMaxAttackDistance()); }
        float hitChance = (attackAction.GetAccuracy() - accuarcyReduction * attackAction.GetDistancePenaltyWeight()) * enemyCoverPercentReduction * 100;
        if (debugMode) { Debug.LogWarning("(" + attackAction.GetAccuracy() + " - " + attackAction.GetMaxAttackDistance() + " * " + attackAction.GetDistancePenaltyWeight() + ")" + enemyCoverPercentReduction + " * 100"); }
        return hitChance;
    }

    public void SetIsStunned(bool isStunned)
    {
        Debug.Log("you is stunned");
        this.isStunned = isStunned;
    }

    public void SetIsCaugtByOverwatch(bool isCaughtByOverwatch)
    {
        Debug.Log("you is caught");
        this.isCaughtByOverwatch = isCaughtByOverwatch;
    }

    public bool GetIsCaugtByOverwatch()
    {
        return isCaughtByOverwatch;
    }
}