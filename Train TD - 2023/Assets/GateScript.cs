using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    public Tooltip myTooltip;
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
        TooltipsMaster.s.ShowTooltip(myTooltip);
    }

    [HideInInspector]
    public UnityEvent OnCanLeaveAndPressLeave;

    public void _OnMouseUpAsButton() {
        if (canGo) {
            OnCanLeaveAndPressLeave?.Invoke();
            _OnMouseExit();
            mouseOver = true;
        }
    }


    public void SetCanGoStatus(bool status, Tooltip tooltip) {
        canGo = status;
        readyToGoEffects.SetActive(canGo);
        downCurrentSpeed = 0;
        _outline.OutlineColor = canGo ? canGoColor : cannotGoColor;
        myTooltip = tooltip;
        enabled = true;
    }

    
    private void Update() {
        if (PlayStateMaster.s.isCombatInProgress()) {
            enabled = false;
            TooltipsMaster.s.HideTooltip();
        }
        
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
