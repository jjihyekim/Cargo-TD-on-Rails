using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines.Primitives;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TooltipsMaster : MonoBehaviour {

    public static TooltipsMaster s;

    public GameObject tooltipWindow;
    RectTransform UIRect;
    public TMP_Text tooltipText;

    public Vector3 offset = new Vector3(0.1f, -0.1f, 0);
    public float edgeGive = 0.1f;

    public int splitCharCount = 50;
    
    public static float tooltipShowTime = 0.5f;

    private void Awake() {
        s = this;
    }

    public bool isTooltipActive = false;
    private Canvas _canvas;
    private RectTransform CanvasRect;

    private void Start() {
        _canvas = tooltipWindow.GetComponentInParent<Canvas>();
        CanvasRect = _canvas.GetComponent<RectTransform>();
        UIRect = tooltipWindow.GetComponent<RectTransform>();
        HideTooltip();
    }

    string AddBreaks(string source) {
        if (source.Length < 50) {
            return source;
        }else {
            var split =source.Split(' ');
            var runningLength = 0;
            var result = "";
            for (int i = 0; i < split.Length; i++) {
                runningLength += split[i].Length;
                if (runningLength < splitCharCount) {
                    result += split[i] + ' ';
                } else {
                    result += split[i] + '\n';
                    runningLength = 0;
                }
            }
            return result;
        }
    }

    public void ShowTooltip(Tooltip tooltip) {
        tooltipText.text = AddBreaks(tooltip.text);
        tooltipWindow.SetActive(true);
        isTooltipActive = true;
        Update();
    }

    public void HideTooltip() {
        tooltipWindow.SetActive(false);
        isTooltipActive = false;
    }
    
    private void Update() {
        if (isTooltipActive) {
            var screenPoint = (Vector3)Mouse.current.position.ReadValue() + offset;
            screenPoint = OverlayCamsReference.s.uiCam.ScreenToViewportPoint(screenPoint);
            // restrict to screen:
            //now you can set the position of the ui element
            var halfWidthLimit = (CanvasRect.rect.width - (UIRect.rect.width*2 + edgeGive)) / 2f;
            var halfHeightLimit = (CanvasRect.rect.height - (UIRect.rect.height*2 + edgeGive)) / 2f;
            halfWidthLimit /= CanvasRect.rect.width;
            halfHeightLimit /= CanvasRect.rect.height;
            screenPoint.x = Mathf.Clamp(screenPoint.x - 0.5f,
                -halfWidthLimit,
                halfWidthLimit
            ) + 0.5f;
            screenPoint.y = Mathf.Clamp(screenPoint.y - 0.5f,
                -halfHeightLimit,
                halfHeightLimit
            ) + 0.5f;

            screenPoint.z = 1.5f; //distance of the plane from the camera

            UIRect.transform.position = OverlayCamsReference.s.uiCam.ViewportToWorldPoint(screenPoint);
        }
    }
}
