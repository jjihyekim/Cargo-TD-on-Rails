using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_SingleInfo : MonoBehaviour {
    public ClickableEntityInfo myInfo;

    public TMP_Text myInfoText;
    
    public MiniGUI_SingleInfo SetUp(ClickableEntityInfo info) {
        myInfo = info;

        myInfoText.text = info.info;

        GetComponent<UITooltipDisplayer>().myTooltip = myInfo.tooltip;

        return this;
    }

    private void Update() {
        myInfoText.text = myInfo.info;
    }
}
