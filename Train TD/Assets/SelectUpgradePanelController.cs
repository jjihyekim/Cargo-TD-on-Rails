using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectUpgradePanelController : MonoBehaviour {
    public static SelectUpgradePanelController s;

    private MiniGUI_UpgradeReward source;
    public GameObject panel;

    private void Awake() {
        s = this;

        HidePanel();
    }

    public UpgradeInfoScreenController[] upgradeInfoScreens;
    
    public void OpenUpgradeSelections(MiniGUI_UpgradeReward _source) {
        source = _source;

        for (int i = 0; i < source.upgrades.Length; i++) {
            upgradeInfoScreens[i].ChangeSelectedUpgrade(source.upgrades[i]);
            upgradeInfoScreens[i].gameObject.SetActive(true);
        }
        
        panel.SetActive(true);
    }
    
    public void GetUpgrade(Upgrade toGet) {
        UpgradesController.s.GetUpgrade(toGet);
        source.RewardGotten();
        source = null;
        HidePanel();
    }

    public void HidePanel() {
        for (int i = 0; i < upgradeInfoScreens.Length; i++) {
            upgradeInfoScreens[i].gameObject.SetActive(false);
        }

        panel.SetActive(false);

        if (source != null)
            source.RewardDismissed();
    }
}
