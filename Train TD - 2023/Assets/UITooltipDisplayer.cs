using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UITooltipDisplayer : MonoBehaviour {
    public Tooltip myTooltip;
    //public RectTransform targetRect;

    public float curTimer = 0;

    public bool showingTooltip = false;

    public bool doubleTimer = false;

    public GameObject enableOnHover;
    private void Update() {
        if (myTooltip != null && myTooltip.text.Length > 0) {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), mousePos, OverlayCamsReference.s.uiCam)) {
                curTimer += Time.deltaTime;
                if (PlayerWorldInteractionController.s.showDetailClick.action.WasPerformedThisFrame()) {
                    curTimer = 100000;
                }
                
                if(enableOnHover)
                    enableOnHover.SetActive(true);
                
            } else {
                curTimer = 0;
                
                if(enableOnHover)
                    enableOnHover.SetActive(false);
            }

            if (curTimer > (doubleTimer ? TooltipsMaster.tooltipShowTime * 2 : TooltipsMaster.tooltipShowTime)) {
                Show();
            } else {
                Hide();
            }
        }
    }

    private void OnDisable() {
        Hide();
    }

    void Show() {
        if (!showingTooltip) {
            TooltipsMaster.s.ShowTooltip(myTooltip);
            showingTooltip = true;
        }
    }

    void Hide() {
        if (showingTooltip) {
            TooltipsMaster.s.HideTooltip();
            showingTooltip = false;
        }
    }
}
