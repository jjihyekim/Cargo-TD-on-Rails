using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class LevelData {
	public string levelName = "unset";
	public int levelMenuOrder = -1;
	//public GameObject train;
	public int trainLength = 3;

	public int missionRewardMoney = 200;
	
	public TrainBuildingData[] starterModules;
	public EnemyWaveData[] enemyWaves;

	[Header("Money")]

	public int startingMoney = 40;
	public float moneyGainSpeed = 1f;

	[Header("Mission Length")]
	public int missionDistance = 300;
	
	[Header("Mission Speed requirements")]
	public int bestEngineSpeed = 200;
	public int mediumEngineSpeed = 150;
	public int worstEngineSpeed = 100;

	//[Header("Mission rewarsds")]

	[Header("Reputation Requirement")] 
	public int reputationRequirement;

	public bool isRealLevel() {
		return levelName != "unset";
	}

	private bool hasArmoredEnemy = false;
	private bool armoredEnemyCheckMade = false;
	public bool HasArmoredEnemy() {
		if (!armoredEnemyCheckMade) {
			for (int i = 0; i < enemyWaves.Length; i++) {
				if (enemyWaves[i].enemyUniqueName == "Army") {
					hasArmoredEnemy = true;
					break;
				}
			}

			armoredEnemyCheckMade = true;
		}

		return hasArmoredEnemy;
	}
}

[Serializable]
public class TrainBuildingData {
	public string uniqueName = "unset";
	public int count = 1;
}

[Serializable]
public class EnemyWaveData {
	[Title("$enemyUniqueName", "@(startDistance + headsUpTime*2).ToString()")]
	public string enemyUniqueName = "unset";

	[ValueDropdown("GetAllEnemyNames")]
	public string unique2 = "unset";
	public float enemyData = -1;

	public enum EnemyPathType {
		backFarToClose, backClose, mountainsToFront, mountainsToBack, frontToBack, 
	}

	
	[HorizontalGroup("Group 1")]
	public bool isLeft = true;
	[HideLabel]
	[HorizontalGroup("Group 1")]
	public EnemyPathType enemyPathType;

	public int startDistance = 10;
	public float headsUpTime = 30f;

	public float accurateDistance {
		get { return (startDistance + headsUpTime * 2); }
	}
	
	private static IEnumerable GetAllEnemyNames() {
		var enemies = GameObject.FindObjectOfType<DataHolder>().enemies;
		var enemyNames = new List<string>();
		for (int i = 0; i < enemies.Length; i++) {
			enemyNames.Add(enemies[i].uniqueName);
		}
		return enemyNames;
	}
}

public interface IData {
	public void SetData(float data);
}