using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraReference : MonoBehaviour {
    public static MainCameraReference s;

    public Camera cam;
    public Camera uiCam;
    private void Awake() {
        if (s == null) {
            s = this;
            cam = GetComponent<Camera>();
            uiCam = transform.GetChild(0).GetComponent<Camera>();
        }
    }
}
