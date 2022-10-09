using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class MiniGUI_ProfileDisplay : MonoBehaviour {

    public TMP_Text fuelText;
    public TMP_Text scrapsText;
    public TMP_Text moneyText;
    public TMP_Text profileName;
    public TMP_Text playTime;

    public bool autoSetOnStart = true;

    private DataSaver.SaveFile mySave;
    void Start() {
        if (autoSetOnStart) {
            SetStats(DataSaver.s.GetCurrentSave());
        }
    }


    private void Update() {
        fuelText.text = $"{mySave.currentRun.myResources.fuel}/{mySave.currentRun.myResources.maxFuel}";
        moneyText.text = mySave.currentRun.myResources.money.ToString();
        scrapsText.text = mySave.currentRun.myResources.scraps.ToString();
        profileName.text = mySave.currentRun.character;
        playTime.text = SpeedController.GetNiceTime(mySave.currentRun.playtime);
    }

    public void SetStats(DataSaver.SaveFile save) {
        mySave = save;
    }
}
