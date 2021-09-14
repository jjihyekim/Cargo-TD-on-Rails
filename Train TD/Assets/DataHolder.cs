using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataHolder : MonoBehaviour {
    public static DataHolder s;

    public GameObject cartPrefab;
    public float cartLength;
    
    private void Awake() {
        if (s == null)
            s = this;
    }


    public TrainBuildingHolder[] buildings;
    public EnemyHolder[] enemies;


    public TrainBuilding GetBuilding(string buildingName) {
        for (int i = 0; i < buildings.Length; i++) {
            if (PreProcess(buildings[i].uniqueName) == PreProcess(buildingName)) {
                return buildings[i].data;
            }
        }

        Debug.LogError($"Can't find building {buildingName}");
        return null;
    }
    
    public GameObject GetEnemy(string enemyName) {
        for (int i = 0; i < enemies.Length; i++) {
            if (PreProcess(enemies[i].uniqueName) == PreProcess(enemyName)) {
                return enemies[i].data;
            }
        }

        Debug.LogError($"Can't find enemy {enemyName}");
        return null;
    }

    string PreProcess(string input) {
        return input.Replace(" ", "").ToLower();
    } 
}


[Serializable]
public class TrainBuildingHolder {
    public string uniqueName = "unset";
    public TrainBuilding data;
}


[Serializable]
public class EnemyHolder {
    public string uniqueName = "unset";
    public GameObject data;
}