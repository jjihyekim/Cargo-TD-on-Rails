using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;
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
	public GameObject singleModuleRewardPrefab;
	public GameObject powerUpRewardPrefab;

	private void Start() {
		winUI.SetActive(false);
	}


	public bool isWon = false;
	
	public UnityEvent OnLevelFinished = new UnityEvent();
	
	public void MissionWon(bool isShowingPrevRewards = false) {
		isWon = true;
		PlayStateMaster.s.FinishCombat();
		EnemyWavesController.s.Cleanup();
		//EnemyHealth.winSelfDestruct?.Invoke(false);
		ShowAlert(false);

		for (int i = 0; i < scriptsToDisable.Length; i++) {
			scriptsToDisable[i].enabled = false;
		}
		
		for (int i = 0; i < gameObjectsToDisable.Length; i++) {
			gameObjectsToDisable[i].SetActive(false);
		}

		ChangeRangeShowState(false);
		
		var mySave = DataSaver.s.GetCurrentSave();

		MapController.s.FinishTravelingToStar();

		// mission rewards
		if(!isShowingPrevRewards)
			GenerateMissionRewards();
		
		
		// save our resources
		mySave.currentRun.myResources.scraps = Mathf.FloorToInt(MoneyController.s.scraps);
		mySave.currentRun.myTrain = Train.s.GetTrainState();
		
		DataSaver.s.SaveActiveGame();
		
		ShowAndGiveMissionRewards();
		cameraSwitcher.Engage();
		winUI.SetActive(true);


		if (PlayStateMaster.s.currentLevel != null)  { // if level is null that means we are getting unclaimed rewards. hence no need to send data again.
			
			//send analytics
			AnalyticsResult analyticsResult = Analytics.CustomEvent(
				"LevelWon",
				new Dictionary<string, object> {
					{ "Level", PlayStateMaster.s.currentLevel.levelName },

					{ "character", DataSaver.s.GetCurrentSave().currentRun.character.uniqueName },

					{ "buildingsBuild", ModuleHealth.buildingsBuild },
					{ "buildingsDestroyed", ModuleHealth.buildingsDestroyed },

					{ "enemiesLeftAlive", EnemyHealth.enemySpawned - EnemyHealth.enemyKilled },
					{ "winTime", SpeedController.s.currentTime },
				}
			);
			
			Debug.Log("Mission Won Analytics: " + analyticsResult);
		}
		

		SoundscapeController.s.PlayMissionWonSound();

		// MusicPlayer.s.SwapMusicTracksAndPlay(false);
		FMODMusicPlayer.s.SwapMusicTracksAndPlay(false);

		DirectControlMaster.s.DisableDirectControl();
		
		OnLevelFinished?.Invoke();
	}

	void ChangeRangeShowState(bool state) {
		var ranges = Train.s.GetComponentsInChildren<RangeVisualizer>();

		for (int i = 0; i < ranges.Length; i++) {
			ranges[i].ChangeVisualizerEdgeShowState(state);
		}
	}

	public void ShowUnclaimedRewards() {
		Invoke(nameof(DelayedShowRewards), 0.05f);
	}

	void DelayedShowRewards() {
		MissionWon(true);
	}


	public int maxTrainLengthForGettingCartReward = 5;
	void GenerateMissionRewards() {
		var mySave = DataSaver.s.GetCurrentSave();
		var playerStar = mySave.currentRun.map.GetPlayerStar();
		
		//var rewardMoney = playerStar.rewardMoney;
		var allCargo = Train.s.GetComponentsInChildren<CargoModule>();
		foreach (var cargo in allCargo) {
			var cargoObj = Instantiate(cargoDeliveredPrefab, cargoDeliveredParent);
			cargoObj.GetComponent<MiniGUI_DeliveredCargo>().SetUp(cargo);
			//rewardMoney += cargoObj.GetComponent<MiniGUI_DeliveredCargo>().SetUp(cargo);
			if(cargo.IsBuildingReward()){
			
				mySave.currentRun.unclaimedRewards.Add($"b{cargo.GetReward()}");
			}else {
				mySave.currentRun.unclaimedRewards.Add($"p{cargo.GetReward()}");
			}
			cargo.CargoSold();
		}
		
		Train.s.SaveTrainState();

		cargoText.text = $"Cargo Delivered {allCargo.Length}";

		// mission rewards, must do after the stuff above
		//mySave.currentRun.unclaimedRewards.Add($"s{Random.Range(50,60)}");
		//mySave.currentRun.unclaimedRewards.Add($"m{rewardMoney}");

		/*string[] upgradeRewards;
		if (playerStar.isBoss) {
			upgradeRewards = UpgradesController.s.GetRandomBossRewards();
		} else {
			upgradeRewards = UpgradesController.s.GetRandomLevelRewards();
		}

		var upgradesString = string.Join(",", upgradeRewards);
		mySave.currentRun.unclaimedRewards.Add($"u{upgradesString}");*/
		
		if(playerStar.rewardCart > 0 || Train.s.cartCount < maxTrainLengthForGettingCartReward)
			mySave.currentRun.unclaimedRewards.Add($"c{1}");
		
		//mySave.currentRun.unclaimedRewards.Add($"p{DataHolder.s.powerUps[Random.Range(0, DataHolder.s.powerUps.Length)].name}");//power up -> boost

		mySave.xpProgress.xp += 1;
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
						//Instantiate(scrapRewardPrefab, rewardsParent).GetComponent<MiniGUI_ScrapsReward>().SetUpReward(scrap, i);
						unclaimedRewardCount += 1;
					} else {
						Debug.LogError($"Can't parse reward: {cur}");
					}
					break;
				case 'u':
					var upgradeNames = cur.Substring(1).Split(',');
					//Instantiate(upgradeRewardPrefab, rewardsParent).GetComponent<MiniGUI_BuildingReward>().SetUpReward(upgradeNames, i);
					unclaimedRewardCount += 1;
					
					break;
				case 'c':
					if (int.TryParse(cur.Substring(1), out var cartCount)) {
						//Instantiate(cartRewardPrefab, rewardsParent).GetComponent<MiniGUI_CartReward>().SetUpReward(cartCount, i);
						unclaimedRewardCount += 1;
					} else {
						Debug.LogError($"Can't parse reward: {cur}");
					}
					break;
				
				case 'p':
					var powerUpName = cur.Substring(1);
					//Instantiate(powerUpRewardPrefab, rewardsParent).GetComponent<MiniGUI_PowerUpReward>().SetUpReward(DataHolder.s.GetPowerUp(powerUpName), i);
					break;
				
				case 'r':
					// Redeemed. Don't do anything.
					break;
				case 'b':
					var buildingName = cur.Substring(1);
					//Instantiate(singleModuleRewardPrefab, rewardsParent).GetComponent<MiniGUI_SingleBuildingReward>().SetUpReward(buildingName, i);
					break;
				default:
					Debug.LogError($"Unknown reward: {cur}");
					break;
			}
		}
	}

	public void ClearRewardWithIndex(int index) {
		var mySave = DataSaver.s.GetCurrentSave();
		mySave.currentRun.unclaimedRewards[index] = "redeemed";
		unclaimedRewardCount -= 1;
		ShowAlert( false);
	}

	/*public void UpdateUnclaimedRewards() {
		var mySave = DataSaver.s.GetCurrentSave();
		mySave.currentRun.unclaimedRewards = new List<string>();

		var scrapsReward = rewardsParent.GetComponentInChildren<MiniGUI_ScrapsReward>();
		if(scrapsReward != null)
			mySave.currentRun.unclaimedRewards.Add($"s{scrapsReward.scraps}");
		
		
		var moneyReward = rewardsParent.GetComponentInChildren<MiniGUI_MoneyReward>();
		if(moneyReward != null)
			mySave.currentRun.unclaimedRewards.Add($"m{moneyReward.money}");

		var upgradeReward = rewardsParent.GetComponentInChildren<MiniGUI_UpgradeReward>();
		if (upgradeReward != null) {
			var upgradeRewards = upgradeReward.upgrades;
			var upgradesString = string.Join(",", upgradeRewards.Select(u => u.upgradeUniqueName));
			mySave.currentRun.unclaimedRewards.Add($"u{upgradesString}");
		}


		var cartReward = rewardsParent.GetComponentInChildren<MiniGUI_CartReward>();
		if(cartReward != null)
			mySave.currentRun.unclaimedRewards.Add($"c{cartReward.cartCount}");
	}*/

	void ClearOldRewards() {
		unclaimedRewardCount = 0;
		rewardsParent.DeleteAllChildren();
		ShowAlert(false);
	}

	public void DebugRedoRewards() {
		ClearOldRewards();
		ShowAndGiveMissionRewards();
	}


	public TMP_Text continueButtonText;

	public void ShowAlert(bool status) {
		isShowingAlert = status;
		if (isShowingAlert) {
			continueButtonText.text = "Are you sure you want to skip rewards?";
		} else {
			continueButtonText.text = "Continue";
		}
	}

	public int unclaimedRewardCount = 0;
	public bool isShowingAlert = false;
	public void ContinueToClearOutOfCombat() {
		if (!isShowingAlert) {
			if (unclaimedRewardCount > 0) {
				ShowAlert(true);
				return;
			}
		}

		ClearOldRewards();
		DataSaver.s.GetCurrentSave().currentRun.unclaimedRewards = new List<string>();
		DataSaver.s.GetCurrentSave().currentRun.shopInitialized = false;
		Train.s.SaveTrainState();
		DataSaver.s.SaveActiveGame();
		
		if (DataSaver.s.GetCurrentSave().currentRun.map.GetPlayerStar().isBoss) {
			ActFinishController.s.OpenActWinUI();
		} else {
			//Cleanup();
			PlayStateMaster.s.ClearOutOfCombat();
		}
	}

	public void Cleanup() {
		isWon = false;
		for (int i = 0; i < gameObjectsToDisable.Length; i++) {
			gameObjectsToDisable[i].SetActive(false);
		}
		
		ChangeRangeShowState(true);
		cameraSwitcher.Disengage();
		winUI.SetActive(false);
		
		for (int i = 0; i < scriptsToDisable.Length; i++) {
			scriptsToDisable[i].enabled = true;
		}
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
