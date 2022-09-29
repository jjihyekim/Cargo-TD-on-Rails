using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAddScreenSpaceCamera : MonoBehaviour
{
	
	
	void Start() {
		var canvas = GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
		canvas.planeDistance = 2;
		canvas.worldCamera = MainCameraReference.s.uiCam;
	}
}