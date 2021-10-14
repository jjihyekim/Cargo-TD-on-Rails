using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickfireAction : ModuleAction {

	[Space]
	public float actionTime = 10f;
	public float fireSpeedBoost = 10f;
	protected override void _EngageAction() {

		GetComponent<GunModule>().fireDelay /= fireSpeedBoost;
		
		Invoke("StopAction", actionTime);
	}

	void StopAction() {
		
		GetComponent<GunModule>().fireDelay *= fireSpeedBoost;
	}
}
