using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_SingleInfo : MonoBehaviour {
    public IClickableInfo myInfo;

    public TMP_Text myInfoText;
    
    public MiniGUI_SingleInfo SetUp(IClickableInfo info) {
        myInfo = info;

        myInfoText.text = info.GetInfo();

        GetComponent<UITooltipDisplayer>().myTooltip = myInfo.GetTooltip();

        return this;
    }

    private void Update() {
        myInfoText.text = myInfo.GetInfo();
    }
}
