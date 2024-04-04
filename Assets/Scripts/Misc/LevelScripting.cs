using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScripting : MonoBehaviour
{

    [SerializeField] private List<GameObject> Room1; 
    [SerializeField] private List<GameObject> middleRoom;
    [SerializeField] private List<GameObject> foyer;
    [SerializeField] private List<GameObject> finalRoom;
    [SerializeField] private List<GameObject> EnemyRoom1;
    [SerializeField] private List<GameObject> foyerEnemy;
    [SerializeField] private List<GameObject> finalRoomEnemy;
    [SerializeField] private Door door1;
    [SerializeField] private Door door2;

    private bool hasShownFirstHider = false;
    private bool hasShownSecondHider = false;
    bool crateDestroyed;

    private void Start()
    {
        LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMovedGridPosition;
        door1.OnDoorOpened += (object sender, EventArgs e) =>
        {
            SetActiveGameObjectList(middleRoom, false);
        };
        door2.OnDoorOpened += (object sender, EventArgs e) =>
        {
            SetActiveGameObjectList(foyer, false);
            if (crateDestroyed)
            {
                SetActiveGameObjectList(foyerEnemy, true);
            }
            
        };
    }

    private void LevelGrid_OnAnyUnitMovedGridPosition(object sender, LevelGrid.OnAnyUnitMovedGridPositionEventArgs e)
    {
        //trigger for room1
        if (e.toGridPosition.x == 12 && !hasShownFirstHider)
        {
            hasShownFirstHider = true;
            SetActiveGameObjectList(Room1, false);
            SetActiveGameObjectList(EnemyRoom1, true);
        }

        if ((e.toGridPosition.x == 7 || e.toGridPosition.x == 6) && e.toGridPosition.z > 10 && !hasShownSecondHider)
        {
            hasShownSecondHider = true;
            SetActiveGameObjectList(finalRoom, false);
            SetActiveGameObjectList(finalRoomEnemy, true);
        }


    }

    private void SetActiveGameObjectList(List<GameObject> gameObjectList, bool isActive)
    {
        foreach (GameObject gameObject in gameObjectList)
        {
            gameObject.SetActive(isActive);
        }
    }
}
