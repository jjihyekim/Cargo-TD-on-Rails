using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour/*, IPointerEnterHandler, IPointerExitHandler*/ {
	public bool isFrontSlot = false;
	
	public TrainBuilding[] myBuildings = new TrainBuilding[3];
	public bool isCompletelyEmpty() {
		var isAllSlotsEmpty = true;
		for (int i = 0; i < myBuildings.Length; i++) {
			if (myBuildings[i] != null) {
				isAllSlotsEmpty = false;
				break;
			}
		}

		return isAllSlotsEmpty;
	}

	int DistanceFromFront() {
		return (GetComponentInParent<Cart>().index * 2) + (isFrontSlot ? 0 : 1);
	}
	
	public float DistancePercent() {
		var floatPercent = (float)DistanceFromFront() / (float)(GetComponentInParent<Train>().cartCount * 2);
		//var realPercent = Mathf.RoundToInt(floatPercent * 20)*5;
		return  floatPercent;
	}
	

	public bool CanBuiltInSlot(TrainBuilding building, int slot) {
		if (building.occupiesEntireSlot) {
			var isAllEmpty = true;
			for (int i = 0; i < myBuildings.Length; i++) {
				if (myBuildings[i] != null)
					isAllEmpty = false;
			}

			return isAllEmpty;
		} else {
			return myBuildings[slot] == null;
		}
	}

	public void AddBuilding(TrainBuilding building, int slot) {
		if (building.occupiesEntireSlot) {
			for (int i = 0; i < myBuildings.Length; i++) {
				myBuildings[i] = building;
			}
			building.mySlotIndex = 1;
		} else {
			myBuildings[slot] = building;
			myBuildings[slot].mySlotIndex = slot;
		}

		myBuildings[slot].transform.SetParent(transform);
		myBuildings[slot].transform.localPosition = Vector3.zero;
	}

	public void RemoveBuilding(TrainBuilding building) {
		for (int i = 0; i < myBuildings.Length; i++) {
			if (myBuildings[i] == building) {
				myBuildings[i] = null;
			}
		}
	}
}
