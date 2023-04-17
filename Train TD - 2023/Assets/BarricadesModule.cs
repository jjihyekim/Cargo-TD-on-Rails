using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadesModule : MonoBehaviour, IActiveDuringCombat
{
	public void ActivateForCombat() {
		Debug.LogError("module not implemented");
		this.enabled = true;
	}

	public void Disable() {
		this.enabled = false;
	}
}
