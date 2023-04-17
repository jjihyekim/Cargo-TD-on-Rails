using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSnap : MonoBehaviour
{
	private void Start() {
		MainCameraReference.s.cam.transform.position = transform.position;
		MainCameraReference.s.cam.transform.rotation = transform.rotation;
	}
}
