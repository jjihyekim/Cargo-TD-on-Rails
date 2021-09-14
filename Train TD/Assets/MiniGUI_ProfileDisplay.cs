using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class MiniGUI_ProfileDisplay : MonoBehaviour {

    public TMP_Text starText;
    public TMP_Text moneyText;
    public TMP_Text profileName;

    public bool autoSetOnStart = true;

    void Start() {
        if (autoSetOnStart) {
            SetStats(DataSaver.s.GetCurrentSave());
        }
    }


    public void SetStats(DataSaver.SaveFile save) {
        moneyText.text = save.money.ToString();
        starText.text = save.reputation.ToString();
        profileName.text = save.saveName;
    }
}
