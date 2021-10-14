using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_UpgradeButton : MonoBehaviour {

    [ReadOnly]
    public Upgrade myUpgrade;
    public Button upgradeButton;
    public TMP_Text upgradeCost;
    public Image icon;
    public Image bg;

    public Image selectedOverlay;

    public Color notBoughtColor = Color.grey;
    public Color boughtColor = Color.white;

    public GameObject starRequiredOverlay;
    public TMP_Text starRequirementAmount;


   public void Initialize() {
        selectedOverlay.enabled = false;
        if (myUpgrade != null) {
            SetUp(myUpgrade);
        }
    }

    public void SetUp(Upgrade upgrade) {
        myUpgrade = upgrade;
        Refresh();
    }
    
    public void Refresh() {
        var mySave = DataSaver.s.GetCurrentSave();

        if (mySave.reputation < myUpgrade.starRequirement) {
            starRequiredOverlay.SetActive(true);
            starRequirementAmount.text = myUpgrade.starRequirement.ToString();
        } else {
            starRequiredOverlay.SetActive(false);
        }
        
        
        if (myUpgrade.isUnlocked) {
            upgradeButton.interactable = false;
            upgradeCost.text = "bought";

            bg.color = boughtColor;

        } else {
            if (mySave.money >= myUpgrade.cost) {
                upgradeButton.interactable = true;
            } else {
                upgradeButton.interactable = false;
            }
            
            upgradeCost.text = myUpgrade.cost.ToString();
            bg.color = notBoughtColor;
        }

        icon.sprite = myUpgrade.icon;
    }

    public void Select() {
        UpgradesController.s.ChangeSelectedUpgrade(myUpgrade);
    }
    
    public void SetSelectionStatus(bool isSelected) {
        selectedOverlay.enabled = isSelected;
    }


    public void Buy() {
        UpgradesController.s.BuyUpgrade(myUpgrade);
    }
}
