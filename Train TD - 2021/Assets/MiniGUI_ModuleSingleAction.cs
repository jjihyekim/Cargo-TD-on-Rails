using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_ModuleSingleAction : MonoBehaviour {

    public ModuleAction myAction;

    public TMP_Text cost;

    public TMP_Text actionName;
    public TMP_Text cooldown;
    public Slider curCooldown;
    public Button myButton;
    
    public Image icon;
	
    public Sprite moneyCost;
    public Sprite scrapCost;
    public Sprite ammoCost;
    public Sprite fuelCost;
    
    public Color regularCostColor = Color.black;
    public Color cantAffordColor = Color.red;

    public MiniGUI_ModuleSingleAction SetUp(ModuleAction myAction) {
        this.myAction = myAction;

        actionName.text = myAction.actionName;

        if (myAction.cost > 0) {
            cost.text = myAction.cost.ToString();
        } else {
            cost.text = $"+{-myAction.cost}";
            cost.color = DataHolder.s.moneyBackColor;
        }

        switch (myAction.myType) {
            case ResourceTypes.scraps:
                icon.sprite = scrapCost;
                break;
            case ResourceTypes.money:
                icon.sprite = moneyCost;
                break;
            case ResourceTypes.ammo:
                icon.sprite = ammoCost;
                break;
            case ResourceTypes.fuel:
                icon.sprite = fuelCost;
                break;
            default:
                Debug.LogError($"Action type {myAction.myType} not implemented");
            break;
        }


        if (myAction.cooldown > 0) {
            cooldown.text = myAction.cooldown.ToString();
        } else {
            curCooldown.gameObject.SetActive(false);
            cooldown.text = "0";
            myButton.interactable = true;
        }

        var tooltip = myAction.myTooltip;
        GetComponent<UITooltipDisplayer>().myTooltip = tooltip;

        return this;
    }


    private void Update() {
        if (myAction.cooldown > 0) {
            curCooldown.value = 1f - (myAction.curCooldown / myAction.cooldown);
        }

        if (myAction.cost > 0) {
            cost.text = myAction.cost.ToString();
        } else {
            cost.text = $"+{-myAction.cost}";
        }
        
        
        myButton.interactable = myAction.canEngage && myAction.canAfford && myAction.isCooldownOver;

        curCooldown.gameObject.SetActive(myAction.canEngage && myAction.canAfford);

        if (myAction.canAfford) {
            cost.color = regularCostColor;
        } else {
            cost.color = cantAffordColor;
        }
    }


    public void TryEngage() {
        myAction.EngageAction();
    }
}
