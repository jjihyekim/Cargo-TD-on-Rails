using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_ScrapsReward : MonoBehaviour {

	public TMP_Text amount;
	public int scraps;
	private int rewardIndex;
	
	public void SetUpReward(int _scraps, int _rewardIndex) {
		scraps = _scraps;
		rewardIndex = _rewardIndex;
		amount.text = $"{scraps} Scraps";
	}


	public void GetRewards() {
		MissionWinFinisher.s.ClearRewardWithIndex(rewardIndex);
		MoneyController.s.ModifyResource(ResourceTypes.scraps, scraps);
		Destroy(gameObject);
	}
}
