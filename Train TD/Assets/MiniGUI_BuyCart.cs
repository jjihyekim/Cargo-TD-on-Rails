using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_BuyCart : MonoBehaviour
{
    public int moneyCost = 300;

    public Button myButton;
    public TMP_Text costText;

    public void Buy() {
        MoneyController.s.ModifyResource(ResourceTypes.money, -moneyCost);
        DataSaver.s.GetCurrentSave().currentRun.myTrain.myCarts.Add(new DataSaver.TrainState.CartState());
        Train.s.DrawTrain(DataSaver.s.GetCurrentSave().currentRun.myTrain);
        DataSaver.s.SaveActiveGame();
        Destroy(gameObject);
    }


    private DataSaver.RunState curRun;

    public GameObject optionScreen;

    private void Start() {
        if (DataSaver.s.GetCurrentSave().isInARun) {
            curRun = DataSaver.s.GetCurrentSave().currentRun;
            costText.text = moneyCost.ToString();

            optionScreen.SetActive((curRun.map.GetPlayerStar().city.canBuyCart));
        }
    }

    private void Update() {
        if (DataSaver.s.GetCurrentSave().isInARun && curRun != null && myButton != null) {
            myButton.interactable = MoneyController.s.HasResource(ResourceTypes.money, moneyCost);
        }
    }
}
