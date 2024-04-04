using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectedVisual : MonoBehaviour
{
    [SerializeField] Unit unit;
    MeshRenderer meshRenderer;


    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        UnitActionSystem.Instance.OnSelectedUnitChange += UnitActionSystem_OnSelectedUnitChanged;
        UpdateVisual();
    }


    private void UnitActionSystem_OnSelectedUnitChanged(object sender, EventArgs empty)
    {

        UpdateVisual();
    }

    void UpdateVisual()
    {
        meshRenderer.enabled = (unit == UnitActionSystem.Instance.GetSelectedUnit());
    }

    private void OnDestroy()
    {
        UnitActionSystem.Instance.OnSelectedUnitChange -= UnitActionSystem_OnSelectedUnitChanged;
    }
}
