using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class OverlayCamsReference : MonoBehaviour {
    public static OverlayCamsReference s;

    private void Awake() {
        s = this;
    }

    public Camera uiCam;
    public Camera fancy2duiCam;
    public Camera tooltipCam;

    
    public const float planeDistance = 1.5f;

    private void Start() {
        var camData = MainCameraReference.s.cam.GetUniversalAdditionalCameraData();
        if (!camData.cameraStack.Contains(uiCam)) {
            camData.cameraStack.Add(uiCam);
            camData.cameraStack.Add(fancy2duiCam);
            camData.cameraStack.Add(tooltipCam);
        }
    }
}
