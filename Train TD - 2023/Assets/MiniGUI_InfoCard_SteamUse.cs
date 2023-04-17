using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class MiniGUI_InfoCard_SteamUse : MonoBehaviour, IBuildingInfoCard {

    [ReadOnly] public EngineModule engineModule;
    [ReadOnly] public GunModule gunModule;
    [ReadOnly] public RoboRepairModule roboRepairModule;

    public void SetUp(TrainBuilding building) {
        engineModule = building.GetComponent<EngineModule>();
        gunModule = building.GetComponent<GunModule>();
        roboRepairModule = building.GetComponent<RoboRepairModule>();

        if (!(engineModule != null || gunModule != null || roboRepairModule != null)) {
            gameObject.SetActive(false);
            return;
        }else{
            gameObject.SetActive(true);
        }
    }
}
