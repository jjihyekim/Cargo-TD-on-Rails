using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Mini_Smithery : MonoBehaviour {
	
	public GameObject smitheryPoof;
	public Transform poofLoc;
	
	public UnityEvent OnStuffCollided = new UnityEvent();
	public void StuffCollided() {
		OnStuffCollided?.Invoke();
		Instantiate(smitheryPoof, poofLoc);
	}


	public void EngageAnim() {
		GetComponent<Animator>().SetTrigger("Engage");
	}

}
