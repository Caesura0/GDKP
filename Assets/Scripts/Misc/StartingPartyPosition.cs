using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingPartyPosition : MonoBehaviour
{
    [SerializeField] List<Transform> startingPositionList;

    PartySetup partySetup;

    private void Start()
    {
        partySetup = PartySetup.Instance;
        LoadStartingParty();
    }

    public List<Transform> GetStartingPositionList()
    {
        return startingPositionList;
    }



    public int GetCountOfPositions()
    {
        return startingPositionList.Count;
    }



    private void LoadStartingParty()
    {
        if (partySetup != null)
        {
            List<Transform> startingPositionList = GetStartingPositionList();

            List<Unit> activeUnitList = partySetup.GetActiveUnitList();

            for (int i = 0; i < GetCountOfPositions(); i++)
            {
                if (partySetup.GetActiveUnitList()[i] != null)
                {
                    Instantiate(partySetup.GetActiveUnitList()[i], startingPositionList[i].position, startingPositionList[i].rotation);
                }
            }
        }
    }

}
