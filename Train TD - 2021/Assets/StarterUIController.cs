using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StarterUIController : MonoBehaviour {
	public static StarterUIController s;
	private void Awake() { s = this; }

	public List<LevelData> allLevels {
		get {
			return LevelDataLoader.s.allLevels;
		}
	}

	public TMP_Text missionDistance;

	public UnityEvent OnEnteredStarterUI = new UnityEvent();
	public UnityEvent OnLevelChanged = new UnityEvent();
	public UnityEvent OnLevelStarted = new UnityEvent();


	public GameObject starterUI;
	public GameObject gameUI;
	
	
	public bool levelSelected = false;
	public int selectedLevelIndex = -1;


	public Button mapOpenButton;

	public TMP_Text backToProfileOrAbandonText;
	void UpdateBackToProfileOrAbandonButton() {
		if (SceneLoader.s.isLevelInProgress) {
			backToProfileOrAbandonText.text = "Abandon Run";
		} else {
			backToProfileOrAbandonText.text = "Back to Main Menu";
		}
	}
	public void BackToProfileOrAbandon() {
		if (SceneLoader.s.isLevelInProgress) {
			Pauser.s.AbandonMission();
		} else {
			Pauser.s.Unpause();
			BackToProfileSelection();
		}
	}
	
	public void BackToProfileSelection() {
		starterUI.SetActive(false);
		if (SceneLoader.s.isLevelInProgress) {
			MissionWinFinisher.s.Cleanup();
			FirstTimeTutorialController.s.SkipTutorial();
		}
		SceneLoader.s.OpenProfileScreen();
		MusicPlayer.s.SwapMusicTracksAndPlay(false);
	}

	public void OpenStarterUI() {
		if(SceneLoader.s.isLevelInProgress)
			return;
		
		starterUI.SetActive(true);
		RangeVisualizer.SetAllRangeVisualiserState(false);
		CharacterSelector.s.CheckAndShowCharSelectionScreen();

		if (DataSaver.s.GetCurrentSave().isInARun) {
			Train.s.DrawTrainBasedOnSaveData();

			mapOpenButton.interactable = true;
			//mapDisabledDuringBattleOverlay.SetActive(false);
			
			if (DataSaver.s.GetCurrentSave().currentRun.unclaimedRewards.Count > 0) {
				StartLevel(false);
				starterUI.SetActive(false);
				MissionWinFinisher.s.ShowUnclaimedRewards();
			} else {
				OnEnteredStarterUI?.Invoke();
			}
		}


		UpdateBackToProfileOrAbandonButton();
		OnLevelChanged?.Invoke();
	}

	public void SetStarterUIStatus(bool status) {
		starterUI.SetActive(status);
	}

	public void StartLevel(bool legitStart = true) {
		if (levelSelected) {
			var currentLevel = SceneLoader.s.currentLevel;
			starterUI.SetActive(false);

			mapOpenButton.interactable = false;
			//mapDisabledDuringBattleOverlay.SetActive(true);

				ClearStaticTrackers();

				gameUI.SetActive(true);
				SceneLoader.s.StartLevel();
				Train.s.LevelStateChanged();
				
				OnLevelStarted?.Invoke();
				UpdateBackToProfileOrAbandonButton();

				if (legitStart) {
					RangeVisualizer.SetAllRangeVisualiserState(true);

					SoundscapeController.s.PlayMissionStartSound();
					MusicPlayer.s.SwapMusicTracksAndPlay(true);
					
					if(currentLevel.isBossLevel)
						MiniGUI_BossNameUI.s.ShowBossName(currentLevel.levelName);
				}
			/*} else {
				SceneLoader.s.FinishLevel();
				EncounterController.s.EngageEncounter(currentLevel);
			}*/ // stuf to do during encounters
		}
	}

	public void DebugEngageEncounter(string encounterName) {
		var playerStar = DataSaver.s.GetCurrentSave().currentRun.map.GetPlayerStar();
		starterUI.SetActive(false);

		mapOpenButton.interactable = false;
		//mapDisabledDuringBattleOverlay.SetActive(true);

		SceneLoader.s.FinishLevel();
		EncounterController.s.EngageEncounter(encounterName);
	}

	void ClearStaticTrackers() {
		ModuleHealth.buildingsBuild = 0;
		ModuleHealth.buildingsDestroyed = 0;
		EnemyHealth.enemySpawned = 0;
		EnemyHealth.enemyKilled = 0;
		PlayerBuildingController.s.currentLevelStats = new Dictionary<string, PlayerBuildingController.BuildingData>();
	}

	public void SelectLevelAndStart(StarState targetStar) {
		SelectLevel(targetStar);
		StartLevel();
	}

	public void SelectLevel(StarState targetStar) {
		var playerStar = DataSaver.s.GetCurrentSave().currentRun.map.GetPlayerStar();

		for (int i = 0; i < playerStar.outgoingConnections.Count; i++) {
			if (playerStar.outgoingConnections[i] == targetStar.starName) {
				selectedLevelIndex = i;
				break;
			}
		}

		if (selectedLevelIndex == -1) {
			Debug.LogError($"Illegal star target: {targetStar.starName}");
			return;
		}

		var level = playerStar.outgoingConnectionLevels[selectedLevelIndex];
		SceneLoader.s.SetCurrentLevel(level);
		//missionDistance.text = "Mission Length: " + level.missionDistance;
		levelSelected = true;
		OnLevelChanged?.Invoke();
	}

	public void SelectLevelAndStart_StarterUIStartOnly(ConstructedLevel data) {
		SceneLoader.s.SetCurrentLevel(data);
		//missionDistance.text = "Mission Length: " + data.missionDistance;
		levelSelected = true;
		OnLevelChanged?.Invoke();

		starterUI.SetActive(false);

		ClearStaticTrackers();

		gameUI.SetActive(true);
		SceneLoader.s.StartLevel();
		Train.s.LevelStateChanged();

		OnLevelStarted?.Invoke();
		UpdateBackToProfileOrAbandonButton();
		
		RangeVisualizer.SetAllRangeVisualiserState(true);

		SoundscapeController.s.PlayMissionStartSound();
		MusicPlayer.s.SwapMusicTracksAndPlay(true);
		
		StartLevel();
	}


	public void QuickStart() {
		if (DataSaver.s.GetCurrentSave().isInARun) {
			var playerStar = DataSaver.s.GetCurrentSave().currentRun.map.GetPlayerStar();
			var targetStar = DataSaver.s.GetCurrentSave().currentRun.map.GetStarWithName(playerStar.outgoingConnections[0]);
			SelectLevelAndStart(targetStar);
		}
	}
}
