using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MiniGUI_PlayerActionInfo : MonoBehaviour {


    public GameObject panel;
    public GameObject reloadInfoAmmo;
    public GameObject reloadInfoFuel;
    public GameObject repairInfo;
    public GameObject shopRepairInfo;
    public GameObject shopSellInfo;
    public GameObject moveInfo;
    public GameObject directControlInfo;
    public GameObject powerUpInfo;
    public TMP_Text powerUpInfoText;

    public TMP_Text amountText;

    public Color regularColor = Color.black;
    public Color cannotAffordColor = Color.red;

    private void Start() {
        Deactivate();
    }

    public ResourceTypes myType;
    public int cost;

    void Update() {
        var mousePos = Mouse.current.position.ReadValue();
        var screenPoint = new Vector3(mousePos.x, mousePos.y, OverlayCamsReference.planeDistance);
        transform.position = OverlayCamsReference.s.uiCam.ScreenToWorldPoint(screenPoint);

        if (cost > 0) {
            bool canAfford = MoneyController.s.HasResource(myType, cost);
            amountText.color = canAfford ? regularColor : cannotAffordColor;
        } else {
            amountText.color = regularColor;
        }
    }


    public void ShowReloadCost(int _cost, ResourceTypes type) {
        cost = _cost;
        panel.SetActive(true);
        switch (type) {
            case ResourceTypes.ammo:
                reloadInfoAmmo.SetActive(true);
                break;
            case ResourceTypes.fuel:
                reloadInfoFuel.SetActive(true);
                break;
        }
        amountText.text = cost.ToString();
        myType = type;
        
        Update();
    }

    public void Deactivate() {
        cost = -1;
        powerUpInfoText.text = "";
        panel.SetActive(false);
        reloadInfoAmmo.SetActive(false);
        reloadInfoFuel.SetActive(false);
        repairInfo.SetActive(false);
        directControlInfo.SetActive(false);
        shopRepairInfo.SetActive(false);
        moveInfo.SetActive(false);
        shopSellInfo.SetActive(false);
        powerUpInfo.SetActive(false);
    }

    public void ShowRepairInfo(int _cost, ResourceTypes type) {
        cost = _cost;
        panel.SetActive(true);
        repairInfo.SetActive(true);
        amountText.text = $"{cost}/s";
        myType = type;
        
        Update();
    }
    
    public void ShowShopRepairInfo(int _cost, ResourceTypes type) {
        cost = _cost;
        panel.SetActive(true);
        shopRepairInfo.SetActive(true);
        amountText.text = $"{cost}";
        myType = type;
        
        Update();
    }

    public void ShowDirectControlInfo() {
        panel.SetActive(true);
        directControlInfo.SetActive(true);
        amountText.text = "";
        
        Update();
    }

    public void ShowMoveInfo() {
        panel.SetActive(true);
        moveInfo.SetActive(true);
        amountText.text = "";
        
        Update();
    }
    
    public void ShowShopSellInfo(int _cost) {
        cost = _cost;
        panel.SetActive(true);
        shopSellInfo.SetActive(true);
        amountText.text = $"+{-cost}";
        
        Update();
    }

    public void ShowPowerUpInfo(string description) {
        panel.SetActive(true);
        powerUpInfo.SetActive(true);
        powerUpInfoText.text = description;
        
        Update();
    }
}
