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

	[NonSerialized]
	public bool debugNoRegularSpawns = false;
    public void UpdateBasedOnLevelData() {
	    Cleanup();
	    if(debugNoRegularSpawns)
		    return;
	    
	    if(!DataSaver.s.GetCurrentSave().isInARun || SceneLoader.s.currentLevel == null || SceneLoader.s.currentLevel.isEncounter)
		    return;
	    
	    enemiesInitialized = !SceneLoader.s.currentLevel.isEncounter;
	    if (enemiesInitialized) {
		    var enemiesOnPath = SceneLoader.s.currentLevel.enemiesOnPath;
		    for (int i = 0; i < enemiesOnPath.Length; i++) {
			    SpawnEnemy(enemiesOnPath[i].enemyIdentifier, enemiesOnPath[i].distanceOnPath, enemiesOnPath[i].startMoving /*, enemiesOnPath[i].isLeft*/);
		    }
	    }
    }


    
    public void DebugEnemySpawn(EnemyIdentifier debugEnemy, int distance) {
	    SpawnEnemy(debugEnemy, SpeedController.s.currentDistance + distance, false);
    }

    public void SpawnEnemy(EnemyOnPathData data) {
	    SpawnEnemy(data.enemyIdentifier, data.distanceOnPath, data.startMoving);
    }

    public int maxConcurrentWaves = 6;

    void SpawnEnemy(EnemyIdentifier enemyIdentifier, float distance, bool startMoving) {
	    var playerDistance = SpeedController.s.currentDistance;
	    var wave = Instantiate(enemyWavePrefab, Vector3.forward*(distance-playerDistance), Quaternion.identity).GetComponent<EnemyWave>();
	    wave.transform.SetParent(transform);
	    wave.SetUp(enemyIdentifier, distance, startMoving, Random.value > 0.5f);
	    waves.Add(wave);
    }
    
    void Update()
    {
	    if (SceneLoader.s.isLevelInProgress && enemiesInitialized) {

		    var playerDistance = SpeedController.s.currentDistance;
		    
		    for (int i = 0; i < waves.Count; i++) {
			    waves[i].UpdateBasedOnDistance(playerDistance);
		    }

		    
		    if(debugNoRegularSpawns)
			    return;
		    
		    if (waves.Count < maxConcurrentWaves) {
			    for (int i = 0; i < SceneLoader.s.currentLevel.dynamicSpawnEnemies.Length; i++) {
				    var enemy = SceneLoader.s.currentLevel.dynamicSpawnEnemies[i];
				    if (enemy.firstSpawned) {
					    if (enemy.curTime >= enemy.spawnInterval) {
						    SpawnEnemy(enemy.enemyIdentifier, playerDistance - enemy.distanceFromTrain, true);
						    enemy.curTime = 0;

						    enemy.curIncreaseInNumberCount += 1;
						    if (enemy.increaseInNumberInterval >= 0  && enemy.curIncreaseInNumberCount >= enemy.increaseInNumberInterval) {
							    enemy.enemyIdentifier.enemyCount += 1;
							    enemy.curIncreaseInNumberCount = 0;
						    }
					    }
				    } else {
					    if (enemy.curTime >= enemy.firstSpawnTime) {
						    SpawnEnemy(enemy.enemyIdentifier, playerDistance - enemy.distanceFromTrain, true);
						    enemy.curTime = 0;
						    enemy.firstSpawned = true;
					    }
				    }

				    enemy.curTime += Time.deltaTime;
			    }
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
	    transform.DeleteAllChildren();
	    enemiesInitialized = false;
    }
}
