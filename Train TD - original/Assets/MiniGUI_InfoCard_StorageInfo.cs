using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class MiniGUI_InfoCard_StorageInfo : MonoBehaviour, IBuildingInfoCard {

    [ReadOnly] public ModuleStorage storageModule;

    public void SetUp(Cart building) {
        storageModule = building.GetComponentInChildren<ModuleStorage>();

        if (storageModule == null) {
            gameObject.SetActive(false);
            return;
        }else{
            gameObject.SetActive(true);
        }
    }
}
