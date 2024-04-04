using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MercUnit : MonoBehaviour
{

    [SerializeField] Transform cameraPositionMark;
    [SerializeField] Transform cameraLookAtTarget;
    public string mercName;
    public string price;
    [TextArea]
    public string Abilities;
    bool isHired;

    [SerializeField] Unit unit;



    // Start is called before the first frame update


    public Transform GetCameraPositionMark() { return cameraPositionMark; }
    public Transform GetCameraLookAtTarget() { return cameraLookAtTarget; }

    public void HireMerc()
    {
        isHired = true;
        PartySetup.Instance.AddUnitToList(unit);
    }

    public bool GetIsHired()
    {
        return isHired; 
    }

    public Unit GetUnit()
    {
        return unit;
    }
}
