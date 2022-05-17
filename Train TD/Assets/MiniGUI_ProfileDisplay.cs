using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class MiniGUI_ProfileDisplay : MonoBehaviour {

    public TMP_Text starText;
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
        moneyText.text = mySave.money.ToString();
        starText.text = mySave.reputation.ToString();
        profileName.text = mySave.saveName;
        playTime.text = SpeedController.GetNiceTime(mySave.playtime);
    }

    public void SetStats(DataSaver.SaveFile save) {
        mySave = save;
        
    }
}
