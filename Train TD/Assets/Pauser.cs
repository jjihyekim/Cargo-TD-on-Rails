using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;

public class Pauser : MonoBehaviour {
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
        if (LevelLoader.s.isLevelInProgress) {
            if (isPaused) {
                pauseMenu.SetActive(false);
                TimeController.s.UnPause();
                isPaused = false;
            } else {
                pauseMenu.SetActive(true);
                TimeController.s.Pause();
                isPaused = true;
            }
        }
    }


    private void Start() {
        Unpause();
    }


    public void Unpause() {
        isPaused = true;
        TogglePause();
    }

    public void AbandonMission() {
        var myMission = DataSaver.s.GetCurrentSave().GetCurrentMission();
        AnalyticsResult analyticsResult = Analytics.CustomEvent(
            "LevelAbandoned",
            new Dictionary<string, object> {
                { "Level", LevelLoader.s.currentLevel.levelName },
                { "distance", Mathf.RoundToInt(SpeedController.s.currentDistance / 10) *10},
                { "time", Mathf.RoundToInt(SpeedController.s.currentTime/10) * 10},
                { "cargoRatio", CargoController.s.aliveCargo/CargoController.s.totalCargo},
                
                {"finishedBefore", myMission.isWon},

                { "buildingsBuild", ModuleHealth.buildingsBuild },
                { "buildingsDestroyed", ModuleHealth.buildingsDestroyed },
				
                { "remainingMoney", MoneyController.s.money },
                { "enemiesLeftAlive", EnemyHealth.enemySpawned - EnemyHealth.enemyKilled},
                { "emptyTrainSlots", Train.s.GetEmptySlotCount() },
            }
        );
        
        TimeController.s.UnPause();
        LevelLoader.s.BackToMenu();
    }
}
