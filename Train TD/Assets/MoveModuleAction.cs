using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveModuleAction : ModuleAction
{
    protected override void _EngageAction() {
        var building = GetComponent<TrainBuilding>();
        PlayerBuildingController.s.StartBuilding(DataHolder.s.GetBuilding(building.uniqueName), BuildingDoneCallback, GetTheFinishedBuilding, true,false);
        PlayerModuleSelector.s.HideModuleActionSelector();
        building.SetHighlightState(true);
    }

    bool BuildingDoneCallback(bool isSuccess) {
        var building = GetComponent<TrainBuilding>();
        if (isSuccess) {
            Destroy(gameObject);
        } else {
            RefundAction();
            PlayerModuleSelector.s.ShowModuleActionSelector();
        }

        return false;
    }

    void GetTheFinishedBuilding(TrainBuilding newBuilding) {
        newBuilding.SetCurrentHealth(GetComponent<ModuleHealth>().currentHealth);

        var ammo = GetComponent<ModuleAmmo>();

        if (ammo != null) {
            var newAmmo = newBuilding.GetComponent<ModuleAmmo>();

            newAmmo.curAmmo = ammo.curAmmo;
        }
    }
}
