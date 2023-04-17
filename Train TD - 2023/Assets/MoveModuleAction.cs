using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveModuleAction : ModuleAction, IActiveDuringCombat, IActiveDuringShopping {

    public bool freeInStation = false;
    private int realCost = -1;
    protected override void _EngageAction() {
        var building = GetComponent<TrainBuilding>();
        building.mySlot.TemporaryRemoval(building);
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
        var state = new DataSaver.TrainState.CartState.BuildingState();
        Train.ApplyBuildingToState(GetComponent<TrainBuilding>(),state);
        Train.ApplyStateToBuilding(newBuilding, state);
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
