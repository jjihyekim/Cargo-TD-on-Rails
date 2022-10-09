using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_MoneyReward : MonoBehaviour
{
    public TMP_Text amount;
    public int money;
	
    public void SetUpReward(int _money) {
        money = _money;
        amount.text = $"{money} Moniez";
    }


    public void GetRewards() {
        DataSaver.s.GetCurrentSave().currentRun.myResources.money += money;
        DataSaver.s.SaveActiveGame();
        Destroy(gameObject);
    }
}
