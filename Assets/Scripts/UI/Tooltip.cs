using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tooltip : MonoBehaviour
{

    public static Tooltip Instance {  get; private set; }

    [SerializeField] TextMeshProUGUI toolTipText;
    [SerializeField] RectTransform backgroundRectTransform;
    [SerializeField] RectTransform canvasRectTransform;

    RectTransform rectTransform;

    System.Func<string> getTooltipTextFunc;

    private void Awake()
    {
        SetText("Tooltip text practice");
        rectTransform = GetComponent<RectTransform>();
        if(Instance == null )
        {
            Instance = this;
        }
        HideTooltip();
    }


    private void Update()
    {

        //SetText(getTooltipTextFunc());

        Vector2 anchoredPosition  = Input.mousePosition / canvasRectTransform.localScale.x;
        if (anchoredPosition.x + backgroundRectTransform.rect.width > canvasRectTransform.rect.width)
        {
            //tooltip left screen on right side
            anchoredPosition.x = canvasRectTransform.rect.width - backgroundRectTransform.rect.width;
        }
        if (anchoredPosition.y + backgroundRectTransform.rect.height > canvasRectTransform.rect.height)
        {
            //tooltip left screen on top side
            anchoredPosition.y = canvasRectTransform.rect.height - backgroundRectTransform.rect.height;
        }


        rectTransform.anchoredPosition = anchoredPosition;
    }


    void SetText(string text)
    {
        toolTipText.text = text;
        toolTipText.ForceMeshUpdate();
        Vector2 textSize = toolTipText.GetRenderedValues();
        Vector2 padding = new Vector2(4f, 4f);
        backgroundRectTransform.sizeDelta = textSize + padding;
    }


    public void ShowTooltip(string tooltipText)
    {
        gameObject.SetActive(true);
        SetText(tooltipText);
    }


    void ShowTooltip(System.Func<string> getTooltipTextFunc)
    {
        this.getTooltipTextFunc = getTooltipTextFunc;
        gameObject.SetActive(true);
        SetText(getTooltipTextFunc());
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }



    public static void ShowTooltip_Static(string tooltipText)
    {
        Instance.ShowTooltip(tooltipText);
    }

    public static void ShowTooltip_Static(System.Func<string> getTooltipTextFunc)
    {
        Instance.ShowTooltip(getTooltipTextFunc);
    }

    public static void Hidetooltip_Static()
    {
        Instance.HideTooltip();
    }

}
