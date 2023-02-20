using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

public class EnemyWavesController : MonoBehaviour {
	public static EnemyWavesController s;

	private void Awake() {
		s = this;
	}

	public GameObject enemyWavePrefab;

	public List<EnemyWave> waves = new List<EnemyWave>();

	public bool enemiesInitialized = false;

	[NonSerialized] public bool debugNoRegularSpawns = false;


	public MiniGUI_PursuerTimer pursuerTimerObject;


	private void Start() {
		Cleanup();
	}


	public void SetUpLevel() {
		Cleanup();
		pursuerTimerObject.SetUp(SceneLoader.s.currentLevel.dynamicSpawnData);

		pursuerTimerObject.gameObject.SetActive(false);

		var curDynamicSpawn = SceneLoader.s.currentLevel.dynamicSpawnData;
		curDynamicSpawn.curTime = curDynamicSpawn.firstSpawnTime;
	}

	public Queue<PowerUpScriptable> powerUpScriptables = new Queue<PowerUpScriptable>();
	public void SpawnEnemiesOnSegment(float segmentStartDistance, LevelSegment segment) {
		if (debugNoRegularSpawns)
			return;

		enemiesInitialized = !segment.isEncounter;
		if (enemiesInitialized) {

			PowerUpScriptable powerUpScriptable = null;
			if (segment.rewardPowerUpAtTheEnd) {
				powerUpScriptable = DataHolder.s.GetPowerUp(segment.powerUpRewardUniqueName);
			}

			powerUpScriptables.Enqueue(powerUpScriptable);
			
			var enemiesOnPath = segment.enemiesOnPath;
			for (int i = 0; i < enemiesOnPath.Length; i++) {
				SpawnEnemy(enemiesOnPath[i].enemyIdentifier, segmentStartDistance+ enemiesOnPath[i].distanceOnPath, false, enemiesOnPath[i].isLeft, powerUpScriptable);
			}
		}
	}



	public void DebugEnemySpawn(EnemyIdentifier debugEnemy, int distance) {
		SpawnEnemy(debugEnemy, SpeedController.s.currentDistance + distance, false, false);
	}

	public void SpawnEnemy(EnemyOnPathData data) {
		SpawnEnemy(data.enemyIdentifier, data.distanceOnPath, false, data.isLeft);
	}

	public int maxConcurrentWaves = 6;

	void SpawnEnemy(EnemyIdentifier enemyIdentifier, float distance, bool startMoving, bool isLeft, PowerUpScriptable powerUp = null) {
		var playerDistance = SpeedController.s.currentDistance;
		var wave = Instantiate(enemyWavePrefab, Vector3.forward * (distance - playerDistance), Quaternion.identity).GetComponent<EnemyWave>();
		wave.transform.SetParent(transform);
		wave.SetUp(enemyIdentifier, distance, startMoving, isLeft, powerUp);
		waves.Add(wave);
	}

	void Update() {
		if (SceneLoader.s.isLevelInProgress && enemiesInitialized) {
			pursuerTimerObject.gameObject.SetActive(true);

			var playerDistance = SpeedController.s.currentDistance;

			for (int i = 0; i < waves.Count; i++) {
				waves[i].UpdateBasedOnDistance(playerDistance);
			}


			if (debugNoRegularSpawns)
				return;

			if (waves.Count < maxConcurrentWaves) {
				//for (int i = 0; i < SceneLoader.s.currentLevel.dynamicSpawnEnemies.Length; i++) {
					var enemy = SceneLoader.s.currentLevel.dynamicSpawnData;
					
					if (enemy.curTime <= 0) {
						SpawnEnemy(enemy.enemyIdentifier, playerDistance - enemy.distanceFromTrain, true, Random.value > 0.5f);
						enemy.curTime = enemy.spawnInterval;

						enemy.curIncreaseInNumberCount += 1;
						if (enemy.increaseInNumberInterval >= 0 && enemy.curIncreaseInNumberCount >= enemy.increaseInNumberInterval) {
							enemy.enemyIdentifier.enemyCount += 1;
							enemy.curIncreaseInNumberCount = 0;
						}
					}

					enemy.curTime -= Time.deltaTime;
				//}
			}
		} else {
			var playerDistance = SpeedController.s.currentDistance;

			for (int i = 0; i < waves.Count; i++) {
				waves[i].UpdateBasedOnDistance(playerDistance);
			}
		}
	}

	public void RemoveWave(EnemyWave toRemove) {
		waves.Remove(toRemove);
	}

	public void Cleanup() {
		pursuerTimerObject.gameObject.SetActive(false);
		transform.DeleteAllChildren();
		enemiesInitialized = false;
		powerUpScriptables.Clear();
	}
}
