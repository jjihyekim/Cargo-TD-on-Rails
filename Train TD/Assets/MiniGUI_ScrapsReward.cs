using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_ScrapsReward : MonoBehaviour {

	public TMP_Text amount;
	public int scraps;
	
	public void SetUpReward(int _scraps) {
		scraps = _scraps;
		amount.text = $"{scraps} Scraps";
	}


	public void GetRewards() {
		DataSaver.s.GetCurrentSave().currentRun.scraps += scraps;
		DataSaver.s.SaveActiveGame();
		Destroy(gameObject);
	}
}
