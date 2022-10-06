using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CargoModule : MonoBehaviour, IActiveDuringCombat {

    public int cargoSize = 1;

    public int moneyReward = 100;
    public int moneyCost = 50;

    private void OnEnable() {
        /*if(SceneLoader.s.isLevelInProgress)
	        CargoController.s.AliveCargo(cargoSize);*/
    }

    private void OnDisable() {
        /*if(SceneLoader.s.isLevelInProgress && CargoController.s != null) 
            CargoController.s.DeadCargo(cargoSize);*/
    }
    
    public void ActivateForCombat() {
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }

    public void CargoSold() {
        GetComponentInParent<Slot>().RemoveBuilding(GetComponent<TrainBuilding>());
        Destroy(gameObject);
    }
}
