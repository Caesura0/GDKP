using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InCoverUI : MonoBehaviour
{
    [SerializeField] GameObject CoverIcon;
    [SerializeField] Unit unit;

    [SerializeField] Sprite fullCover;
    [SerializeField] Sprite halfCover;
    [SerializeField] Sprite thinCover;

    Sprite sprite;
    //this should likely be on the general UI script
    private void Start()
    {
        Unit.OnCoverTypeChanged += UnitActionSystem_OnSelectedUnitChange;
        SetCoverUI();
        //Debug.Log("InCoverUI");
        sprite = CoverIcon.GetComponent<Sprite>();
    }

    private void UnitActionSystem_OnSelectedUnitChange(object sender, EventArgs e)
    {
        SetCoverUI();
    }

    private void SetCoverUI()
    {
        //Debug.Log(unit.GetCoverType() + unit.unitName);
        CoverType coverType = LevelGrid.Instance.GetBestCoverTypeAtGridPosition(unit.GetGridPosition());

        switch (coverType)
        {
            case CoverType.Full:
                CoverIcon.SetActive(true);
                sprite = fullCover;
                break;
            case CoverType.Half:
                CoverIcon.SetActive(true);
                sprite = halfCover;
                break;
            case CoverType.Thin:
                CoverIcon.SetActive(true);
                sprite = thinCover;
                break;
            case CoverType.None:
                CoverIcon.SetActive(false);
                break;
        }

    }

    private void OnDestroy()
    {
        Unit.OnCoverTypeChanged -= UnitActionSystem_OnSelectedUnitChange;
    }
}
