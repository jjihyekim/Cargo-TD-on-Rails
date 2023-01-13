using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_EncounterOption : MonoBehaviour {
	public TMP_Text myText;
	
	public EncounterOption myOption;

	public Transform requirementOrRewardParent;
	public void SetUp(EncounterOption _myOption) {
		myOption = _myOption;
		var text = _myOption.text;
		if (myOption.nextNode == null) {
			text += "<i><color=#0000FF> Continue...</i></color>";
		}
		myText.text = text;
		
		bool canUseOption = true;

		var requirements = myOption.GetComponentsInChildren<EncounterRequirement>();
		for (int i = 0; i < requirements.Length; i++) {
			var req = Instantiate(EncounterController.s.requirementOrRewardPrefab, requirementOrRewardParent).GetComponent<MiniGUI_EncounterRequirementOrRewardDisplay>();
			canUseOption = canUseOption && req.SetUp(requirements[i]);
		}
		
		
		var rewards = myOption.GetComponentsInChildren<EncounterReward>();
		for (int i = 0; i < rewards.Length; i++) {
			var req = Instantiate(EncounterController.s.requirementOrRewardPrefab, requirementOrRewardParent).GetComponent<MiniGUI_EncounterRequirementOrRewardDisplay>();
			req.SetUp(rewards[i]);
		}
	}


	public void PickOption() {
		var requirements = myOption.GetComponentsInChildren<EncounterRequirement>();
		for (int i = 0; i < requirements.Length; i++) {
			requirements[i].UseResource();
		}
		
		var rewards = myOption.GetComponentsInChildren<EncounterReward>();
		for (int i = 0; i < rewards.Length; i++) {
			rewards[i].GainReward();
		}

		EncounterController.s.MoveToNewNode(myOption.nextNode);
	}
}
