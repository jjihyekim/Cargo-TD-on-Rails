using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class TrainBuildingTweakableApplier : MonoBehaviour
{
	private void Start() {
		//ApplyStats(DataHolder.s.GetTweaks());
		TweakablesMaster.s.tweakableChanged.AddListener(ApplyStats);
		ApplyStats();
	}

	
	private void ApplyStats() {
		var cart = GetComponent<Cart>();
		var trainBuilding = GetComponent<TrainBuilding>();
		var health = GetComponent<ModuleHealth>();
		
		if (cart != null) {
			health.damageReductionMultiplier = TweakablesMaster.s.myTweakables.cartDamageReductionMultiplier;
		}

		var moduleStorages = GetComponents<ModuleStorage>();

		if (moduleStorages.Length == 1) {
			if (moduleStorages[0].myType == ResourceTypes.ammo) {
				moduleStorages[0].generationPerSecond = 1f / TweakablesMaster.s.myTweakables.ammoStorageAmmoGenDelay;
			}
		}

		if (moduleStorages.Length == 3) {
			for (int i = 0; i < moduleStorages.Length; i++) {
				if (moduleStorages[i].myType == ResourceTypes.ammo) {
					moduleStorages[i].generationPerSecond = 1f / TweakablesMaster.s.myTweakables.tripleStorageAmmoGenDelay;
				}
			}
		}
	}
}
