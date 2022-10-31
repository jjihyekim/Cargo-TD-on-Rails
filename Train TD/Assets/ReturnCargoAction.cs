using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnCargoAction : ModuleAction, IActiveDuringShopping {

	public MiniGUI_StarterBuildingButton myButton;
	
	protected override void _EngageAction() {
		myButton.ReturnCargo(GetComponent<TrainBuilding>());
	}

	public void ActivateForShopping() {
		if(myButton != null)
			this.enabled = true;
		else 
			this.enabled = false;
		
	}

	public void Disable() {
		this.enabled = false;
	}
}
