using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StarterUIController : MonoBehaviour {
	public static StarterUIController s;
	private void Awake() { s = this; }

	public LevelData[] allLevels;

	public Transform levelButtonParent;
	public GameObject levelButtonPrefab;
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
	
	private void Start() {
		allLevelButtons = new MiniGUI_LevelButton[allLevels.Length];
		for (int i = 0; i < allLevels.Length; i++) {
			allLevelButtons[i] = Instantiate(levelButtonPrefab, levelButtonParent).GetComponent<MiniGUI_LevelButton>().SetUp(allLevels[i]);
		}
		if(LevelLoader.s.currentLevel != null)
			SelectLevel(LevelLoader.s.currentLevel);
	}

	void SetCurrentSelectedLevel() {
		for (int i = 0; i < allLevels.Length; i++) {
			if (allLevels[i] == LevelLoader.s.currentLevel) {
				allLevelButtons[i].SetSelected(true);
			} else {
				allLevelButtons[i].SetSelected(false);
			}
		}

	}

	public void BackToProfileSelection() {
		LevelLoader.s.OpenProfileScreen();
	}

	public void StartLevel() {
		ClearStaticTrackers();
		
		starterUI.SetActive(false);
		gameUI.SetActive(true);
		LevelLoader.s.isLevelStarted = true;
		OnLevelStarted?.Invoke();
	}

	void ClearStaticTrackers() {
		ModuleHealth.buildingsBuild = 0;
		ModuleHealth.buildingsDestroyed = 0;
		EnemyHealth.enemySpawned = 0;
		EnemyHealth.enemyKilled = 0;
		PlayerBuildingController.s.currentLevelBuilds = new Dictionary<string, List<PlayerBuildingController.BuildingBuildData>>();
	}

	public void SelectLevel(LevelData data) {
		LevelLoader.s.currentLevel = data;

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
		SetCurrentSelectedLevel();
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

	public InputActionReference quickStart;

	private void OnEnable() {
		quickStart.action.Enable();
		quickStart.action.performed += QuickStart;
	}


	private void OnDisable() {
		quickStart.action.Disable();
		quickStart.action.performed -= QuickStart;
	}

	private void QuickStart(InputAction.CallbackContext obj) {
		QuickStart();
	}

	public void QuickStart() {
		if(LevelLoader.s.currentLevel == null)
			SelectLevel(allLevels[0]);
		MoneyController.s.money = 1000;
		StartLevel();
	}
}
