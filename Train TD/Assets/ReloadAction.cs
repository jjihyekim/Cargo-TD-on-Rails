using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadAction : ModuleAction, IActiveDuringCombat, IActiveDuringShopping {
	private int fullCost;
	private ModuleAmmo myMod;
	protected override void _Start() {
		fullCost = cost;
		myMod = GetComponent<ModuleAmmo>();
	}

	protected override void _EngageAction() {
		myMod.SetAmmo(myMod.maxAmmo);
	}

	protected override void _Update() {
		var percent = (1f - (float)myMod.curAmmo / myMod.maxAmmo);
		cost = (int)( percent * fullCost);

		canEngage = percent > 0.1f;
	}

	public void ActivateForCombat() {
		this.enabled = true;
	}

	public void ActivateForShopping() {
		this.enabled = true;
	}

	public void Disable() {
		this.enabled = false;
	}

}
