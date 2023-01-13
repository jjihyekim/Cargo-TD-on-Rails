using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickfireAction : ModuleActionTweakable, IActiveDuringCombat {

	protected override void _EngageAction() {
		GetComponent<GunModule>().fireDelay /= boost;
		GetComponent<GunModule>().DeactivateGun();
		
		
		Invoke(nameof(StartShooting), initialDelay);
		
		Invoke(nameof(StopAction), initialDelay+actionTime);
	}

	void StartShooting() {
		var gun = GetComponent<GunModule>();
		GetComponent<GunModule>().ActivateGun();
	}

	void StopAction() {
		GetComponent<GunModule>().fireDelay *= boost;
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
