using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_BuildingReward : MonoBehaviour
{
    public string[] buildingUniqueNames;
    private int rewardIndex;
	
    public void SetUpReward(string[] _buildingUniqueNames, int _rewardIndex) {
        buildingUniqueNames = _buildingUniqueNames;
        rewardIndex = _rewardIndex;
    }
    
    public void GetRewards() {
        SelectTrainBuildingPanelController.s.OpenUpgradeSelections(this);
    }

    public void RewardGotten() {
        MissionWinFinisher.s.ClearRewardWithIndex(rewardIndex);
        Destroy(gameObject);
    }

    public void RewardDismissed() {
        //do nothing
    }
}
