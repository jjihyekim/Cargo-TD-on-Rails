using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

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
	
	
	public Image[] speedStarsImages;
	public Image[] cargoStarsImages;
	
	public Color starActiveColor = Color.white;
	public Color starLostColor = Color.grey;

	public TMP_Text cargoText;
	public TMP_Text timeText;

	public CameraSwitcher cameraSwitcher;

	public TMP_Text worstTime;
	public TMP_Text medTime;
	public TMP_Text bestTime;

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

		var myMission = DataSaver.s.GetCurrentSave().GetCurrentMission();
		

		
		// Analytics calculations (need to be done done mission rewards are given)
		bool isWonBefore = myMission.isWon;
		var prevStarCount = (myMission.isWon ? 1 : 0) + myMission.speedStars + myMission.cargoStars;
		var curStarCount = StarController.s.speedStars + StarController.s.cargoStars + 1;
		bool scoreImprovementSuccess = curStarCount > prevStarCount;
		
		
		
		// mission rewards
		ShowAndGiveMissionRewards(myMission);
		cameraSwitcher.Engage();
		winUI.SetActive(true);
		
		
		
		//send analytics
		AnalyticsResult analyticsResult = Analytics.CustomEvent(
			"LevelWon",
			new Dictionary<string, object> {
				{ "Level", SceneLoader.s.currentLevel.levelName },
				{ "cargoStars", StarController.s.cargoStars },
				{ "speedStars", StarController.s.speedStars },
				
				{"finishedBefore", isWonBefore},
				{"scoreImprovementSucces", scoreImprovementSuccess},
				
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
				ranges[i].ChangeVisualizerStatus(false);
			}
		}
	}

	void ShowAndGiveMissionRewards(DataSaver.MissionStats myMission) {
		var totalRewards = SceneLoader.s.currentLevel.missionRewardMoney;
		foreach (var cargo in Train.s.GetComponentsInChildren<CargoModule>()) {
			var cargoObj = Instantiate(cargoDeliveredPrefab, cargoDeliveredParent);
			totalRewards += cargoObj.GetComponent<MiniGUI_DeliveredCargo>().SetUp(cargo);
		}

		var mySave = DataSaver.s.GetCurrentSave();
		mySave.money += totalRewards;

		var starCount = StarController.s.speedStars + StarController.s.cargoStars + 1;
		SetStarAmount(speedStarsImages, StarController.s.speedStars);
		SetStarAmount(cargoStarsImages, StarController.s.cargoStars);
		
		myMission.isWon = true;
		myMission.speedStars = Mathf.Max(myMission.speedStars,StarController.s.speedStars);
		myMission.cargoStars = Mathf.Max(myMission.cargoStars,StarController.s.cargoStars);
		myMission.bestCargoCount = Mathf.Max(myMission.bestCargoCount,CargoController.s.aliveCargo);
		myMission.bestTime = Mathf.Min(myMission.bestTime,SpeedController.s.currentTime);
		
		cargoText.text = $"Cargo: {CargoController.s.aliveCargo}/{CargoController.s.totalCargo}";
		timeText.text = $"Time: {SpeedController.s.GetCurrentTime()}";

		worstTime.text = SpeedController.s.GetWorstTime();
		medTime.text = SpeedController.s.GetMedTime();
		bestTime.text = SpeedController.s.GetBestTime();
		
		
		DataSaver.s.SaveActiveGame();
	}


	public void ContinueToStarterMenu() {
		SceneLoader.s.BackToMenu();
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
