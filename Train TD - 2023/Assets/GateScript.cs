using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateScript : MonoBehaviour {

    public GameObject readyToGoEffects;
    public GameObject readyToGoEffectsUI;

    public Transform gate;

    public Transform gateFullOpenPos;
    public Transform gateHalfOpenPos;
    public Transform gateClosePos;

    public float upMoveSpeed = 1f;
    public float downMoveGravity = 10f;
    public float downCurrentSpeed = 0;
    
    public bool mouseOver;
    public bool canGo;

    public Tooltip selectDestinationTooltip;
    public Tooltip pickUpWorldCartTooltip;
    public Tooltip fillFleaMarketTooltip;
    public Tooltip allGoodToGoTooltip;

    public Color canGoColor= Color.green;
    public Color cannotGoColor = Color.red;

    private Outline _outline;

    private void Start() {
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
    }

    public void _OnMouseEnter() {
        mouseOver = true;
        downCurrentSpeed = 0;
        _outline.enabled = true;
        Invoke(nameof(ShowTooltip), TooltipsMaster.tooltipShowTime);
    }

    public void _OnMouseExit() {
        mouseOver = false;
        _outline.enabled = false;
        CancelInvoke(nameof(ShowTooltip));
        TooltipsMaster.s.HideTooltip();
    }

    void ShowTooltip() {
        switch (ShopStateController.s.currentStatus) {
            case ShopStateController.CanStartLevelStatus.allGoodToGo:
                TooltipsMaster.s.ShowTooltip(allGoodToGoTooltip);
                break;
            case ShopStateController.CanStartLevelStatus.needToSelectDestination:
                TooltipsMaster.s.ShowTooltip(selectDestinationTooltip);
                break;
            case ShopStateController.CanStartLevelStatus.needToPickUpFreeCarts:
                TooltipsMaster.s.ShowTooltip(pickUpWorldCartTooltip);
                break;
            case ShopStateController.CanStartLevelStatus.needToPutThingInFleaMarket:
                TooltipsMaster.s.ShowTooltip(fillFleaMarketTooltip);
                break;
        }
        
    }

    public void _OnMouseUpAsButton() {
        if (canGo) {
            ShopStateController.s.StartLevel();
            _OnMouseExit();
            mouseOver = true;
        }
    }


    public void SetCanGoStatus(bool status) {
        canGo = status;
        readyToGoEffects.SetActive(canGo);
        downCurrentSpeed = 0;
        _outline.OutlineColor = canGo ? canGoColor : cannotGoColor;
    }

    
    private void Update() {
        if (canGo) {
            if (mouseOver) {
                gate.transform.position = Vector3.MoveTowards(gate.transform.position, gateFullOpenPos.position, upMoveSpeed * Time.deltaTime);
            } else {
                gate.transform.position = Vector3.MoveTowards(gate.transform.position, gateHalfOpenPos.position, upMoveSpeed * Time.deltaTime);
            }
        } else {
            gate.transform.position = Vector3.MoveTowards(gate.transform.position, gateClosePos.position, downCurrentSpeed * Time.deltaTime);
            downCurrentSpeed += downMoveGravity * Time.deltaTime;
            downCurrentSpeed = Mathf.Clamp(downCurrentSpeed, 0, 10f);
        }
    }
}
