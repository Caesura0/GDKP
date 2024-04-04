using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActionButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TextMeshProUGUI textmeshPro;
    [SerializeField] Button button;
    [SerializeField] GameObject selectedGameObject;
    [SerializeField] Tooltip tooltipPrefab;

    GameObject tooltip;

    BaseAction baseAction;


    public void OnPointerEnter(PointerEventData eventData)
    {
        Tooltip.Instance.ShowTooltip(baseAction.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Tooltip.Instance.HideTooltip();
    }

    public void SetBaseAction(BaseAction baseAction)
    {
        textmeshPro.text = baseAction.GetActionName().ToUpper();
        this.baseAction = baseAction;
        button.onClick.AddListener(() =>
        {
            UnitActionSystem.Instance.SetSelectedAction(baseAction);
        });
    }


    public void UpdateSelectedVisual()
    {
        BaseAction selectedBaseAction = UnitActionSystem.Instance.GetSelectedAction();
        selectedGameObject.SetActive(selectedBaseAction == baseAction);
    }


    private void OnDestroy()
    {
        ClearTooltip();
    }

    private void OnDisable()
    {
        ClearTooltip();
    }


    private void ClearTooltip()
    {
        if (tooltip)
        {
            Destroy(tooltip.gameObject);
        }
    }
}
