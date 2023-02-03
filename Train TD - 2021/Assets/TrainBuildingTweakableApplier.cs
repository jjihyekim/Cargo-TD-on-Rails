using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class TrainBuildingTweakableApplier : MonoBehaviour
{
	
	/*[ValueDropdown("GetAllBuildingNames")]
	public string uniqueName = "unset";

	private void OnEnable() {
		//ApplyStats(DataHolder.s.GetTweaks());
		//DataHolder.s.tweakableChanged.AddListener(ApplyStats);
	}

	private void OnDisable() {
		//DataHolder.s.tweakableChanged.RemoveListener(ApplyStats);
	}
	private void ApplyStats(TweakablesParent tweakablesParent) {
		TweakableTrainBuilding myTweaks = null;
		for (int i = 0; i < tweakablesParent.trainBuildings.Length; i++) {
			if (tweakablesParent.trainBuildings[i].uniqueName == uniqueName) {
				myTweaks = tweakablesParent.trainBuildings[i];
			}
		}

		if (myTweaks == null) {
			Debug.LogError($"Tweaks for enemy:{uniqueName} not found!");
			return;
		}

		var building = GetComponent<TrainBuilding>();
		building.cost = myTweaks.scrapCost;
		building.weight = myTweaks.weight;

		var health = GetComponent<ModuleHealth>();
		var hpPercent = health.currentHealth / health.maxHealth;
		health.maxHealth = myTweaks.health;
		health.SetHealth(myTweaks.health*hpPercent);

		var gun = GetComponent<GunModule>();
		if(gun != null)
			myTweaks.myGun.ApplyTweaks(gun);

		var engine = GetComponent<EngineModule>();
		if (engine != null) 
			myTweaks.myEngine.ApplyTweaks(engine);

		var cargo = GetComponent<CargoModule>();
		if(cargo != null)
			myTweaks.myCargo.ApplyTweaks(cargo);


		for (int i = 0; i < myTweaks.moduleActions.Length; i++) {
			var action = myTweaks.moduleActions[i];
			if (action != null) {
				switch (action.uniqueName) {
					case "quickfire":
						var module = GetComponent<QuickfireAction>();
						if(module != null)
							action.ApplyTweaks(module, tweakablesParent.globalTweaks);
						break;
				}
			}
		}
	}
	
	private static IEnumerable GetAllBuildingNames() {
		var buildings = GameObject.FindObjectOfType<DataHolder>().buildings;
		var buildingNames = new List<string>();
		for (int i = 0; i < buildings.Length; i++) {
			buildingNames.Add(buildings[i].uniqueName);
		}
		return buildingNames;
	}*/
}
