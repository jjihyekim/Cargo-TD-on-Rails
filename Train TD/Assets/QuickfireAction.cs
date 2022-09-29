using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickfireAction : ModuleAction, IActiveDuringCombat {

	[Space]
	public float actionTime = 10f;
	public float initialDelay = 2f;
	public float endDelay = 2f;
	
	public float fireSpeedBoost = 10f;
	protected override void _EngageAction() {
		GetComponent<GunModule>().fireDelay /= fireSpeedBoost;
		GetComponent<GunModule>().DeactivateGun();
		
		
		Invoke(nameof(StartShooting), initialDelay);
		
		Invoke(nameof(StopAction), initialDelay+actionTime);
	}

	void StartShooting() {
		var gun = GetComponent<GunModule>();
		GetComponent<GunModule>().ActivateGun();
	}

	void StopAction() {
		GetComponent<GunModule>().fireDelay *= fireSpeedBoost;
		GetComponent<GunModule>().DeactivateGun();
		Invoke(nameof(ResumeShooting), endDelay);
	}

	void ResumeShooting() {
		GetComponent<GunModule>().ActivateGun();
	}
	
	public void ActivateForCombat() {
		this.enabled = true;
	}

	public void Disable() {
		this.enabled = false;
	}
}
