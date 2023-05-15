using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class MiniGUI_InfoCard_RoboRepair : MonoBehaviour, IBuildingInfoCard {

    [ReadOnly] public RoboRepairModule roboRepairModule;

    public void SetUp(Cart building) {
        roboRepairModule = building.GetComponentInChildren<RoboRepairModule>();

        if (roboRepairModule == null) {
            gameObject.SetActive(false);
            return;
        }else{
            gameObject.SetActive(true);
        }
    }
}

