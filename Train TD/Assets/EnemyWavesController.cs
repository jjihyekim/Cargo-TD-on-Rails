using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class EnemyWavesController : MonoBehaviour {
	public GameObject enemyWavePrefab;

	public List<EnemyWave> waves = new List<EnemyWave>();

    public void UpdateBasedOnLevelData() {
	    /*var childCount = waves.Length;
	    for (int i = childCount-1; i >= 0 ; i--) {
		    Destroy(waves[i].gameObject);
	    }
	    
	    var waveData = SceneLoader.s.currentLevel.enemyWaves;

	    waves = new EnemyWave[waveData.Length];

	    for (int i = 0; i < waveData.Length; i++) {
		    var curWaveData = waveData[i];
		    waves[i] = Instantiate(enemyWavePrefab, transform).GetComponent<EnemyWave>();
		    var curWave = waves[i];
		    curWave.SetUp(GetCircuit(curWaveData.enemyPathType), curWaveData);
	    }*/

	    var enemiesOnPath = SceneLoader.s.currentLevel.enemiesOnPath;
	    for (int i = 0; i < enemiesOnPath.Length; i++) {
		    var wave = Instantiate(enemyWavePrefab, transform).GetComponent<EnemyWave>();
		    wave.SetUp(enemiesOnPath[i].enemyIdentifier, enemiesOnPath[i].distanceOnPath, enemiesOnPath[i].startMoving, enemiesOnPath[i].isLeft);
		    waves.Add(wave);
	    }
    }
    
    void Update()
    {
	    if (SceneLoader.s.isLevelInProgress) {
		    for (int i = 0; i < waves.Count; i++) {
			    waves[i].UpdateBasedOnDistance(SpeedController.s.currentDistance);
		    }
	    }
    }
}
