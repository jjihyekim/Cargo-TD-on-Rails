using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationConstraint : MonoBehaviour {

    public float targetY = 90f;
    public float strength = 100f;

    private void LateUpdate() {
        var curEuler = transform.rotation.eulerAngles;
        var targetRot = Quaternion.Euler(curEuler.x, targetY, curEuler.z);
        
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, strength *Time.deltaTime);
    }
}
