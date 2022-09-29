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

    public MiniGUI_ModuleSingleAction SetUp(ModuleAction myAction) {
        this.myAction = myAction;

        actionName.text = myAction.actionName;

        if (myAction.cost > 0) {
            cost.text = myAction.cost.ToString();
        } else {
            cost.text = $"+{-myAction.cost}";
            cost.color = DataHolder.s.moneyBackColor;
        }


        if (myAction.cooldown > 0) {
            cooldown.text = myAction.cooldown.ToString();
        } else {
            curCooldown.gameObject.SetActive(false);
            cooldown.text = "0";
            myButton.interactable = true;
        }

        return this;
    }


    private void Update() {
        if (myAction.cooldown > 0) {
            curCooldown.value = 1f - (myAction.curCooldown / myAction.cooldown);
            
            myButton.interactable = myAction.curCooldown <= 0;
        }

        if (myAction.cost > 0) {
            cost.text = myAction.cost.ToString();
        } else {
            cost.text = $"+{-myAction.cost}";
        }
    }


    public void TryEngage() {
        myAction.EngageAction();
    }
}
