using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class MiniGUI_InfoCard_OverloadInfo : MonoBehaviour, IBuildingInfoCard {

    [ReadOnly] public EngineModule engineModule;

    public void SetUp(TrainBuilding building) {
        engineModule = building.GetComponent<EngineModule>();

        if (engineModule == null) {
            gameObject.SetActive(false);
            return;
        }else{
            gameObject.SetActive(true);
        }
    }
}
