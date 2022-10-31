using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheatsController : MonoBehaviour
{
    public InputActionReference cheatButton;

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
            if(SceneLoader.s.isProfileMenu())
                ProfileSelectionMenu.s.StartGame();
            
            StarterUIController.s.QuickStart();

            //DataSaver.s.GetCurrentSave().currentRun.money += 10000;

        } else if(!SceneLoader.s.isLevelFinished()) {
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
