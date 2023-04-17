using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleRotateAction : ModuleAction {

	[Space]
	public bool isClockwise = true;
	protected override void _EngageAction() {
		foreach (var rotateAction in GetComponents<ModuleRotateAction>()) {
			if (rotateAction != this) {
				rotateAction.curCooldown = cooldown;
			}
		}

		GetComponent<TrainBuilding>().TargetingAreaRotate(isClockwise);
	}

}
