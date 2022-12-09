using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_UpgradeReward : MonoBehaviour
{
    public Upgrade[] upgrades;
    private int rewardIndex;
	
    public void SetUpReward(Upgrade[] _upgrades, int _rewardIndex) {
        upgrades = _upgrades;
        rewardIndex = _rewardIndex;
    }
    
    public void GetRewards() {
        SelectUpgradePanelController.s.OpenUpgradeSelections(this);
    }

    public void RewardGotten() {
        MissionWinFinisher.s.ClearRewardWithIndex(rewardIndex);
        Destroy(gameObject);
    }

    public void RewardDismissed() {
        //do nothing
    }
}
