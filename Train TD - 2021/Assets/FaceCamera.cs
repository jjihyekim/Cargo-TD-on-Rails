using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour {
	private Transform camera;
    void Start() {
	    camera = MainCameraReference.s.cam.transform;
    }

    // Update is called once per frame
    void Update() {
	    transform.LookAt(camera);
    }
}
