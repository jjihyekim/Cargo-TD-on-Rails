using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_UpgradeReward : MonoBehaviour
{
    public Upgrade[] upgrades;
	
    public void SetUpReward(Upgrade[] _upgrades) {
        upgrades = _upgrades;
    }
    
    public void GetRewards() {
        SelectUpgradePanelController.s.OpenUpgradeSelections(this);
    }

    public void RewardGotten() {
        Destroy(gameObject);
    }

    public void RewardDismissed() {
        //do nothing
    }
}
