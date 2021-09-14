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
        icon.sprite = myCargo.GetComponent<TrainBuilding>().Icon;
        cargoMoney.text = myCargo.moneyReward.ToString();
        return myCargo.moneyReward;
    }
}
