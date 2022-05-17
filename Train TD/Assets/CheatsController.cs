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
            //StarterUIController.s.QuickStart();

            DataSaver.s.GetCurrentSave().money += 10000;
            DataSaver.s.GetCurrentSave().debugExtraReputation += 10;

        } else {
            MoneyController.s.AddMoney(1000);

            /*var train = Train.s;
            var healths = train.GetComponentsInChildren<ModuleHealth>();

            foreach (var gModuleHealth in healths) {
                gModuleHealth.DealDamage(gModuleHealth.currentHealth/2);
            }*/
        }
    }
}
