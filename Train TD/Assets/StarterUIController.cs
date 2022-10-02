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

	public UnityEvent OnLevelChanged = new UnityEvent();
	public UnityEvent OnLevelStarted = new UnityEvent();


	public GameObject starterUI;
	public GameObject gameUI;
	
	public void BackToProfileSelection() {
		starterUI.SetActive(false);
		SceneLoader.s.OpenProfileScreen();
	}

	public void OpenStarterUI() {
		starterUI.SetActive(true);
		if (DataSaver.s.GetCurrentSave().currentRun.unclaimedRewards.Count > 0) {
			SelectLevelAndStart(DataSaver.s.GetCurrentSave().currentRun.map.GetPlayerStar().level);
			MissionWinFinisher.s.ShowUnclaimedRewards();
		}
	}

	void StartLevel() {
		ClearStaticTrackers();
		
		starterUI.SetActive(false);
		gameUI.SetActive(true);
		SceneLoader.s.StartLevel();
		OnLevelStarted?.Invoke();
		Train.s.LevelStateChanged();
	}

	void ClearStaticTrackers() {
		ModuleHealth.buildingsBuild = 0;
		ModuleHealth.buildingsDestroyed = 0;
		EnemyHealth.enemySpawned = 0;
		EnemyHealth.enemyKilled = 0;
		PlayerBuildingController.s.currentLevelStats = new Dictionary<string, PlayerBuildingController.BuildingData>();
	}

	public void SelectLevelAndStart(LevelData data) {
		SceneLoader.s.SetCurrentLevel(data);
		missionDistance.text = "Mission Length: " + data.missionDistance;
		OnLevelChanged?.Invoke();
		StartLevel();

		RangeVisualizer.SetAllRangeVisualiserState(true);
	}


	
	public void QuickStart() {
		SelectLevelAndStart(allLevels[0]);
	}
}
