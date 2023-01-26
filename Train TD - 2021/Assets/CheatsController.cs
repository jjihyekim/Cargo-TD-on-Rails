using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheatsController : MonoBehaviour
{
    public InputActionReference cheatButton;
    
    public EncounterTitle debugEncounter;

    public bool infiniteLevel = false;
    public bool debugNoRegularSpawns = false;
    public EnemyIdentifier debugEnemy;

    private void Start() {
        if(debugNoRegularSpawns || infiniteLevel)
            Debug.LogError("Debug options active!");
        
        if (debugNoRegularSpawns)
            EnemyWavesController.s.debugNoRegularSpawns = true;
    }

    private void Update() {
        if (infiniteLevel) {
            if (SpeedController.s.missionDistance - SpeedController.s.currentDistance < 50) {
                SpeedController.s.IncreaseMissionEndDistance(50);
            }
        }
    }

    [Button]
    void DebugEnemySpawn(int distance) {
        EnemyWavesController.s.DebugEnemySpawn(debugEnemy, distance);
    }

    [Button]
    public void EngageEncounter() {
        StarterUIController.s.DebugEngageEncounter(debugEncounter.title);
    }

    private void OnEnable() {
        cheatButton.action.Enable();
        cheatButton.action.performed += EngageCheat;
    }


    private void OnDisable() {
        cheatButton.action.Enable();
        cheatButton.action.performed -= EngageCheat;
    }

    private void EngageCheat(InputAction.CallbackContext obj) {

        if (!SceneLoader.s.isLevelStarted()) {
            if (WorldMapCreator.s.worldMapOpen) {
                MapController.s.DebugTravelToSelectStar();
                
            } else {
                if (SceneLoader.s.isProfileMenu())
                    ProfileSelectionMenu.s.StartGame();

                StarterUIController.s.QuickStart();

                //DataSaver.s.GetCurrentSave().currentRun.money += 10000;
            }
        } else if (!SceneLoader.s.isLevelFinished()) {
            //MoneyController.s.AddScraps(1000);

            MissionWinFinisher.s.MissionWon();

            /*var train = Train.s;
            var healths = train.GetComponentsInChildren<ModuleHealth>();

            foreach (var gModuleHealth in healths) {
                gModuleHealth.DealDamage(gModuleHealth.currentHealth/2);
            }*/
        } else {
            MissionWinFinisher.s.DebugRedoRewards();
        }

    }
}
