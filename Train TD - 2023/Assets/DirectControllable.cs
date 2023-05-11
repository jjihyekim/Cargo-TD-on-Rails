using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectControllable : MonoBehaviour {

	public Transform cameraParent;
	
	public enum DirectControlMode {
		Gun, LockOn
	}

	public DirectControlMode myMode;


	public Transform GetDirectControlTransform() {
		return cameraParent;
	}
}
