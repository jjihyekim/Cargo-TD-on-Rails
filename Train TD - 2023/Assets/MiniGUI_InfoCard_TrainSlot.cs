using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_InfoCard_TrainSlot : MonoBehaviour, IBuildingInfoCard {

	public Toggle sidePlacement;
	public Toggle topPlacement;
	public Toggle allSlot;
	public void SetUp(TrainBuilding building) {
		if (building.occupiesEntireSlot) {
			allSlot.isOn = true;
			topPlacement.isOn = false;
			sidePlacement.isOn = false;
		} else {
			allSlot.isOn = false;
			topPlacement.isOn = building.canPointUp;
			sidePlacement.isOn = building.canPointSide;
		}
	}
}
