using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using Sirenix.Serialization;


[CreateAssetMenu(menuName = "Tweakables/Parent")]
public class TweakablesParentScriptable : ScriptableObject {
	public TweakablesParent myTweakable;
}


[Serializable]
public class TweakablesParent {
	public TweakableEngineSystem engineSystem;
	public TweakableGlobals globalTweaks;

	public TweakableTrainBuilding[] trainBuildings;
	public TweakableEnemy[] enemies;
	public TweakableMissionSettings[] missions;
	public TweakableMapRewards[] rewards;
	public TweakablesParent Clone() {
		var serialized = SerializationUtility.SerializeValue(this, DataFormat.Binary);
		return SerializationUtility.DeserializeValue<TweakablesParent>(serialized, DataFormat.Binary);
	}
}


[Serializable]
public class TweakableEnemy  {
	[ValueDropdown("GetAllEnemyNames")]
	public string uniqueName = "unset";
	public int enemyHealth = 30;

	public int enemyScrapReward = 10;
	public int enemyAmmoReward = 10;
	public int enemyFuelReward = 10;

	public int enemySpeed = 5;

	public TweakableGunModule enemyGun;
	
	private static IEnumerable GetAllEnemyNames() {
		var enemies = GameObject.FindObjectOfType<DataHolder>().enemies;
		var enemyNames = new List<string>();
		for (int i = 0; i < enemies.Length; i++) {
			enemyNames.Add(enemies[i].uniqueName);
		}
		return enemyNames;
	}
}

[Serializable]
public class TweakableGunModule {
	public float fireDelay = 2f;
	public int fireBarrageCount = 2;
	public float fireBarrageDelay = 0.2f;
	public int projectileDamage = 4;
	public float rotationSpan = 360;
	public float range = 5;
	public float ammoPerShot = 0.5f;

	public void ApplyTweaks(GunModule module) {
		module.fireDelay = fireDelay;
		module.fireBarrageCount = fireBarrageCount;
		module.fireBarrageDelay = fireBarrageDelay;
		module.projectileDamage = projectileDamage;

		var targetPicker = module.GetComponent<TargetPicker>();
		targetPicker.rotationSpan = rotationSpan;
		targetPicker.range = range;
	}
}

[Serializable]
public class TweakableEngineSystem {
	
}

[Serializable]
public class TweakableTrainBuilding {
	public string uniqueName = "unset";
	public int scrapCost = 50;
	public int weight = 25;
	public int health = 200;

	public TweakableGunModule myGun;
	public TweakableModuleAction[] moduleActions;
	public TweakableEngineModule myEngine;
	public TweakableCargoModule myCargo;
}

[Serializable]
public class TweakableModuleAction {
	[ValueDropdown("GetAllBuildingNames")]
	public string uniqueName = "unset";
	public int cost = 25;
	public int cooldown = 60;
	
	public float actionTime = 8;
	public float boost = 5;

	public void ApplyTweaks(ModuleActionTweakable myAction, TweakableGlobals globals) {
		myAction.cost = cost;
		myAction.cooldown = cooldown;
		myAction.actionTime = actionTime;
		myAction.initialDelay = globals.moduleActionInitialDelay;
		myAction.endDelay = globals.moduleActionEndDelay;
		myAction.boost = 5;
	}
	
	private static IEnumerable GetAllBuildingNames() {
		var buildings = GameObject.FindObjectOfType<DataHolder>().buildings;
		var buildingNames = new List<string>();
		for (int i = 0; i < buildings.Length; i++) {
			buildingNames.Add(buildings[i].uniqueName);
		}
		return buildingNames;
	}
}

[Serializable]
public class TweakableEngineModule {
	public int enginePower = 200;

	public void ApplyTweaks(EngineModule module) {
		module.enginePower = enginePower;
	}
}

[Serializable]
public class TweakableCargoModule {

	public void ApplyTweaks(CargoModule module) {
		
	}
}

[Serializable]
public class TweakableGlobals {
	public float repairAndSellCostPerHealthLost_Battle = 0.5f;
	public float repairAndSellCostPerHealthLost_Shop = 0.75f;
	public float moduleActionInitialDelay = 1;
	public float moduleActionEndDelay = 2;
	public int cartHealth = 200;
	public int cartWeight = 50;
}

[Serializable]
public class TweakableMissionSettings {
	
}

[Serializable]
public class TweakableMapRewards {
	
}
