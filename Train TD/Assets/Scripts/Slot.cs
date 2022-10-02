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

	public bool isSlotNeedToBeCut() {
		for (int i = 0; i < myBuildings.Length; i++) {
			if (myBuildings[i] != null) {
				if (myBuildings[i].occupiesEntireSlot) {
					return true;
				}
			}
		}

		return false;
	}


	int DistanceFromFront() {
		return (GetComponentInParent<Cart>().index * 2) + (isFrontSlot ? 0 : 1);
	}
	
	public float DistancePercent() {
		var floatPercent = (float)DistanceFromFront() / (float)(GetComponentInParent<Train>().cartCount * 2);
		//var realPercent = Mathf.RoundToInt(floatPercent * 20)*5;
		return  floatPercent;
	}
	

	public bool CanBuiltInSlot(TrainBuilding building, ref int slot) {
		if (building.occupiesEntireSlot) {
			var isAllEmpty = true;
			for (int i = 0; i < myBuildings.Length; i++) {
				if (myBuildings[i] != null)
					isAllEmpty = false;
			}

			return isAllEmpty;
		} else {
			if (slot == 0 && !building.canPointUp) {
					Debug.LogError($"Wrong building slot provided given:{slot} correct:{1} due to {building.uniqueName} cannot point up");
				slot = 1;
			}else if (slot != 0 && !building.canPointSide) {
					Debug.LogError($"Wrong building slot provided given:{slot} correct:{0} due to {building.uniqueName} cannot point sides");
				slot = 0;
			}
			
			if (slot >= 0 && slot <= myBuildings.Length) {
				return myBuildings[slot] == null;
			} else {
				return false;
			}
		}
	}

	public void AddBuilding(TrainBuilding building, int slot, bool fixSlot = false) {
		building.mySlot = this;
		if (building.occupiesEntireSlot) {
			for (int i = 0; i < myBuildings.Length; i++) {
				myBuildings[i] = building;
			}
			if(slot != 0)
				if(!fixSlot)
					Debug.LogError($"Wrong building slot provided given:{slot} correct:{0} due to {building.uniqueName} entire slot occupation");
			slot = 0;

			myBuildings[0].SetRotationBasedOnIndex(0, isFrontSlot);
		} else {
			if (slot == 0 && !building.canPointUp) {
				if(!fixSlot)
					Debug.LogError($"Wrong building slot provided given:{slot} correct:{1} due to {building.uniqueName} cannot point up");
				slot = 1;
			}else if (slot != 0 && !building.canPointSide) {
				if(!fixSlot)
					Debug.LogError($"Wrong building slot provided given:{slot} correct:{0} due to {building.uniqueName} cannot point sides");
				slot = 0;
			}
			
			if(myBuildings[slot] != null)
				if(!fixSlot)
					Debug.LogError($"Slot {slot} is already full but trying to put {building.uniqueName}");

			myBuildings[slot] = building;
			myBuildings[slot].SetRotationBasedOnIndex(slot, isFrontSlot);
		}

		myBuildings[slot].transform.SetParent(transform);
		myBuildings[slot].transform.localPosition = Vector3.zero;
		
		GetComponentInParent<Cart>().SlotsAreUpdated();
		if(GetComponentInParent<Train>())
			GetComponentInParent<Train>().trainWeight += building.weight;
	}

	public void RemoveBuilding(TrainBuilding building) {
		for (int i = 0; i < myBuildings.Length; i++) {
			if (myBuildings[i] == building) {
				myBuildings[i] = null;
			}
		}
		
		building.mySlot = null;
		
		GetComponentInParent<Cart>().SlotsAreUpdated();
		if(GetComponentInParent<Train>())
			GetComponentInParent<Train>().trainWeight -= building.weight;
	}
}
