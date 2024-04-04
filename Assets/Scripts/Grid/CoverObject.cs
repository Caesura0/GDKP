using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverObject : MonoBehaviour
{
    [SerializeField] CoverType coverType;
    GridPosition gridPosition;

    public CoverType GetCoverType()
    {
        return coverType;
    }

    void Start()
    {

        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.SetCoverTypeAtGridPosition(gridPosition, coverType);
        //Debug.Log(gridPosition.x + " X, " + gridPosition.z + " Z " + coverType);
    }
}

public enum CoverType
{
    None,
    Thin,
    Half,
    Full
}
