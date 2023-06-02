using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class Pauser : MonoBehaviour {

    public static Pauser s;

    private void Awake() {
        s = this;
    }

    public InputActionReference pauseButton;

    public bool isPaused = false;
    public GameObject pauseMenu;
    public Button gamepadSelect;

    private void OnApplicationFocus(bool hasFocus) {
        if(enabled && !Application.isEditor)
            if(!hasFocus)
                Pause();
    }

    private void OnApplicationPause(bool pauseStatus) {
        if(enabled && !Application.isEditor)
            if(pauseStatus)
                Pause();
    }

    private void OnEnable() {
        pauseButton.action.Enable();
        pauseButton.action.performed += TogglePause;
        
    }

    private void OnDisable() {
        pauseButton.action.Disable();
        pauseButton.action.performed -= TogglePause;
    }

    private void TogglePause(InputAction.CallbackContext obj) {
        var menuToggles = pauseMenu.GetComponentsInChildren<MenuToggle>();
        var menuClosed = false;

        for (int i = 0; i < menuToggles.Length; i++) {
            if (menuToggles[i].isMenuActive) {
                menuClosed = true;
                menuToggles[i].HideMenu();
            }
        }
        
        if(!menuClosed)
            TogglePause();
    }

    void TogglePause() {
        isPaused = !isPaused;
        
        if (isPaused) {
            Pause();
        } else {
            Unpause();
        }
        
        //gamepadSelect.transition
    }


    private void Start() {
        Unpause();
    }


    public void Pause() {
        pauseMenu.SetActive(true);
        TimeController.s.Pause();
        isPaused = true;
        
        if (CameraController.s.directControlActive) {
            Cursor.lockState = CursorLockMode.None;
        }

        if (SettingsController.GamepadMode()) {
            EventSystem.current.SetSelectedGameObject(gamepadSelect.gameObject);
        }
    }

    public void Unpause() {
        pauseMenu.SetActive(false);
        TimeController.s.UnPause();
        isPaused = false;
        
        if (CameraController.s.directControlActive) {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void AbandonMission() {
        AnalyticsResult analyticsResult = Analytics.CustomEvent(
            "LevelAbandoned",
            new Dictionary<string, object> {
                { "Level", PlayStateMaster.s.currentLevel.levelName },
                { "distance", Mathf.RoundToInt(SpeedController.s.currentDistance / 10) *10},
                { "time", Mathf.RoundToInt(SpeedController.s.currentTime/10) * 10},
                
                {"character", DataSaver.s.GetCurrentSave().currentRun.character.uniqueName},

                { "buildingsBuild", ModuleHealth.buildingsBuild },
                { "buildingsDestroyed", ModuleHealth.buildingsDestroyed },
                
                { "enemiesLeftAlive", EnemyHealth.enemySpawned - EnemyHealth.enemyKilled},
            }
        );
        
        Unpause();
        FirstTimeTutorialController.s.StopInitialCutscene();
        MissionLoseFinisher.s.MissionLost();
    }
}
