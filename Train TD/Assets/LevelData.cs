using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class LevelData {
	public string levelName = "unset";

	//public int missionRewardMoney = 200;
	
	public EnemyWaveData[] enemyWaves;
	

	[Header("Mission Length")]
	public int missionDistance = 300;
	
	
	public bool isRealLevel() {
		return levelName != "unset";
	}
}

[Serializable]
public class EnemyWaveData {
	[Title("$enemyUniqueName")]
	[ValueDropdown("GetAllEnemyNames")]
	public string enemyUniqueName = "unset";
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
	public float headsUpTime = 30;

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