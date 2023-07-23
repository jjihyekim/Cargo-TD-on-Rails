using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TooltipsMaster : MonoBehaviour {

    public static TooltipsMaster s;

    public GameObject tooltipWindow;
    RectTransform UIRect;
    public TMP_Text tooltipText;

    public Vector3 offset = new Vector3(0.1f, -0.1f, 0);
    public float edgeGive = 10f;

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
        if(tooltip == null)
            return;
        
        tooltipText.text = AddBreaks(tooltip.text);
        
        // yeet it off screen before our calculation
        UIRect.transform.position = new Vector3(-1000, -1000, 0);
        
        tooltipWindow.SetActive(true);

        // fix the auto size fitting
        Invoke(nameof(OneFrameLater), 0.01f);
    }

    void OneFrameLater() {
        Canvas.ForceUpdateCanvases();  
        tooltipText.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
        tooltipText.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;
        
        
        Invoke(nameof(AnotherFrameLater), 0.01f);
    }

    void AnotherFrameLater() {
        // then put it back where it should be
        isTooltipActive = true;
        Update();
    }

    public void HideTooltip() {
        tooltipWindow.SetActive(false);
        isTooltipActive = false;
    }
    
    private void Update() {
        if (isTooltipActive) {
            Vector3 screenPoint = Vector3.zero;
            if (SettingsController.GamepadMode()) {
                screenPoint = MainCameraReference.s.cam.WorldToViewportPoint(GamepadControlsHelper.s.GetTooltipPosition());
            } else {
                screenPoint = (Vector3)Mouse.current.position.ReadValue() + offset;
                screenPoint = OverlayCamsReference.s.uiCam.ScreenToViewportPoint(screenPoint);
            }
            
            // restrict to screen:
            //now you can set the position of the ui element
            var halfWidthLimit = (CanvasRect.rect.width - (UIRect.rect.width + edgeGive)) / 2f;
            var halfHeightLimit = (CanvasRect.rect.height - (UIRect.rect.height + edgeGive)) / 2f;
            halfWidthLimit /= CanvasRect.rect.width;
            halfHeightLimit /= CanvasRect.rect.height;
            screenPoint.x = Mathf.Clamp(screenPoint.x ,
                0,
                halfWidthLimit*2
            ) ;
            screenPoint.y = Mathf.Clamp(screenPoint.y - 0.5f,
                -halfHeightLimit,
                halfHeightLimit
            ) + 0.5f;

            screenPoint.z = 1.5f; //distance of the plane from the camera

            UIRect.transform.position = OverlayCamsReference.s.uiCam.ViewportToWorldPoint(screenPoint);
        }
    }
}
