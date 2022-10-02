using System;
using System.Collections;
using System.Collections.Generic;
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

    public void UpdateBasedOnLevelData() {
	    var enemiesOnPath = SceneLoader.s.currentLevel.enemiesOnPath;
	    for (int i = 0; i < enemiesOnPath.Length; i++) {
		    SpawnEnemy(enemiesOnPath[i].enemyIdentifier, enemiesOnPath[i].distanceOnPath, enemiesOnPath[i].startMoving/*, enemiesOnPath[i].isLeft*/);
	    }
    }

    void SpawnEnemy(EnemyIdentifier enemyIdentifier, float distance, bool startMoving) {
	    var wave = Instantiate(enemyWavePrefab, transform).GetComponent<EnemyWave>();
	    wave.SetUp(enemyIdentifier, distance, startMoving, Random.value > 0.5f);
	    waves.Add(wave);
    }
    
    void Update()
    {
	    if (SceneLoader.s.isLevelInProgress) {

		    var playerDistance = SpeedController.s.currentDistance;
		    
		    for (int i = 0; i < waves.Count; i++) {
			    waves[i].UpdateBasedOnDistance(playerDistance);
		    }


		    for (int i = 0; i < SceneLoader.s.currentLevel.dynamicSpawnEnemies.Length; i++) {
			    var enemy = SceneLoader.s.currentLevel.dynamicSpawnEnemies[i];
			    if (enemy.firstSpawned) {
				    if (enemy.curTime >= enemy.spawnInterval) {
					    SpawnEnemy(enemy.enemyIdentifier, playerDistance - enemy.distanceFromTrain, true);
					    enemy.curTime = 0;
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
    }

    public void RemoveWave(EnemyWave toRemove) {
	    waves.Remove(toRemove);
    }
}
