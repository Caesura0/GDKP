using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    public static TurnSystem Instance { get; private set; }

    public static EventHandler OnAnyTurnChanged;

    int turnNumber = 1;
    bool isPlayerTurn = true;

    public EventHandler OnTurnChange;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("There's more then one unitactionsystem" + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void NextTurn()
    {
        turnNumber++;
        isPlayerTurn = !isPlayerTurn;
        OnTurnChange?.Invoke(this, EventArgs.Empty);
        OnAnyTurnChanged?.Invoke(this, EventArgs.Empty);
    }
    public int GetTurnNumber()
    {
        return turnNumber;
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }
}
