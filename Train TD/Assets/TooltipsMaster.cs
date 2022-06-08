using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines.Primitives;
using UnityEngine;

public class TooltipsMaster : MonoBehaviour {

    public static TooltipsMaster s;

    private void Awake() {
        s = this;
    }

    public void ShowTooltip(Tooltip tooltip) {
        
    }

    public void HideTooltip() {
        
    }
}
