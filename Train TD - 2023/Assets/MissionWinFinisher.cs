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

	public CameraSwitcher cameraSwitcher;
	
	public GateScript gateScript;

	public Tooltip deliverYourCargoFirstTooltip;
	public Tooltip getAllTheCarts;
	public Tooltip allGoodToGoTooltip;
	private void Start() {
		winUI.SetActive(false);
		gateScript.OnCanLeaveAndPressLeave.AddListener(ContinueToNextCity);
		gateScript.SetCanGoStatus(false, deliverYourCargoFirstTooltip);
	}
	
	public bool isWon = false;
	public void MissionWon(bool isShowingPrevRewards = false) {
		SpeedController.s.TravelToMissionEndDistance();
		isWon = true;
		PlayStateMaster.s.FinishCombat();
		EnemyWavesController.s.Cleanup();
		PlayerWorldInteractionController.s.canSelect = false;
		//EnemyHealth.winSelfDestruct?.Invoke(false);

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
		if (!isShowingPrevRewards) {
			UpgradesController.s.ClearCurrentShop();
			GenerateMissionRewards();
		}
		
		
		// save our resources
		mySave.currentRun.myResources.scraps = Mathf.FloorToInt(MoneyController.s.scraps);
		mySave.currentRun.myTrain = Train.s.GetTrainState();
		mySave.currentRun.isInEndRunArea = true;
		
		DataSaver.s.SaveActiveGame();
		
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
		

		if(!isShowingPrevRewards)
			SoundscapeController.s.PlayMissionWonSound();

		// MusicPlayer.s.SwapMusicTracksAndPlay(false);
		FMODMusicPlayer.s.SwapMusicTracksAndPlay(false);

		DirectControlMaster.s.DisableDirectControl();
	}

	void ChangeRangeShowState(bool state) {
		var ranges = Train.s.GetComponentsInChildren<RangeVisualizer>();

		for (int i = 0; i < ranges.Length; i++) {
			ranges[i].ChangeVisualizerEdgeShowState(state);
		}
	}

	public void ShowUnclaimedRewards() {
		MissionWon(true);
		//Invoke(nameof(DelayedShowRewards), 0.05f);
	}

	void DelayedShowRewards() {
		MissionWon(true);
	}
	
	
	void GenerateMissionRewards() {
		var mySave = DataSaver.s.GetCurrentSave();
		var playerStar = mySave.currentRun.map.GetPlayerStar();

		Train.s.SaveTrainState();
		mySave.xpProgress.xp += 1;
	}

	
	public void ContinueToClearOutOfCombat() {
		for (int i = 0; i < gameObjectsToDisable.Length; i++) {
			gameObjectsToDisable[i].SetActive(false);
		}
		
		cameraSwitcher.Disengage();
		winUI.SetActive(false);
		
		for (int i = 0; i < scriptsToDisable.Length; i++) {
			scriptsToDisable[i].enabled = true;
		}

		PlayerWorldInteractionController.s.canSelect = true;
		
		UpgradesController.s.UpdateCargoHighlights();
		
		PlayStateMaster.s.EnterMissionRewardArea();
	}

	public void ContinueToNextCity() {
		if (isWon) { // call this only once
			if (DataSaver.s.GetCurrentSave().currentRun.map.GetPlayerStar().isBoss) {
				ActFinishController.s.OpenActWinUI();
			} else {
				PlayStateMaster.s.LeaveMissionRewardArea();
			}
		}

		isWon = false;
	}

	public void CleanupWhenLeavingMissionRewardArea() {
		ContinueToClearOutOfCombat();
		
		DataSaver.s.GetCurrentSave().currentRun.shopInitialized = false;
		DataSaver.s.GetCurrentSave().currentRun.isInEndRunArea = false;
		Train.s.SaveTrainState();
		DataSaver.s.SaveActiveGame();
	}
	
	
	public void SetCanGo() {
		gateScript.SetCanGoStatus(true, allGoodToGoTooltip);
	}

	public void SetCannotGo(bool becauseOfDelivery) {
		if (becauseOfDelivery) {
			gateScript.SetCanGoStatus(false, deliverYourCargoFirstTooltip);
		} else {
			gateScript.SetCanGoStatus(false, getAllTheCarts);
		}
	}
}
