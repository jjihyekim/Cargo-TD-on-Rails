using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveModuleAction : ModuleAction, IActiveDuringCombat, IActiveDuringShopping {

    public bool freeInStation = false;
    private int realCost = -1;
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
        
        Train.s.SaveTrainState();

        return false;
    }

    void GetTheFinishedBuilding(TrainBuilding newBuilding) {
        newBuilding.SetCurrentHealth(GetComponent<ModuleHealth>().currentHealth);

        var ammo = GetComponent<ModuleAmmo>();

        if (ammo != null) {
            var newAmmo = newBuilding.GetComponent<ModuleAmmo>();

            newAmmo.SetAmmo(ammo.curAmmo);
        }
    }

    public void ActivateForCombat() {
        this.enabled = true;
        
        if (freeInStation) {
            if (realCost != -1) {
                cost = realCost;
            }
        }
    }

    public void ActivateForShopping() {
        this.enabled = true;
        if (freeInStation) {
            if (realCost == -1) {
                realCost = cost;
            }
            cost = 0;
        }
    }

    public void Disable() {
        this.enabled = false;
    }
}
