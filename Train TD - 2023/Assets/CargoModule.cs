using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CargoModule : MonoBehaviour, IActiveDuringCombat, IActiveDuringShopping {

    [SerializeField]
    private string myRewardCart;

    [SerializeField] private string myRewardArtifact;

    public bool isLeftCargo;

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

    public string GetRewardArtifact() {
        return myRewardArtifact;
    }

    public string GetRewardCart() {
        return myRewardCart;
    }

    public Sprite GetRewardIcon() {
        /*if (isBuildingReward) {
            return DataHolder.s.GetCart(myReward).Icon;
        } else {
            return DataHolder.s.GetPowerUp(myReward).icon;
        }*/
        return DataHolder.s.GetCart(myRewardCart).Icon;
    }

    public void SetCargo(DataSaver.TrainState.CartState.CargoState cargoState) {
        myRewardCart = cargoState.cargoReward;
        myRewardArtifact = cargoState.artifactReward;
        isLeftCargo = cargoState.isLeftCargo;

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
