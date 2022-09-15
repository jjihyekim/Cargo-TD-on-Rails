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
    public TMP_Text tooltipText;

    public Vector3 offset = new Vector3(-5, -5, 0);
    
    private void Awake() {
        s = this;
    }

    public bool isTooltipActive = false;
    private Canvas _canvas;

    private void Start() {
        _canvas = tooltipWindow.GetComponentInParent<Canvas>();
        HideTooltip();
    }

    string AddBreaks(string source) {
        /*if (source.Length < 50) {
            return source;
        }else if (source.Length < 150) {
            
        }*/
        return source;
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
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform, Mouse.current.position.ReadValue(), _canvas.worldCamera, out var pos);
            tooltipWindow.transform.position = _canvas.transform.TransformPoint(pos) + offset;
        }
    }
}
