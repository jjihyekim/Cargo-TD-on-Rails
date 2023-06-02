using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class MiniGUI_InfoCard_ExtraEffect : MonoBehaviour, IBuildingInfoCard {

    public TMP_Text myText;

    [ReadOnly] public IExtraInfo extraInfo;
    public void SetUp(Cart building) {
        extraInfo = building.GetComponentInChildren<IExtraInfo>();
        
        if (extraInfo == null) {
            gameObject.SetActive(false);
            return;
        }else{
            gameObject.SetActive(true);
        }

        myText.text = extraInfo.GetInfoText();
    }
}


public interface IExtraInfo {
    public string GetInfoText();
}