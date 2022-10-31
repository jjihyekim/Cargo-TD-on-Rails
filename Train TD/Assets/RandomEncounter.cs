using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


public abstract class RandomEncounter : MonoBehaviour {
	public string encounterUniqueName = "unset";

	public string encounterTitleName = "unset title";

	[Multiline(10)] 
	public string richEncounterText = "please choose";
	[Multiline(2)]
	public string[] options;


	/// <summary>
	/// return a bool array telling which options are not available due to not having enough resources
	/// Array might be smaller than input, then it is assumed option is available
	/// </summary>
	public virtual bool[] SetOptionTexts() {
		return null;
	}

	// return null to show that encounter is over
	public abstract RandomEncounter OptionPicked(int option);

	public bool hasEncounterCamPos = false;
	[ShowIf("hasEncounterCamPos")] 
	public Transform encounterCamPos;
}

