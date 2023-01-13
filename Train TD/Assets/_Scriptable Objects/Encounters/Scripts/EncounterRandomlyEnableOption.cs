using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterRandomlyEnableOption : MonoBehaviour
{
	public void Randomize() {
		var options = GetComponentsInChildren<EncounterOption>();

		var pick = Random.Range(0, options.Length);

		for (int i = 0; i < options.Length; i++) {
			if (i != pick) {
				options[i].optionEnabled = false;
			}
		}
	}
}
