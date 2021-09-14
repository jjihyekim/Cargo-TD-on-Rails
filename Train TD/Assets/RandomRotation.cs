using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotation : MonoBehaviour {
    private Vector3 rotation;

    public float speed = 0.1f;
    void Start() {
        rotation = Random.onUnitSphere;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotation*speed*Time.deltaTime);
    }
}
