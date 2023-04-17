using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_SingleBuildingReward : MonoBehaviour
{
    public TMP_Text powerUpName;
    public Image powerUpIcon;
    private int rewardIndex;

    private string myBuilding;
    public void SetUpReward(string buildingName, int _rewardIndex) {
        myBuilding = buildingName;
        rewardIndex = _rewardIndex;
        var buildingScript = DataHolder.s.GetBuilding(myBuilding);
        powerUpName.text = buildingScript.displayName;
        GetComponent<UITooltipDisplayer>().myTooltip.text = buildingScript.GetComponent<ClickableEntityInfo>().tooltip.text;
        powerUpIcon.sprite = buildingScript.Icon;
    }


    public void GetRewards() {
        MissionWinFinisher.s.ClearRewardWithIndex(rewardIndex);
        UpgradesController.s.GetBuilding(myBuilding);
        Destroy(gameObject);
    }
}
