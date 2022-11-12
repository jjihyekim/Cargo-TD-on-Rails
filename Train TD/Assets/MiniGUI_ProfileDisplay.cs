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
    public TMP_Text ammoText;
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
        if (mySave.isInARun) {
            moneyText.text = MoneyController.s.money.ToString();
            profileName.text = mySave.currentRun.character.uniqueName;
            playTime.text = SpeedController.GetNiceTime(mySave.currentRun.playtime);
            
            fuelText.text = $"{MoneyController.s.fuel:F0}/{MoneyController.s.maxFuel:F0}";
            scrapsText.text = $"{MoneyController.s.scraps:F0}/{MoneyController.s.maxScraps:F0}";
            ammoText.text = $"{MoneyController.s.ammo:F0}/{MoneyController.s.maxAmmo:F0}";
        }
    }

    public void SetStats(DataSaver.SaveFile save) {
        mySave = save;
    }
}
