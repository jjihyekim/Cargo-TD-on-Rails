using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;

public class Pauser : MonoBehaviour {

    public static Pauser s;

    private void Awake() {
        s = this;
    }

    public InputActionReference pauseButton;

    public bool isPaused = false;

    public GameObject pauseMenu;


    private void OnEnable() {
        pauseButton.action.Enable();
        pauseButton.action.performed += TogglePause;
        
    }

    private void OnDisable() {
        pauseButton.action.Disable();
        pauseButton.action.performed -= TogglePause;
    }

    private void TogglePause(InputAction.CallbackContext obj) {
        TogglePause();
    }

    void TogglePause() {
        if (SceneLoader.s.isLevelInProgress) {
            isPaused = !isPaused;
            
            if (isPaused) {
                Pause();
            } else {
                Unpause();
            }
        }
    }


    private void Start() {
        Unpause();
    }


    public void Pause() {
        pauseMenu.SetActive(true);
        TimeController.s.Pause();
        isPaused = true;
    }

    public void Unpause() {
        pauseMenu.SetActive(false);
        TimeController.s.UnPause();
        isPaused = false;
    }

    public void AbandonMission() {
        AnalyticsResult analyticsResult = Analytics.CustomEvent(
            "LevelAbandoned",
            new Dictionary<string, object> {
                { "Level", SceneLoader.s.currentLevel.levelName },
                { "distance", Mathf.RoundToInt(SpeedController.s.currentDistance / 10) *10},
                { "time", Mathf.RoundToInt(SpeedController.s.currentTime/10) * 10},
                
                {"character", DataSaver.s.GetCurrentSave().currentRun.character},

                { "buildingsBuild", ModuleHealth.buildingsBuild },
                { "buildingsDestroyed", ModuleHealth.buildingsDestroyed },
                
                { "enemiesLeftAlive", EnemyHealth.enemySpawned - EnemyHealth.enemyKilled},
                { "emptyTrainSlots", Train.s.GetEmptySlotCount() },
            }
        );
        
        TimeController.s.UnPause();
        SceneLoader.s.BackToStarterMenuHardLoad();
    }
}
