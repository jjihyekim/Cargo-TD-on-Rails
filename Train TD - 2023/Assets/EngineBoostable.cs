using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineBoostable : ActivateWhenAttachedToTrain {

	public bool engineDamageBoostActive = false;
	public bool engineDamageReductionActive = false;

	protected override void _AttachedToTrain() {
		for (int i = 0; i < Train.s.carts.Count; i++) {
			ApplyBoost(Train.s.carts[i], true);
		}
	}

	protected override bool CanApply(Cart target) {
		var gun = target.GetComponentInChildren<GunModule>();
		return gun != null;
	}

	protected override void _ApplyBoost(Cart target, bool doApply) {
		var gun = target.GetComponentInChildren<GunModule>();
		if (engineDamageBoostActive) {
			if (doApply) {
				gun.damageMultiplier += 2;
			} 
			
			target.SetBuildingBoostState(doApply ? 1f : 0f);
		} else if (engineDamageReductionActive){
			
			if (doApply) {
				gun.damageMultiplier -= 0.5f;
			} 
			
			target.SetBuildingBoostState(doApply ? 0.5f : 0f);
		} else {
			target.SetBuildingBoostState( 0f);
		}

	}

	protected override void _DetachedFromTrain() {
		for (int i = 0; i < Train.s.carts.Count; i++) {
			ApplyBoost(Train.s.carts[i], false);
		}
	}
}
