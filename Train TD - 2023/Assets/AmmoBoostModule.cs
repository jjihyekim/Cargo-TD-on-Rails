using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBoostModule : ActivateWhenAttachedToTrain, IExtraInfo {
	public float ammoBoost = 1;
	
	protected override void _AttachedToTrain() {
		ApplyBoost(Train.s.GetNextBuilding(true, GetComponentInParent<Cart>()), true);
		ApplyBoost(Train.s.GetNextBuilding(false, GetComponentInParent<Cart>()), true);
	}

	protected override bool CanApply(Cart target) {
		var ammo = target.GetComponentInChildren<ModuleAmmo>();
		return ammo != null;
	}

	protected override void _ApplyBoost(Cart target, bool doApply) {
		var ammo = target.GetComponentInChildren<ModuleAmmo>();
		if (doApply) {
			ammo.ChangeMaxAmmo(ammoBoost);
		} else {
			ammo.ChangeMaxAmmo(-ammoBoost);
		}
	}

	protected override void _DetachedFromTrain() {
		ApplyBoost(Train.s.GetNextBuilding(true, GetComponentInParent<Cart>()), false);
		ApplyBoost(Train.s.GetNextBuilding(false, GetComponentInParent<Cart>()), false);
	}

	public string GetInfoText() {
		return "Doubles the max ammo of connected carts";
	}
}
