using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationConstraint : MonoBehaviour {

    public float targetY = 90f;
    public float strength = 100f;

    private void OnEnable() {
        CameraController.s.AfterCameraPosUpdate.AddListener(_LateUpdate);
    }

    private void OnDisable() {
        CameraController.s.AfterCameraPosUpdate.RemoveListener(_LateUpdate);
    }

    private void _LateUpdate() {
        var curEuler = transform.rotation.eulerAngles;
        var targetRot = Quaternion.Euler(curEuler.x, targetY, curEuler.z);
        
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, strength *Time.deltaTime);
    }
}
