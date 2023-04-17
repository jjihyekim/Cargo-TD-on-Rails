using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionForce : MonoBehaviour {
    public float xPositionForceMultiplier = -2f;

    public Vector3 forceLocation = Vector3.zero;
    private void FixedUpdate() {
        GetComponent<Rigidbody>().AddForceAtPosition( xPositionForceMultiplier * transform.localPosition.x *Vector3.right, transform.TransformPoint(forceLocation));
    }
}
