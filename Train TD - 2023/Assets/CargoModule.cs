using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CargoModule : MonoBehaviour, IActiveDuringCombat, IActiveDuringShopping {

    [SerializeField]
    private bool isBuildingReward;
    [SerializeField]
    private string myReward;

    public bool isLeftCargo;

    public SpriteRenderer[] icons;
    
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

    public bool IsBuildingReward() {
        return isBuildingReward;
    }
    
    public string GetReward() {
        return myReward;
    }

    public Sprite GetRewardIcon() {
        if (isBuildingReward) {
            return DataHolder.s.GetCart(myReward).Icon;
        } else {
            return DataHolder.s.GetPowerUp(myReward).icon;
        }
    }

    public void SetCargo(DataSaver.TrainState.CartState.CargoState cargoState) {
        isBuildingReward = cargoState.isBuildingCargo;
        myReward = cargoState.cargoReward;
        isLeftCargo = cargoState.isLeftCargo;

        var icon = GetRewardIcon();
        
        for (int i = 0; i < icons.Length; i++) {
            icons[i].sprite = icon;
        }
    }
}
