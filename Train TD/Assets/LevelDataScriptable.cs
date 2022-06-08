using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu()]
public class LevelDataScriptable : ScriptableObject {
	public string levelName = "unset";
	public int levelMenuOrder = -1;
	//public GameObject train;
	
	public int missionRewardMoney = 200;
	
	public int trainLength = 3;
	public TrainBuildingData[] starterModules;
	//public TrainCartData[] levelTrain;
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

	//[Header("Mission rewarsds")


	[Header("Unlock Requirements")] 
	public int reputationRequirement;

	public bool isRealLevel() {
		return levelName != "unset";
	}
}