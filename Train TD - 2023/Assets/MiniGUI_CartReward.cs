using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_CartReward : MonoBehaviour
{
    public TMP_Text amount;
    public int cartCount;
    private int rewardIndex;
	
    public void SetUpReward(int _cartCount, int _rewardIndex) {
        cartCount = _cartCount;
        rewardIndex = _rewardIndex;
        amount.text = $"Get {cartCount} carts";
    }


    public void GetRewards() {
        DataSaver.s.GetCurrentSave().currentRun.myTrain.myCarts.Add(new DataSaver.TrainState.CartState());
        Train.s.DrawTrainBasedOnSaveData();
        DataSaver.s.SaveActiveGame();
        
        MissionWinFinisher.s.ClearRewardWithIndex(rewardIndex);
        Destroy(gameObject);
    }
}
