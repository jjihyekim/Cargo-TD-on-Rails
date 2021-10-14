using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using Random = UnityEngine.Random;

public class MissionLoseFinisher : MonoBehaviour {
    public static MissionLoseFinisher s;

    private void Awake() {
        s = this;
    }


    public MonoBehaviour[] scriptsToDisable;
    public GameObject[] gameObjectsToDisable;
    
    
    public GameObject loseUI;
    public GameObject speedLose;
    public GameObject cargoLose;

    public TMP_Text tipText;

    public string[] speedLoseTips;
    public string[] cargoLoseTips;

    public void MissionLost(bool isLostBecauseOfCargo) {
        SceneLoader.s.isLevelFinished = true;
        
        for (int i = 0; i < scriptsToDisable.Length; i++) {
            scriptsToDisable[i].enabled = false;
        }
		
        for (int i = 0; i < gameObjectsToDisable.Length; i++) {
            gameObjectsToDisable[i].SetActive(false);
        }
        
        loseUI.SetActive(true);
        if (isLostBecauseOfCargo) {
            cargoLose.SetActive(true);
            speedLose.SetActive(false);
            tipText.text = cargoLoseTips[Random.Range(0, cargoLoseTips.Length)];
        } else {
            cargoLose.SetActive(false);
            speedLose.SetActive(true);
            tipText.text = speedLoseTips[Random.Range(0, speedLoseTips.Length)];
        }
        
        var myMission = DataSaver.s.GetCurrentSave().GetCurrentMission();
        AnalyticsResult analyticsResult = Analytics.CustomEvent(
            "LevelLost",
            new Dictionary<string, object> {
                { "Level", SceneLoader.s.currentLevel.levelName },
                { "distance", Mathf.RoundToInt(SpeedController.s.currentDistance / 10) *10},
                { "time", Mathf.RoundToInt(SpeedController.s.currentTime/10) * 10},
                { "isLostDueCargo", isLostBecauseOfCargo},
                
                {"finishedBefore", myMission.isWon},

                { "buildingsBuild", ModuleHealth.buildingsBuild },
                { "buildingsDestroyed", ModuleHealth.buildingsDestroyed },
				
                { "remainingMoney", MoneyController.s.money },
                { "enemiesLeftAlive", EnemyHealth.enemySpawned - EnemyHealth.enemyKilled},
                { "emptyTrainSlots", Train.s.GetEmptySlotCount() },
            }
        );
        
        
        Debug.Log("Mission Lost Analytics: " + analyticsResult);
        
        
        PlayerBuildingController.s.LogCurrentLevelBuilds(false);
    }


    /*public void Restart() {
        throw new NotImplementedException();
    }
    */


    public void BackToMenu() {
        SceneLoader.s.BackToMenu();
    }
}
