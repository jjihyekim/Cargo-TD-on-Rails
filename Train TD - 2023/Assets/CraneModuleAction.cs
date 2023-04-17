using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraneModuleAction : ModuleAction, IActiveDuringCombat {
	//public float constructionTime = 2f;

	public GameObject cartBuildPrefab;
	protected override void _EngageAction() {
		var myBuilding = GetComponent<TrainBuilding>();
		var mySlot = myBuilding.mySlot;
		var myCart = myBuilding.GetComponentInParent<Cart>();

		var index = myCart.index;

		if (!mySlot.isFrontSlot) {
			index += 1;
		} else {
			//index -= 1;
			// when we insert at the same index it pushes the existing item forward
		}

		var resultCart = GetComponentInParent<Train>().AddTrainCartAtIndex(index);

		Instantiate(cartBuildPrefab, resultCart.transform.position, Quaternion.identity);
	}
	
	public void ActivateForCombat() {
		this.enabled = true;
	}

	public void Disable() {
		this.enabled = false;
	}
}
