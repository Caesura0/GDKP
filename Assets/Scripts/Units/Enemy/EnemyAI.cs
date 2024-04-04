using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MoveAction;
using static UnityEngine.UI.ScrollRect;

public class EnemyAI : MonoBehaviour
{
    public Action<Unit> OnActiveEnemyUnitChanged;

    public static EnemyAI Instance;

    private enum State
    {
        WaitingForEnemyTurn,
        TakingTurn,
        Busy
    }

    private State state;

    float timer;

    private void Awake()
    {
            if (Instance != null)
            {
                Debug.Log("There's more then one EnemyAISystem" + transform + " - " + Instance);
                Destroy(gameObject);
                return;
            }
            Instance = this;
        state = State.WaitingForEnemyTurn;
    }
    private void Start()
    {
        TurnSystem.Instance.OnTurnChange += TurnSystem_OnTurnChanged;
    }

    private void Update()
    {

        if (TurnSystem.Instance.IsPlayerTurn()) { return; }

        switch (state)
        {
            case State.WaitingForEnemyTurn:
                break;
            case State.TakingTurn:
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    if (TryTakeEnemyAIAction(SetStateTakingTurn))
                    {
                        state = State.Busy;
                    }
                    else
                    {
                        OnActiveEnemyUnitChanged?.Invoke(null);
                        TurnSystem.Instance.NextTurn();
                    }
                    
                }
                break;
            case State.Busy:
                break;
        }



    }

    void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            state = State.TakingTurn;
            timer = 2f;
        }
        
    }

    private bool TryTakeEnemyAIAction(Action onEnemyAIActionComplete)
    {
        foreach(Unit enemyUnit in UnitManager.Instance.GetEnemyUnitList())
        {
            if(TryTakeEnemyAIAction(enemyUnit, onEnemyAIActionComplete))
            {
                OnActiveEnemyUnitChanged?.Invoke(enemyUnit);
                return true;
            }
            
        }
        return false;
    }



    void FindBestAIAction(AIType aIType )
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


    public void FindAllAttackActions(Unit enemyUnit)
    {
        EnemyAIAction bestEnemyAIAction = null;
        BaseAction bestBaseAction = null;


        foreach (BaseAction baseAction in enemyUnit.GetBaseActionArray())
        {

            if(baseAction is BaseAttackAction)
            {
                if (!enemyUnit.CanSpendActionPointsToTakeAction(baseAction))
                {
                    continue;
                }
                if (bestBaseAction == null)
                {
                    bestEnemyAIAction = baseAction.GetBestEnemyAIAction();
                    bestBaseAction = baseAction;
                }
                else
                {
                    EnemyAIAction testEnemyAIAction = baseAction.GetBestEnemyAIAction();
                    if (testEnemyAIAction != null && testEnemyAIAction.actionValue > bestEnemyAIAction.actionValue)
                    {
                        bestEnemyAIAction = testEnemyAIAction;
                        bestBaseAction = baseAction;
                    }
                }
            }
        }
    }



    private bool TryTakeEnemyAIAction(Unit enemyUnit, Action onEnemyAIActionComplete)
    {
        EnemyAIAction bestEnemyAIAction = null;
        BaseAction bestBaseAction = null;

        foreach(BaseAction baseAction in enemyUnit.GetBaseActionArray())
        {
            if(!enemyUnit.CanSpendActionPointsToTakeAction(baseAction))
            {
                continue;
            }
            if(bestBaseAction == null)
            {
                bestEnemyAIAction = baseAction.GetBestEnemyAIAction();
                bestBaseAction = baseAction;

            }
            else
            {
                EnemyAIAction testEnemyAIAction = baseAction.GetBestEnemyAIAction();
                
                if(testEnemyAIAction != null && testEnemyAIAction.actionValue > bestEnemyAIAction.actionValue)
                {
                    bestEnemyAIAction = testEnemyAIAction;
                    bestBaseAction = baseAction;
                }
                
            }
            
        }

        if(bestEnemyAIAction != null && enemyUnit.TrySpendActionPointsToTakeAction(bestBaseAction))
        {
            //Debug.Log(enemyUnit + " Trying action " + bestBaseAction.GetActionName());
            bestBaseAction.TakeAction(bestEnemyAIAction.gridPosition ,onEnemyAIActionComplete);
            return true;
        }
        else
        {
            return false;
        }

    }

    private void SetStateTakingTurn()
    {
        timer = 0.5f;
        state = State.TakingTurn;
    }
}
