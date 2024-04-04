using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : BaseAction
{



    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;

    [SerializeField] float rotateSpeed = 10f;
    [SerializeField] int maxMoveDistance = 6;
    //[SerializeField] AIType aIMovementType = AIType.Waiting;

    CharacterController controller;

    int currentPositionIndex;

    List<Vector3> positionList;


    private void Update()
    {
        if (!isActive) { return; }

        Vector3 targetPosition = positionList[currentPositionIndex];

        float stoppingDistance = .1f;
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);

        // Calculate the rotation needed to face the movement direction
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

        // Smoothly rotate towards the target rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

        if (distance > stoppingDistance)
        {
            float moveSpeed = 4f;
            // transform.position += moveDirection * moveSpeed * Time.deltaTime;

            //added from gamedev forum recomendation, prevents 'overshooting' movement when FPS drops
            var step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, positionList[currentPositionIndex], step);


        }
        else
        {
            currentPositionIndex++;

            if (currentPositionIndex < positionList.Count)
            {
                Vector3 nextPos = positionList[currentPositionIndex];
                //Debug.Log(positionList[currentPositionIndex] + "Pre update");
                nextPos.y = LevelGrid.Instance.GetLevelGridTransform().y + 1.8f;
                RaycastHit hit;
                Physics.Raycast(nextPos, Vector3.down, out hit, LevelGrid.Instance.GetWalkableLayers());
                Vector3 newTargetPosition = positionList[currentPositionIndex];
                newTargetPosition.y = hit.point.y;
                positionList[currentPositionIndex] = newTargetPosition;
               // Debug.Log(positionList[currentPositionIndex] + "Post update");
               // Quaternion rotatation = transform.rotation;
                //targetPosition = transform.position;
                //.x = 0f;
                //transform.rotation = rotatation;
            }

            if (currentPositionIndex >= positionList.Count)
            {
                Quaternion rotatation = transform.rotation;
                rotatation.x = 0f;
                transform.rotation = rotatation;

                OnStopMoving?.Invoke(this, EventArgs.Empty);
                ActionComplete();
            }

        }
        

    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {

        List<GridPosition> gridPositionList = PathFinding.Instance.
        FindPath(unit.GetGridPosition(), gridPosition, out int pathLength);
        currentPositionIndex = 0;
        positionList = new List<Vector3>();

        foreach(GridPosition pathGridPositionList in gridPositionList)
        {
            positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPositionList));
        }



        OnStartMoving?.Invoke(this, EventArgs.Empty);
        ActionStart(onActionComplete);
    }


    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -maxMoveDistance; x <= maxMoveDistance; x++)
        {
            for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;
                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) { continue; }
                if(unitGridPosition == testGridPosition) { continue; }
                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) { continue; }
                if (!PathFinding.Instance.IsWalkableGridPosition(testGridPosition)) { continue; }
                var path = PathFinding.Instance.GetPathLength(unitGridPosition, testGridPosition);

                int pathFindingDistanceMultiplier = 10;
                if (path <= 0 || path > maxMoveDistance * pathFindingDistanceMultiplier ) 
                { 
                    //Path Length is too long or short
                    continue; 
                }
                validGridPositionList.Add(testGridPosition);
            }
           
        }

        return validGridPositionList;
    }


    public override string GetActionName()
    {
        return "Move";
    }



    #region EnemyAI Section



    void FindBestAIAction(AIType aIType)
    {
        switch (aIType)
        {
            case AIType.Waiting:
                //calculate if you can attack any enemies without moving
                

                //return max amount for this action
                //this should be paired with overwatch
                //otherwise wait

                break;
            case AIType.Guarding:
                //calculate if you can attack any enemies without moving
                //return max amount for this action
                //If you can do one move and attack this is secondary
                //otherwise wait
                break;
            case AIType.Hunting:
                //Find nearest player unit
                //Move into attacking distance
                //Kill unit once you are within range
                //switch to next closest unit only if target unit leaves attack range and another one is in range
                break;
            case AIType.Roaming:
                //Move between to points OR within an area(figure that out later
                //if Enemy enters your attack range, attack
                break;

        }
    }



    


    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPostion)
    {
        int coverBonus = 0;

        //find location with cover first
        if ((int)LevelGrid.Instance.GetCoverAccuracyReduction(unit.GetGridPosition()) == 0)
        {
            //Debug.Log( unit.name +  " not in cover");
            coverBonus = (int)LevelGrid.Instance.GetCoverAccuracyReduction(gridPostion);
        }

        //then add points for able to attack from that position
        int targetCountAtGridPosition = 0;
        if (unit.GetAction<BaseAttackAction>().GetTargetCountAtPosition(gridPostion)> 0)
        {
            targetCountAtGridPosition = 1;
        }

        //TODO:if you can kill a unit, worth leaving cover


        //TODO:this section should return the number of targets availble, and then a bool on if any of them are 'kill shots' less iteration and cleaner code
        int targetKillShotAtGridPosition = 0;

        foreach(var baseAttackAction in unit.GetActionList<BaseAttackAction>())
        {
            if (baseAttackAction.KillShotAvailbleAtGridPosition(gridPostion))
            {
                targetKillShotAtGridPosition = 1;
                break;
            }
        }


        //leave cover if you are a hunter and you will not be attacked in that position
        //should look for best target to attack after being in cover, rather then most targets

        //Debug.LogWarning(unit.name + " move actions returned " + (targetCountAtGridPosition + coverBonus + targetKillShotAtGridPosition) * 10);

        return new EnemyAIAction
        {
            gridPosition = gridPostion,
            actionValue = (targetCountAtGridPosition + coverBonus + targetKillShotAtGridPosition) * 10,
        };


    }



    #endregion

    public override bool IsValidActionThisTurn()
    {
        return !unit.GetIsStunned();
    }



}
