using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineOverloadAction : ModuleAction, IActiveDuringCombat, IBoostAction
{
	
	[Space]
	public float actionTime = 60f;
	public float engineSpeedMultiplier = 2f;
	private int originalPower;
	protected override void _EngageAction() {

		originalPower = GetComponent<EngineModule>().enginePower;

		GetComponent<EngineModule>().enginePower = (int)(originalPower*engineSpeedMultiplier);
		
		
		GetComponent<EngineStopAction>().canEngage = false;
		
		Invoke(nameof(StopAction), actionTime);
		
		SetBoostStatus(true);
	}

	void StopAction() {
		GetComponent<EngineModule>().enginePower = originalPower;
		
		GetComponent<EngineStopAction>().canEngage = true;
		
		SetBoostStatus(false);
	}
	
	public void ActivateForCombat() {
		this.enabled = true;
	}

	public void Disable() {
		this.enabled = false;
	}
}
