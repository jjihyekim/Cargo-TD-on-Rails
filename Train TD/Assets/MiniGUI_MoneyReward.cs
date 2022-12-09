using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_MoneyReward : MonoBehaviour
{
    public TMP_Text amount;
    public int money;
    private int rewardIndex;
    
    public void SetUpReward(int _money, int _rewardIndex) {
        money = _money;
        rewardIndex = _rewardIndex;
        amount.text = $"{money} Moniez";
    }


    public void GetRewards() {
        MoneyController.s.ModifyResource(ResourceTypes.money, money);

        MissionWinFinisher.s.ClearRewardWithIndex(rewardIndex);
        Destroy(gameObject);
    }
}
