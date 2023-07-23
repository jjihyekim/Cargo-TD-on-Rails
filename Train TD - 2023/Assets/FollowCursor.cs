using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FollowCursor : MonoBehaviour
{
    
    RectTransform UIRect;
    
    private Canvas _canvas;
    private RectTransform CanvasRect;

    public float edgeGive = 10f;
    private void Awake() {
        _canvas = GetComponentInParent<Canvas>();
        CanvasRect = _canvas.GetComponent<RectTransform>();
        UIRect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 screenPoint = Vector3.zero;
        if (SettingsController.GamepadMode()) {
            screenPoint = MainCameraReference.s.cam.WorldToViewportPoint(GamepadControlsHelper.s.GetTooltipPosition());
        } else {
            screenPoint = (Vector3)Mouse.current.position.ReadValue();
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
