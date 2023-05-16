using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class MiniGUI_InfoCard_RepairableMoveable : MonoBehaviour, IBuildingInfoCard {

    public Toggle repairable;
    public Toggle fragile;
    
    public void SetUp(Cart building) {
        //repairable.isOn = building.GetComponent<RepairAction>() != null;
        //fragile.isOn = building.GetComponentInChildren<RepairableIfDestroyed>() == null;
    }
}
