using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_CartReward : MonoBehaviour
{
    public TMP_Text amount;
    public int cartCount;
	
    public void SetUpReward(int _cartCount) {
        cartCount = _cartCount;
        amount.text = $"Get {cartCount} carts";
    }


    public void GetRewards() {
        DataSaver.s.GetCurrentSave().currentRun.myTrain.myCarts.Add(new DataSaver.TrainState.CartState());
        Train.s.DrawTrain(DataSaver.s.GetCurrentSave().currentRun.myTrain);
        DataSaver.s.SaveActiveGame();
        Destroy(gameObject);
    }
}
