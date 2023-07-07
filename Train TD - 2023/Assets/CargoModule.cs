using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CargoModule : MonoBehaviour, IActiveDuringCombat, IActiveDuringShopping {

    [SerializeField]
    private DataSaver.TrainState.CartState.CargoState myState;

    public SpriteRenderer[] icons;

    private void Start() {
        highlight.SetActive(false);
    }

    public void ActivateForCombat() {
        this.enabled = true;
    }

    public void ActivateForShopping() {
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }

    public DataSaver.TrainState.CartState.CargoState GetState() {
        return myState;
    }

    public Sprite GetRewardIcon() {
        /*if (isBuildingReward) {
            return DataHolder.s.GetCart(myReward).Icon;
        } else {
            return DataHolder.s.GetPowerUp(myReward).icon;
        }*/
        return DataHolder.s.GetCart(myState.cargoReward).Icon;
    }

    public void SetCargo(DataSaver.TrainState.CartState.CargoState cargoState) {
        myState = cargoState;

        var icon = GetRewardIcon();
        
        for (int i = 0; i < icons.Length; i++) {
            icons[i].sprite = icon;
        }
    }

    public GameObject highlight;
    public void HighlightForDelivery() {
        highlight.SetActive(true);
    }
}
