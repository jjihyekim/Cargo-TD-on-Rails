using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class LevelData {
	[HideInInspector]
	public string levelName = "unset";
	
	public bool isEncounter = false;

	public EnemyOnPathData[] enemiesOnPath;
	public EnemyDynamicSpawnData[] dynamicSpawnEnemies;
	
	[Header("Mission Length")]
	public int missionDistance = 300;
	
	public bool isRealLevel() {
		return levelName != "unset";
	}

	public LevelData Copy() {
		var serialized = SerializationUtility.SerializeValue(this, DataFormat.Binary);
		return SerializationUtility.DeserializeValue<LevelData>(serialized, DataFormat.Binary);
	}
}

[Serializable]
public class EnemyOnPathData {
	public EnemyIdentifier enemyIdentifier = new EnemyIdentifier();

	public int distanceOnPath = 30;
	public bool startMoving = false;
	//public bool isLeft = false;
}

[Serializable]
public class EnemyDynamicSpawnData {
	public EnemyIdentifier enemyIdentifier = new EnemyIdentifier();

	public int distanceFromTrain = 30;
	public float firstSpawnTime = 30;
	public float spawnInterval = 30;

	public int increaseInNumberInterval = 2;
	[NonSerialized]
	public float curTime = 0;
	[NonSerialized]
	public bool firstSpawned = false;
	[NonSerialized] 
	public int curIncreaseInNumberCount = 0;
	//public bool isLeft = false;
}

[Serializable]
public class EnemyIdentifier{

	[Title("$enemyUniqueName")]
	[ValueDropdown("GetAllEnemyNames")]
	public string enemyUniqueName = "unset";
	public int enemyCount = 1;
	
	private static IEnumerable GetAllEnemyNames() {
		var enemies = GameObject.FindObjectOfType<DataHolder>().enemies;
		var enemyNames = new List<string>();
		for (int i = 0; i < enemies.Length; i++) {
			enemyNames.Add(enemies[i].uniqueName);
		}
		return enemyNames;
	}
}