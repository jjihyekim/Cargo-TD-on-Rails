using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_UpgradeButton : MonoBehaviour {

    private Upgrade myUpgrade;

    public Button upgradeButton;

    public TMP_Text upgradeCost;
    
    
    void Start() {
        myUpgrade = GetComponent<Upgrade>();
        if (myUpgrade == null) {
            throw new Exception("Upgrade on this component is missing!");
        }

        upgradeCost.text = myUpgrade.cost.ToString();
    }
}
