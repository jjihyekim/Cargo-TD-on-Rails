using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_DeliveredCargo : MonoBehaviour {
    public CargoModule myCargo;
    public Image icon;
    public TMP_Text cargoMoney;

    public int SetUp(CargoModule data) {
        myCargo = data;
        if (myCargo.isBuildingReward) {
            icon.sprite = DataHolder.s.GetBuilding(myCargo.myReward).Icon;
        } else {
            icon.sprite = DataHolder.s.GetPowerUp(myCargo.myReward).icon;
        }
        //icon.sprite = myCargo.GetComponent<TrainBuilding>().Icon;
        
        var reward = myCargo.GetReward();
        
        cargoMoney.text = reward.ToString();

        return -1;
    }
}
