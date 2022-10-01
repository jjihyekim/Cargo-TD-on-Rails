using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class LevelData {
	public string levelName = "unset";

	public EnemyOnPathData[] enemiesOnPath;
	public EnemyDynamicSpawnData[] dynamicSpawnEnemies;
	
	[Header("Mission Length")]
	public int missionDistance = 300;
	
	
	public bool isRealLevel() {
		return levelName != "unset";
	}
}

[Serializable]
public class EnemyOnPathData {
	public EnemyIdentifier enemyIdentifier = new EnemyIdentifier();

	public int distanceOnPath = 30;
	public bool startMoving = false;
	public bool isLeft = false;
}

[Serializable]
public class EnemyDynamicSpawnData {
	public EnemyIdentifier enemyIdentifier = new EnemyIdentifier();

	public int distanceFromTrain = 30;
	public float firstSpawnTime = 30;
	public float spawnInterval = 30;
	public bool isLeft = false;
}

[Serializable]
public class EnemyIdentifier{

	[Title("$enemyUniqueName")]
	[ValueDropdown("GetAllEnemyNames")]
	public string enemyUniqueName = "unset";
	public int enemyCount = 1;
	// 5 speed ~= regular speed
	// 1 speed ~= min train sped
	// 10 speed ~= max speed
	public int enemySpeed = 7; 
	
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