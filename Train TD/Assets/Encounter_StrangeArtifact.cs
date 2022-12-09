using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Encounter_StrangeArtifact : RandomEncounter {

	
	[Title("0 = take it, 1 = leave it, 2 = pry it open")]
	public Encounter_CompleteWithReward takeAndSuccess;

	public Encounter_CompleteWithReward takeAndExplode;

	public Encounter_Complete leaveIt;

	public Encounter_CompleteWithReward pryOpen;

	public float takeSuccessChance = 0.5f;
	

	public override RandomEncounter OptionPicked(int option) {
		switch (option) {
			case 0:
				if (Random.value < takeSuccessChance) {
					return takeAndSuccess;
				} else {
					return takeAndExplode;
				}
			case 1:
				return leaveIt;
			case 2:
				return pryOpen;
			default:
				Debug.LogError($"Unknown choice {option}");
				return null;
		}
	}
}
