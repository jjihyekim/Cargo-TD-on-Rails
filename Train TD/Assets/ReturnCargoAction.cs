using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnCargoAction : ModuleAction {

	public MiniGUI_StarterBuildingButton myButton;
	
	protected override void _EngageAction() {
		myButton.ReturnCargo(this);
	}
}
