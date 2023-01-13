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

    public GameObject ownedBox;
    public Image icon;
    public Image bg;

    public Image selectedOverlay;

    public Color notBoughtColor = Color.grey;
    public Color boughtColor = Color.white;


    public void Initialize() {
        selectedOverlay.enabled = false;
        SetUp(myUpgrade);
    }

    public void SetUp(Upgrade upgrade) {
        myUpgrade = upgrade;
        Refresh();
    }
    
    public void Refresh() {
        if (myUpgrade.isUnlocked) {
            ownedBox.SetActive(true);
            bg.color = boughtColor;

        } else {
            ownedBox.SetActive(false);
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
}
