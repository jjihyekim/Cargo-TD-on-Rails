using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleRangeBoostAction : ModuleAction {
	
	
	[Space] public float actionTime = 10f;
	public float rangeBoost = 3f;
	public float rotateSpeedBoost = 2f;
	public float angleBoost = 115;

	protected override void _EngageAction() {
		GetComponent<TargetPicker>().range += rangeBoost;
		GetComponent<TargetPicker>().rotationSpan += angleBoost;
		GetComponent<GunModule>().rotateSpeed *= rotateSpeedBoost;

		Invoke(nameof(StopAction), actionTime);
	}

	void StopAction() {
		GetComponent<TargetPicker>().range -= rangeBoost;
		GetComponent<TargetPicker>().rotationSpan -= angleBoost;
		GetComponent<GunModule>().fireDelay /= rotateSpeedBoost;
	}
}
