using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadAction : ModuleAction, IActiveDuringCombat, IActiveDuringShopping {
	[NonSerialized]
	public int fullCost = -1;

	//[NonSerialized] public float costWithoutAffordability;
	private ModuleAmmo myMod;
	protected override void _Start() {
		if(fullCost == -1)
			fullCost = cost;
		myMod = GetComponent<ModuleAmmo>();
	}

	protected override void _EngageAction() {
		var ammoToBuy = ((float)cost / (float)fullCost) * myMod.maxAmmo;
		var totalAmmo = myMod.curAmmo + ammoToBuy;
		myMod.SetAmmo(totalAmmo);
	}

	protected override void _Update() {
		var percent = (1f - (float)myMod.curAmmo / myMod.maxAmmo);
		/*costWithoutAffordability = (percent * fullCost);

		var possibleToPay = MoneyController.s.GetAmountPossibleToPay(myType, costWithoutAffordability);
		if (possibleToPay > 0)
			cost = Mathf.FloorToInt(possibleToPay);
		else 
			cost = (int)costWithoutAffordability;*/

		if (percent > 0.1f) {
			cost = 1;
		}else {
			cost = 0;
		}

		canEngage = percent > 0.1f;
	}

	public void GiveBackCurrentStoredAmmo() {
		var inversePercent = ((float)myMod.curAmmo / myMod.maxAmmo);
		var refundAmount = (int)( inversePercent * fullCost);
		
		LevelReferences.s.SpawnResourceAtLocation(myType, refundAmount, transform.position);
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
