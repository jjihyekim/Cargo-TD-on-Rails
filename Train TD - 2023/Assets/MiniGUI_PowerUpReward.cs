using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MiniGUI_PowerUpReward : MonoBehaviour
{
    public TMP_Text powerUpName;
    public Image powerUpIcon;
    private int rewardIndex;

    private PowerUpScriptable myPowerUp;
    public void SetUpReward(PowerUpScriptable powerUpScriptable, int _rewardIndex) {
        myPowerUp = powerUpScriptable;
        rewardIndex = _rewardIndex;
        powerUpName.text = myPowerUp.name;
        GetComponent<UITooltipDisplayer>().myTooltip.text = myPowerUp.description;
        powerUpIcon.sprite = myPowerUp.icon;
    }


    public void GetRewards() {
        MissionWinFinisher.s.ClearRewardWithIndex(rewardIndex);
        PlayerActionsController.s.GetPowerUp(myPowerUp);
        Destroy(gameObject);
    }
}
