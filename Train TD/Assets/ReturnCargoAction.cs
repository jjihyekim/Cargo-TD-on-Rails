using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnCargoAction : ModuleAction, IActiveDuringShopping {

	public MiniGUI_StarterBuildingButton myButton;
	
	protected override void _EngageAction() {
		myButton.ReturnCargo(this);
	}

	public void ActivateForShopping() {
		this.enabled = true;
	}

	public void Disable() {
		this.enabled = false;
	}
}
