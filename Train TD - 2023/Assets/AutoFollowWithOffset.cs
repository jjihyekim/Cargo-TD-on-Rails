using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoFollowWithOffset : MonoBehaviour {
    [Header("Only follows Z")] 
    public Transform target;
    public Vector3 offset;

    void Update() {
        transform.position = new Vector3(0, 0, target.position.z) + offset;
    }
}
