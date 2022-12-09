using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CargoModule : MonoBehaviour, IActiveDuringCombat, IActiveDuringShopping {

    public int cargoSize = 1;

    public int moneyReward = 100;
    //public int moneyCost = 50;
    
    public enum CargoTypes {
        ammo, fuel, food
    }

    public CargoTypes myCargoType;

    public void ActivateForCombat() {
        this.enabled = true;
    }

    public void ActivateForShopping() {
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }
    
    public void CargoSold() {
        GetComponentInParent<Slot>().RemoveBuilding(GetComponent<TrainBuilding>());
        Destroy(gameObject);
    }

    public int GetReward() {
        var health = GetComponent<ModuleHealth>().GetHealthPercent();
        return (int)(health * moneyReward);
    }
}
