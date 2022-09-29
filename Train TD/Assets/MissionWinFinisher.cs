using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MissionWinFinisher : MonoBehaviour {
	public static MissionWinFinisher s;

	private void Awake() {
		s = this;
	}

	public MonoBehaviour[] scriptsToDisable;
	public GameObject[] gameObjectsToDisable;

	public GameObject winUI;
	public Transform cargoDeliveredParent;
	public GameObject cargoDeliveredPrefab;

	public Color starActiveColor = Color.white;
	public Color starLostColor = Color.grey;

	public TMP_Text cargoText;

	public CameraSwitcher cameraSwitcher;

	public Transform rewardsParent;
	public GameObject moneyRewardPrefab;
	public GameObject scrapRewardPrefab;
	public GameObject cartRewardPrefab;
	public GameObject upgradeRewardPrefab;
	
	private void Start() {
		winUI.SetActive(false);
	}

	public void MissionWon() {
		SceneLoader.s.FinishLevel();
		
		for (int i = 0; i < scriptsToDisable.Length; i++) {
			scriptsToDisable[i].enabled = false;
		}
		
		for (int i = 0; i < gameObjectsToDisable.Length; i++) {
			gameObjectsToDisable[i].SetActive(false);
		}

		DeactiveRangeShows();

		// mission rewards
		ShowAndGiveMissionRewards();
		cameraSwitcher.Engage();
		winUI.SetActive(true);
		
		
		//send analytics
		AnalyticsResult analyticsResult = Analytics.CustomEvent(
			"LevelWon",
			new Dictionary<string, object> {
				{ "Level", SceneLoader.s.currentLevel.levelName },
				
				{"character", DataSaver.s.GetCurrentSave().currentRun.character},
				
				{ "buildingsBuild", ModuleHealth.buildingsBuild },
				{ "buildingsDestroyed", ModuleHealth.buildingsDestroyed },
				
				{ "enemiesLeftAlive", EnemyHealth.enemySpawned - EnemyHealth.enemyKilled},
				{ "emptyTrainSlots", Train.s.GetEmptySlotCount() },
				{ "winTime", SpeedController.s.currentTime },
			}
		);
		
		Debug.Log("Mission Won Analytics: " + analyticsResult);
		
		PlayerBuildingController.s.LogCurrentLevelBuilds(true);
	}

	void DeactiveRangeShows() {
		foreach (var slot in Train.s.GetComponentsInChildren<Slot>()) {
			var ranges = slot.GetComponentsInChildren<RangeVisualizer>();

			for (int i = 0; i < ranges.Length; i++) {
				ranges[i].ChangeVisualizerEdgeShowState(false);
			}
		}
	}

	void ShowAndGiveMissionRewards() {
		//var rewardMoney = SceneLoader.s.currentLevel.missionRewardMoney;
		var rewardMoney = 0;
		var allCargo = Train.s.GetComponentsInChildren<CargoModule>();
		foreach (var cargo in allCargo) {
			var cargoObj = Instantiate(cargoDeliveredPrefab, cargoDeliveredParent);
			rewardMoney += cargoObj.GetComponent<MiniGUI_DeliveredCargo>().SetUp(cargo);
		}

		cargoText.text = $"Cargo Delivered {allCargo.Length}";

		var mySave = DataSaver.s.GetCurrentSave();
		
		//mySave.currentRun.money += totalRewards;

		mySave.currentRun.scraps = MoneyController.s.scraps;
		mySave.currentRun.myTrain = Train.s.GetTrainState();
		DataSaver.s.SaveActiveGame();
		
		
		// mission rewards, must do after the stuff above
		Instantiate(scrapRewardPrefab, rewardsParent).GetComponent<MiniGUI_ScrapsReward>().SetUpReward(Random.Range(10,20));
		Instantiate(moneyRewardPrefab, rewardsParent).GetComponent<MiniGUI_MoneyReward>().SetUpReward(rewardMoney);

		var upgradeRewards = UpgradesController.s.GetRandomLevelRewards();
		
		Instantiate(upgradeRewardPrefab, rewardsParent).GetComponent<MiniGUI_UpgradeReward>().SetUpReward(upgradeRewards);
		
		//if(mySave.currentRun.map.GetPlayerStar().isBoss)
		if(mySave.currentRun.map.GetPlayerStar().rewardCart > 0)
			Instantiate(cartRewardPrefab, rewardsParent).GetComponent<MiniGUI_CartReward>().SetUpReward(mySave.currentRun.map.GetPlayerStar().rewardCart);
	}

	void ClearOldRewards() {
		rewardsParent.DeleteAllChildren();
	}

	public void DebugRedoRewards() {
		ClearOldRewards();
		ShowAndGiveMissionRewards();
	}


	public void ContinueToStarterMenu() {
		ClearOldRewards();
		SceneLoader.s.BackToStarterMenuHardLoad();
	}
	
	void SetStarAmount(Image[] stars, int amount) {
		if(stars[0] == null || stars[1] == null)
			return;
		
		switch (amount) {
			case 2:
				stars[0].color = starActiveColor;
				stars[1].color = starActiveColor;
				break;
			case 1:
				stars[0].color = starActiveColor;
				stars[1].color = starLostColor;
				break;
			case 0:
				stars[0].color = starLostColor;
				stars[1].color = starLostColor;
				break;
		}
	}
}
