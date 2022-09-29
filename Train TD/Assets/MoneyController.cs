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

    public int scraps = 50;

    public void UpdateBasedOnLevelData() {
        scraps = DataSaver.s.GetCurrentSave().currentRun.scraps;
    }


    private void Update() {
        if (SceneLoader.s.isLevelInProgress) {
            /*perSecondMoneyCounter += moneyPerSecond * Time.deltaTime;

            if (perSecondMoneyCounter > 0) {
                var addition = Mathf.FloorToInt(perSecondMoneyCounter);
                scraps += addition;
                perSecondMoneyCounter -= addition;
            }
            */

            moneyText.text = scraps.ToString();
        }
    }


    public void SubtractScraps(int amount) {
        scraps -= amount;
    }

    public void AddScraps(int amount) {
        scraps += amount;
    }
}
