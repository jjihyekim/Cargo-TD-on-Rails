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
	public GameObject mapDisabledDuringBattleOverlay;
	
	public void BackToProfileSelection() {
		starterUI.SetActive(false);
		SceneLoader.s.OpenProfileScreen();
		MusicPlayer.s.SwapMusicTracksAndPlay(false);
	}

	public void OpenStarterUI() {
		starterUI.SetActive(true);
		CharacterSelector.s.CheckAndShowCharSelectionScreen();
		
		if (DataSaver.s.GetCurrentSave().isInARun) {
			Train.s.DrawTrain(DataSaver.s.GetCurrentSave().currentRun.myTrain);
			
			if (DataSaver.s.GetCurrentSave().currentRun.unclaimedRewards.Count > 0) {
				StartLevel(false);
				starterUI.SetActive(false);
				MissionWinFinisher.s.ShowUnclaimedRewards();
			} else {
				OnEnteredStarterUI?.Invoke();
			}
		} 
	}

	public void SetStarterUIStatus(bool status) {
		starterUI.SetActive(status);
	}

	public void StartLevel(bool legitStart = true) {
		if (levelSelected) {
			var playerStar = DataSaver.s.GetCurrentSave().currentRun.map.GetPlayerStar();
			starterUI.SetActive(false);

			mapOpenButton.interactable = false;
			mapDisabledDuringBattleOverlay.SetActive(true);

			if (!playerStar.outgoingConnectionLevels[selectedLevelIndex].isEncounter) {
				ClearStaticTrackers();

				gameUI.SetActive(true);
				SceneLoader.s.StartLevel();
				Train.s.LevelStateChanged();
				
				OnLevelStarted?.Invoke();

				if (legitStart) {
					RangeVisualizer.SetAllRangeVisualiserState(true);

					SoundscapeController.s.PlayMissionStartSound();
					MusicPlayer.s.SwapMusicTracksAndPlay(true);
				}
			} else {
				SceneLoader.s.FinishLevel();
				EncounterController.s.EngageEncounter(playerStar.outgoingConnectionLevels[selectedLevelIndex]);
			}
		}
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
		
		SceneLoader.s.SetCurrentLevel(playerStar.outgoingConnectionLevels[selectedLevelIndex]);
		missionDistance.text = "Mission Length: " + playerStar.outgoingConnectionLevels[selectedLevelIndex].missionDistance;
		levelSelected = true;
		OnLevelChanged?.Invoke();
	}

	public void SelectLevelAndStart(LevelData data) {
		SceneLoader.s.SetCurrentLevel(data);
		missionDistance.text = "Mission Length: " + data.missionDistance;
		levelSelected = true;
		OnLevelChanged?.Invoke();

		starterUI.SetActive(false);

		ClearStaticTrackers();

		gameUI.SetActive(true);
		SceneLoader.s.StartLevel();
		Train.s.LevelStateChanged();

		OnLevelStarted?.Invoke();

		RangeVisualizer.SetAllRangeVisualiserState(true);

		SoundscapeController.s.PlayMissionStartSound();
		MusicPlayer.s.SwapMusicTracksAndPlay(true);
	}


	public void QuickStart() {
		var playerStar = DataSaver.s.GetCurrentSave().currentRun.map.GetPlayerStar();
		var targetStar = DataSaver.s.GetCurrentSave().currentRun.map.GetStarWithName(playerStar.outgoingConnections[0]);
		SelectLevelAndStart(targetStar);
	}
}
