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

	public Transform levelButtonParent;
	public GameObject levelButtonPrefab;
	
	[ReadOnly]
	public MiniGUI_LevelButton[] allLevelButtons;

	public Transform starterBuildingsButtonParent;
	public GameObject starterBuildingButtonPrefab;

	public TMP_Text missionDistance;
	
	public bool canStart = false;
	public Button startButton;

	public UnityEvent OnLevelChanged = new UnityEvent();
	public UnityEvent OnLevelStarted = new UnityEvent();


	public GameObject starterUI;
	public GameObject gameUI;


	public Transform enemyInfoParent;
	public GameObject enemyInfoPrefab;
	
	private void Start() {
		allLevelButtons = new MiniGUI_LevelButton[allLevels.Count];
		for (int i = 0; i < allLevels.Count; i++) {
			allLevelButtons[i] = Instantiate(levelButtonPrefab, levelButtonParent).GetComponent<MiniGUI_LevelButton>().SetUp(allLevels[i]);
		}
		if(SceneLoader.s.currentLevel != null && SceneLoader.s.currentLevel.isRealLevel())
			SelectLevel(SceneLoader.s.currentLevel);
	}

	void RefreshCurrentSelectedLevel() {
		for (int i = 0; i < allLevels.Count; i++) {
			if (allLevels[i] == SceneLoader.s.currentLevel) {
				allLevelButtons[i].SetSelected(true);
			} else {
				allLevelButtons[i].SetSelected(false);
			}
		}
	}

	public void BackToProfileSelection() {
		SceneLoader.s.OpenProfileScreen();
	}

	public void StartLevel() {
		ClearStaticTrackers();
		
		starterUI.SetActive(false);
		gameUI.SetActive(true);
		SceneLoader.s.StartLevel();
		OnLevelStarted?.Invoke();
	}

	void ClearStaticTrackers() {
		ModuleHealth.buildingsBuild = 0;
		ModuleHealth.buildingsDestroyed = 0;
		EnemyHealth.enemySpawned = 0;
		EnemyHealth.enemyKilled = 0;
		PlayerBuildingController.s.currentLevelStats = new Dictionary<string, PlayerBuildingController.BuildingData>();
	}

	public void SelectLevel(LevelData data) {
		SceneLoader.s.SetCurrentLevel(data);

		var childCount = starterBuildingsButtonParent.childCount;
		for (int i = childCount-1; i >= 0 ; i--) {
			Destroy(starterBuildingsButtonParent.GetChild(i).gameObject);
		}

		for (int i = 0; i < data.starterModules.Length; i++) {
			var moduleData = data.starterModules[i];
			var button = Instantiate(starterBuildingButtonPrefab, starterBuildingsButtonParent).GetComponent<MiniGUI_StarterBuildingButton>();
			button.SetUp(DataHolder.s.GetBuilding(moduleData.uniqueName), moduleData.count);
		}

		missionDistance.text = "Mission Length: " + data.missionDistance;
		UpdateCanStartStatus();
		OnLevelChanged?.Invoke();
		RefreshCurrentSelectedLevel();
		UpdateEnemies();
	}

	void UpdateEnemies() {
		var childCount = enemyInfoParent.childCount;
		for (int i = childCount-1; i >= 0 ; i--) {
			Destroy(enemyInfoParent.GetChild(i).gameObject);
		}


		var waves = SceneLoader.s.currentLevel.enemyWaves;
		for (int i = 0; i < waves.Length; i++) {
			Instantiate(enemyInfoPrefab, enemyInfoParent).GetComponent<MiniGUI_EnemyInfoPanel>().SetUp(waves[i]);
		}
	}

	public void UpdateCanStartStatus() {
		var starterButtons = starterBuildingsButtonParent.GetComponentsInChildren<MiniGUI_StarterBuildingButton>();
		canStart = true;
		for (int i = 0; i < starterButtons.Length; i++) {
			if (starterButtons[i].count > 0) {
				canStart = false;
				break;
			}
		}

		startButton.interactable = canStart;
	}

	

	public void QuickStart() {
		if(SceneLoader.s.currentLevel == null)
			SelectLevel(allLevels[0]);
		MoneyController.s.money = 1000;
		StartLevel();
	}
}
