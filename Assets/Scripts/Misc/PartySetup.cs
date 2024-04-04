using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartySetup : MonoBehaviour
{
    [SerializeField] List<Unit> unitList;


    List<Unit> activeUnitList = new List<Unit>();
    StartingPartyPosition startingPartyPosition;

    public static PartySetup Instance { get; private set; }

    private void Awake()
    {

        if(Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }



    private void Start()
    {
        activeUnitList = unitList;

    }

    public List<Unit> GetActiveUnitList()
    {
        return activeUnitList;
    }

    public void AddUnitToList(Unit unit)
    {
        if (!unitList.Contains(unit))
        {
            unitList.Add(unit);
        }

    }

    public void RemoveUnitFromList(Unit unit)
    {
        unitList.Remove(unit);
    }
}
