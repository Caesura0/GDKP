using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;


public class MercUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mercName;
    [SerializeField] TextMeshProUGUI mercPrice;
    [SerializeField] TextMeshProUGUI mercDescription;
    [SerializeField] Button HireButton;
    MercUnit merc;
    //UISoundEffects effects;

    public event EventHandler<MercUnit> OnUnitHired;
    public static event EventHandler onUnitChanged;



    private void Start()
    {
        gameObject.SetActive(false);
        //effects = FindObjectOfType<UISoundEffects>();
    }

    public void SetNextMerc(string mercName, string mercPrice, string mercDescription, MercUnit merc)
    {
        //effects.PlayPageTurn2();
        gameObject.SetActive(true);
        this.mercName.text = mercName;
        this.mercDescription.text = mercDescription;
        onUnitChanged?.Invoke(this, EventArgs.Empty);
        if (merc.GetIsHired())
        {
            this.mercPrice.text = "HIRED";
            HireButton.interactable = false;
        }
        else
        {
            this.mercPrice.text = "Price: " + mercPrice;
            HireButton.interactable = true;
        }



        this.merc = merc;
    }

    public void HireMerc()
    {
        merc.HireMerc();
        mercPrice.text = "HIRED";
        OnUnitHired?.Invoke(this, merc);
        HireButton.interactable = false; 
    }
}
