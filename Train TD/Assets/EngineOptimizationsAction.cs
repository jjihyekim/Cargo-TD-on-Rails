using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineOptimizationsAction : ModuleAction {
	public float improvementMultiplier = 1.25f;
	protected override void _EngageAction() {
		//SpeedController.s.enginePowerBoost *= improvementMultiplier;
	}
}
