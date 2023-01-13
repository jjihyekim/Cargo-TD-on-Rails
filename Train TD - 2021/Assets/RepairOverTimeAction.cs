using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairOverTimeAction : ModuleAction, IActiveDuringCombat {
	public float repairPerSecond = 20;
	public float repairDuration = 10;
	
	protected override void _EngageAction() {
		StartCoroutine(RepairOverTime());
	}


	IEnumerator RepairOverTime() {
		var timer = repairDuration;
		var healths = LevelReferences.s.train.GetComponentsInChildren<ModuleHealth>();

		while ( timer > 0) {


			for (int i = 0; i < healths.Length; i++) {
				healths[i].DealDamage(-repairPerSecond * Time.deltaTime);
			}


			timer -= Time.deltaTime;
			yield return null;
		}
	}
	
	public void ActivateForCombat() {
		this.enabled = true;
	}

	public void Disable() {
		this.enabled = false;
	}
}
