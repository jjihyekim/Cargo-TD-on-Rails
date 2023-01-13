using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour/*, IPointerEnterHandler, IPointerExitHandler*/ {
	public bool isFrontSlot = false;
	
	public TrainBuilding[] myBuildings = new TrainBuilding[4];
	
	
	[Tooltip("0 - forward, 1 - backwards, 2 - left, 3 - right, 4 - all slot")]
	public Transform[] myUITargetTransforms;
	public Transform[] myUIBottomTargetTransforms;
	public Transform GetUITargetTransform (int slotIndex, bool isBottom) {
		if (isBottom)
			return myUIBottomTargetTransforms[slotIndex];
		else
			return myUITargetTransforms[slotIndex];
	}
	
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
			if (slot <= 1 && !building.canPointUp) {
				slot = 2;
			}else if (slot >1 && !building.canPointSide) {
				slot = 0;
			}

			switch (slot) {
				case 0:
					case 1:
					// top buildings occupy a "single" slot but can be rotated, hence occupying both
					return myBuildings[0] == null && myBuildings[1] == null;
					break;
				case 2:
					case 3:
					return myBuildings[slot] == null;
					break;
				default:
					Debug.LogError($"Illegal slot {slot}");
					return false;
					break;
			}
		}
	}

	public void AddBuilding(TrainBuilding building, int slot, bool fixSlot = false) {
		building.mySlot = this;
		if (building.occupiesEntireSlot) {
			for (int i = 0; i < myBuildings.Length; i++) {
				myBuildings[i] = building;
			}
			
			slot = 0;

			myBuildings[0].SetRotationBasedOnIndex(0, isFrontSlot);
		} else {
			if (slot <= 1 && !building.canPointUp) {
				slot = 2;
			}else if (slot >1 && !building.canPointSide) {
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
		if (GetComponentInParent<Train>())
			GetComponentInParent<Train>().trainWeightDirty = true;
	}

	public void RemoveBuilding(TrainBuilding building) {
		for (int i = 0; i < myBuildings.Length; i++) {
			if (myBuildings[i] == building) {
				myBuildings[i] = null;
			}
		}
		
		building.mySlot = null;
		
		if(GetComponentInParent<Cart>())
			GetComponentInParent<Cart>().SlotsAreUpdated();
		var train = GetComponentInParent<Train>();
		if (train != null) {
			train.trainWeightDirty = true;
			train.trainUpdatedThroughNonBuildingActions?.Invoke();
		}
	}
}
