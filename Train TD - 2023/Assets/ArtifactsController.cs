using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactsController : MonoBehaviour {

	public static ArtifactsController s;

	private void Awake() {
		s = this;
	}


	public Transform artifactsParent;
	public List<Artifact> myArtifacts = new List<Artifact>();

	public void OnDisarmArtifacts() {
		for (int i = 0; i < myArtifacts.Count; i++) {
			myArtifacts[i].GetComponent<ActivateWhenOnArtifactRow>().Disarm();
		}
	}


	public void OnArmArtifacts() {
		for (int i = 0; i < myArtifacts.Count; i++) {
			myArtifacts[i].GetComponent<ActivateWhenOnArtifactRow>().Arm();
		}
	}
}




public abstract class ActivateWhenOnArtifactRow : MonoBehaviour {

	public bool isArmed = false;

	public void Arm() {
		if (isArmed == false) {
			isArmed = true;
            
			_Arm();
		}
	}

	protected abstract void _Arm();
    

	public void Disarm() {
		if (isArmed == true) {
			isArmed = false;

			_Disarm();
		}
	}
    
    
	protected abstract void _Disarm();
	
}
