using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectControllable : MonoBehaviour, IShowButtonOnCartUIDisplay {

	public Transform cameraParent;
	
	public enum DirectControlMode {
		Gun, LockOn
	}

	public DirectControlMode myMode;


	public Transform GetDirectControlTransform() {
		return cameraParent;
	}

	public Color GetColor() {
		return new Color(1f, 0, 0.9137254901960784f);
	}
}
