using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActionUIHolder : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI accuracyText;
    [SerializeField] TextMeshProUGUI attackText;
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] GameObject actionUIPanel;

    public Button confirmButton;
    public Button cancelButton;
    public Button reloadButton;



    public void SetAttackText(string text)
    {
        attackText.text = text;
    }
    public void SetAccuracyText(string text)
    {
        accuracyText.text = text;
    }
    public void SetAmmoText(string text)
    {
        ammoText.text = text;
    }

    public void ShowActionPanel()
    {
        actionUIPanel.SetActive(true);
    }

    public void HideActionPanel()
    {
        actionUIPanel.SetActive(false);
    }
}
