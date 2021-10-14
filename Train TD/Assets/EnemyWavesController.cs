using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class EnemyWavesController : MonoBehaviour {
	public WaypointHolder[] myWaypoints;

	public GameObject enemyWavePrefab;

	public EnemyWave[] waves;

    public void UpdateBasedOnLevelData() {
	    var childCount = waves.Length;
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
	    }
    }

    WaypointCircuit GetCircuit(EnemyWaveData.EnemyPathType type) {
	    for (int i = 0; i < myWaypoints.Length; i++) {
		    if (myWaypoints[i].pathType == type) {
			    return myWaypoints[i].circuit;
		    }
	    }

	    Debug.LogError($"Can't find circuit {type}");
	    return null;
    }
    
    void Update()
    {
	    if (SceneLoader.s.isLevelInProgress) {
		    for (int i = 0; i < waves.Length; i++) {
			    waves[i].UpdateBasedOnDistance(SpeedController.s.currentDistance);
		    }
	    }
    }
}



[Serializable]
public class WaypointHolder {
	public EnemyWaveData.EnemyPathType pathType;
	public WaypointCircuit circuit;
}
