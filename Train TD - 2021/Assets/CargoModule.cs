using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CargoModule : MonoBehaviour, IActiveDuringCombat, IActiveDuringShopping {

    public bool isBuildingReward;
    public string myReward;
    
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
        Destroy(gameObject);
    }

    public void CargoReturned() {
        Destroy(gameObject);
    }

    public string GetReward() {
        return myReward;
    }
}
