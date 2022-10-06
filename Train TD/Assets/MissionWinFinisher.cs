using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	public void MissionWon(bool isShowingPrevRewards = false) {
		SceneLoader.s.FinishLevel();
		EnemyHealth.winSelfDestruct = true;
		
		for (int i = 0; i < scriptsToDisable.Length; i++) {
			scriptsToDisable[i].enabled = false;
		}
		
		for (int i = 0; i < gameObjectsToDisable.Length; i++) {
			gameObjectsToDisable[i].SetActive(false);
		}

		DeactiveRangeShows();
		
		var mySave = DataSaver.s.GetCurrentSave();
		

		// mission rewards
		if(!isShowingPrevRewards)
			GenerateMissionRewards();
		
		
		// save our resources
		mySave.currentRun.scraps = MoneyController.s.scraps;
		mySave.currentRun.myTrain = Train.s.GetTrainState();
		mySave.currentRun.fuel = (int)SpeedController.s.fuel;
		
		DataSaver.s.SaveActiveGame();
		
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

	public void ShowUnclaimedRewards() {
		Invoke(nameof(DelayedShowRewards), 0.05f);
	}

	void DelayedShowRewards() {
		MissionWon(true);
	}

	void GenerateMissionRewards() {
		var rewardMoney = 0;
		var allCargo = Train.s.GetComponentsInChildren<CargoModule>();
		foreach (var cargo in allCargo) {
			var cargoObj = Instantiate(cargoDeliveredPrefab, cargoDeliveredParent);
			rewardMoney += cargoObj.GetComponent<MiniGUI_DeliveredCargo>().SetUp(cargo);
			cargo.CargoSold();
		}

		cargoText.text = $"Cargo Delivered {allCargo.Length}";

		var mySave = DataSaver.s.GetCurrentSave();

		// mission rewards, must do after the stuff above
		mySave.currentRun.unclaimedRewards.Add($"s{Random.Range(50,60)}");
		mySave.currentRun.unclaimedRewards.Add($"m{rewardMoney}");
		
		var upgradeRewards = UpgradesController.s.GetRandomLevelRewards();
		var upgradesString = string.Join(",", upgradeRewards.Select(u => u.upgradeUniqueName));
		mySave.currentRun.unclaimedRewards.Add($"u{upgradesString}");
		
		if(mySave.currentRun.map.GetPlayerStar().rewardCart > 0)
			mySave.currentRun.unclaimedRewards.Add($"c{mySave.currentRun.map.GetPlayerStar().rewardCart}");
	}

	void ShowAndGiveMissionRewards() {
		ClearOldRewards();
		//var rewardMoney = SceneLoader.s.currentLevel.missionRewardMoney;
		var mySave = DataSaver.s.GetCurrentSave();

		for (int i = 0; i < mySave.currentRun.unclaimedRewards.Count; i++) {
			var cur = mySave.currentRun.unclaimedRewards[i];

			switch (cur[0]) {
				case 's':
					if (int.TryParse(cur.Substring(1), out var scrap)) {
						Instantiate(scrapRewardPrefab, rewardsParent).GetComponent<MiniGUI_ScrapsReward>().SetUpReward(scrap);
					} else {
						Debug.LogError($"Can't parse reward: {cur}");
					}
					break;
				case 'm':
					if (int.TryParse(cur.Substring(1), out var money)) {
						Instantiate(moneyRewardPrefab, rewardsParent).GetComponent<MiniGUI_MoneyReward>().SetUpReward(money);
					} else {
						Debug.LogError($"Can't parse reward: {cur}");
					}
					break;
				case 'u':
					var upgradeNames = cur.Substring(1).Split(',');
					List<Upgrade> upgradeRewards = new List<Upgrade>();
					for (int j = 0; j < upgradeNames.Length; j++) {
						upgradeRewards.Add(UpgradesController.s.GetUpgrade(upgradeNames[j]));
					}
					
					Instantiate(upgradeRewardPrefab, rewardsParent).GetComponent<MiniGUI_UpgradeReward>().SetUpReward(upgradeRewards.ToArray());
					
					break;
				case 'c':
					if (int.TryParse(cur.Substring(1), out var cartCount)) {
						Instantiate(cartRewardPrefab, rewardsParent).GetComponent<MiniGUI_CartReward>().SetUpReward(cartCount);
					} else {
						Debug.LogError($"Can't parse reward: {cur}");
					}
					
					break;
				default:
					Debug.LogError($"Unknown reward: {cur}");
					break;
			}
		}
	}

	void ClearOldRewards() {
		rewardsParent.DeleteAllChildren();
	}

	public void DebugRedoRewards() {
		ClearOldRewards();
		ShowAndGiveMissionRewards();
	}


	public void ContinueToStarterMenu() {
		EnemyHealth.winSelfDestruct = true;
		ClearOldRewards();
		DataSaver.s.GetCurrentSave().currentRun.unclaimedRewards = new List<string>();
		DataSaver.s.SaveActiveGame();
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
