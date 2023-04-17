using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectTrainBuildingPanelController : MonoBehaviour {
    public static SelectTrainBuildingPanelController s;

    private MiniGUI_BuildingReward source;
    public GameObject panel;

    private void Awake() {
        s = this;

        HidePanel();
    }

    public BuildingInfoScreenController[] upgradeInfoScreens;
    
    public void OpenUpgradeSelections(MiniGUI_BuildingReward _source) {
        source = _source;

        for (int i = 0; i < source.buildingUniqueNames.Length; i++) {
            upgradeInfoScreens[i].ChangeSelectedUpgrade(source.buildingUniqueNames[i]);
            upgradeInfoScreens[i].gameObject.SetActive(true);
        }
        
        panel.SetActive(true);
    }
    
    public void GetBuilding(string buildingUniqueName) {
        UpgradesController.s.GetBuilding(buildingUniqueName);
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
