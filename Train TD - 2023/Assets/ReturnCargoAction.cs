using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnCargoAction : ModuleAction, IActiveDuringShopping {

	public MiniGUI_StarterBuildingButton myButton;
	
	protected override void _EngageAction() {
		if (myButton != null) {
			myButton.ReturnCargo(GetComponent<TrainBuilding>());
		} else {
			Train.s.SaveTrainState();
			Destroy(gameObject);
		}
	}

	public void ActivateForShopping() {
		/*if(myButton != null)
			this.enabled = true;
		else 
			this.enabled = false;
			*/
		this.enabled = true;
	}

	public void Disable() {
		this.enabled = false;
	}
}
