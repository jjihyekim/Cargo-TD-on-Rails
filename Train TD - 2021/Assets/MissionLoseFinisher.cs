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

    public TMP_Text tipText;

    public string[] loseTips;

    public void MissionLost() {
        SceneLoader.s.FinishLevel();
        
        for (int i = 0; i < scriptsToDisable.Length; i++) {
            scriptsToDisable[i].enabled = false;
        }
		
        for (int i = 0; i < gameObjectsToDisable.Length; i++) {
            gameObjectsToDisable[i].SetActive(false);
        }
        
        loseUI.SetActive(true);
        
        
        var myChar = DataSaver.s.GetCurrentSave().currentRun.character;
        AnalyticsResult analyticsResult = Analytics.CustomEvent(
            "LevelLost",
            new Dictionary<string, object> {
                { "Level", SceneLoader.s.currentLevel.levelName },
                { "distance", Mathf.RoundToInt(SpeedController.s.currentDistance / 10) *10},
                { "time", Mathf.RoundToInt(SpeedController.s.currentTime/10) * 10},
                
                {"character", myChar.uniqueName},
				
                { "remainingScraps", MoneyController.s.scraps },
                { "remainingMoney", MoneyController.s.money },
                
                { "enemiesLeftAlive", EnemyHealth.enemySpawned - EnemyHealth.enemyKilled},
            }
        );
        
        
        Debug.Log("Mission Lost Analytics: " + analyticsResult);
        
        
        PlayerBuildingController.s.LogCurrentLevelBuilds(false);
        MusicPlayer.s.Stop();
        DirectControlMaster.s.DisableDirectControl();
    }


    /*public void Restart() {
        throw new NotImplementedException();
    }
    */


    public void BackToMenu() {
        loseUI.SetActive(false);
        SettingsController.s.ResetRun();
        MusicPlayer.s.SwapMusicTracksAndPlay(false);
        //SceneLoader.s.BackToStarterMenuHardLoad();
    }
}
