using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class MiniGUI_InfoCard_RepairableMoveable : MonoBehaviour, IBuildingInfoCard {

    public Toggle repairable;
    public Toggle fragile;
    public Toggle moveable;
    
    public void SetUp(TrainBuilding building) {
        repairable.isOn = building.GetComponent<RepairAction>() != null;
        fragile.isOn = building.GetComponent<RepairableIfDestroyed>() == null;
        moveable.isOn = building.GetComponent<MoveModuleAction>() != null;
    }
}
