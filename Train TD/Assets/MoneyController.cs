using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyController : MonoBehaviour {
    public static MoneyController s;

    private void Awake() {
        s = this;
    }


    public TMP_Text moneyText;

    public int money = 50;

    public float moneyPerSecond = 0.5f;
    public float perSecondMoneyCounter = 0;

    public void UpdateBasedOnLevelData() {
        money = SceneLoader.s.currentLevel.startingMoney;
        moneyPerSecond = SceneLoader.s.currentLevel.moneyGainSpeed;
    }


    private void Update() {
        if (SceneLoader.s.isLevelInProgress) {
            perSecondMoneyCounter += moneyPerSecond * Time.deltaTime;

            if (perSecondMoneyCounter > 0) {
                var addition = Mathf.FloorToInt(perSecondMoneyCounter);
                money += addition;
                perSecondMoneyCounter -= addition;
            }

            moneyText.text = money.ToString();
        }
    }


    public void SubtractMoney(int amount) {
        money -= amount;
    }

    public void AddMoney(int amount) {
        money += amount;
    }
}
