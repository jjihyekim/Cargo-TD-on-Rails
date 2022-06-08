using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITooltipDisplayer : MonoBehaviour {
    public Tooltip myTooltip;
    public RectTransform targetRect;

    public float mouseOverTimer = 1f;
    public float curTimer = 0;

    public bool showingTooltip = false;
    private void Update() {
        if (myTooltip != null) {
            Vector2 localMousePosition = targetRect.InverseTransformPoint(Input.mousePosition);
            if (targetRect.rect.Contains(localMousePosition)) {
                curTimer += Time.deltaTime;
            } else {
                curTimer = 0;
            }

            if (curTimer > mouseOverTimer) {
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
