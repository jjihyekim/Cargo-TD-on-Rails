using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Encounter_CrashedPeople : RandomEncounter
{
	
	// 0 help
	// 1 don't help
	// 2 steal their supplies
	[Title("0 = help, 1 = don't help, 2 = steal their supplies")]
	public Encounter_Complete helpButNoReward;
	public Encounter_CompleteWithReward helpWithReward;

	public Encounter_Complete dontHelp;

	public Encounter_CompleteWithReward steal;

	public float helpSuccessChance = 0.5f;

	public int amountAsk = 50;
	public float randomVariation = 0.2f;

	private bool isFuelAsk = false;

	public override bool[] SetOptionTexts() {
		amountAsk = (int)(amountAsk *  1 + Random.Range(-randomVariation, randomVariation));

		options[0] = options[0].Replace("[amount]", $"{amountAsk}");

		var canDoIt = false;
		isFuelAsk = Random.value > 0.5f;
		if (isFuelAsk) {
			options[0] = options[0].Replace("[type]", $"fuel");
			options[1] = options[1].Replace("[type]", $"fuel");
			options[0] = options[0].Replace("[verb]", $"fuel");
			canDoIt = DataSaver.s.GetCurrentSave().currentRun.myResources.fuel >= amountAsk;
			
		} else {
			options[0] = options[0].Replace("[type]", $"scrap");
			options[1] = options[1].Replace("[type]", $"scrap");
			options[0] = options[0].Replace("[verb]", $"repair");
			canDoIt = DataSaver.s.GetCurrentSave().currentRun.myResources.scraps >= amountAsk;
			
		}

		return new[] { canDoIt };
	}

	public override RandomEncounter OptionPicked(int option) {
		switch (option) {
			case 0:
				if (isFuelAsk) {
					DataSaver.s.GetCurrentSave().currentRun.myResources.AddResource(-amountAsk,DataSaver.RunResources.Types.fuel);
				} else {
					DataSaver.s.GetCurrentSave().currentRun.myResources.AddResource(-amountAsk,DataSaver.RunResources.Types.scraps);
				}
				
				if (Random.value < helpSuccessChance) {
					return helpWithReward;
				} else {
					return helpButNoReward;
				}
			case 1:
				return dontHelp;
			case 2:
				return steal;
			default:
				Debug.LogError($"Unknown choice {option}");
				return null;
		}
	}
	
	
}
