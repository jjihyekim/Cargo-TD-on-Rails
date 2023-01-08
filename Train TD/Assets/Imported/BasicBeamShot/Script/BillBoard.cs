using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class BillBoard : MonoBehaviour
{
	private Camera LookAtCam;
	
	public float RandomRotate = 0;
	private float RndRotate;
	Transform this_t_;
	
	void Awake() {
		this_t_ = this.transform;
		RndRotate = Random.value*RandomRotate;
	}

	private void Start() {
		LookAtCam = MainCameraReference.s.cam;
		if ( LookAtCam == null ) this.enabled = false;
	}

	void Update() {
		Transform cam_t = LookAtCam.transform;
		
		Vector3 vec = cam_t.position - this_t_.position;
		vec.x = vec.z = 0.0f;
		this_t_.LookAt(cam_t.position - vec); 
		this_t_.Rotate(Vector3.forward,RndRotate);
	}
}