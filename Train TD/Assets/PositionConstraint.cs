using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionConstraint : MonoBehaviour {
    public float xRange = 0.5f;
    public float yRange = 10f;
    public float zRange = 0.5f;

    public float xzStrength = 10f;
    public float yStrength = 2f;
    
    private void LateUpdate() {
        var lerpXZ = Vector3.Lerp(transform.localPosition, Vector3.zero, xzStrength * Time.deltaTime);
        var lerpY = Vector3.Lerp(transform.localPosition, Vector3.zero, yStrength * Time.deltaTime);

        var targetPos = transform.localPosition;

        if (Mathf.Abs(targetPos.x) > xRange) {
            targetPos.x = lerpXZ.x;
        }

        if (Mathf.Abs(targetPos.z) > zRange) {
            targetPos.z = lerpXZ.z;
        }

        if (Mathf.Abs(targetPos.y) > yRange) {
            targetPos.y = lerpY.y;
        }
        
        transform.localPosition = targetPos;
    }
}
