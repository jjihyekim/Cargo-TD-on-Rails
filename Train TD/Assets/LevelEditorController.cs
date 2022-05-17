using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelEditorController : MonoBehaviour {
	public static LevelEditorController s;

	private void Awake() {
		s = this;
	}

	private void Start() {
		selectedLevel = LevelDataLoader.s.allLevels[PlayerPrefs.GetInt("lastEditedLevel", 0)];
		DrawLevelButtons();
	}
	
	public List<LevelData> allLevels {
		get {
			return LevelDataLoader.s.allLevels;
		}
	}

	public Transform levelButtonParent;
	public GameObject levelButtonPrefab;
	
	[ReadOnly]
	public MiniGUI_LevelEditorLevelButton[] allLevelButtons;
	void DrawLevelButtons() {
		allLevelButtons = new MiniGUI_LevelEditorLevelButton[allLevels.Count];
		for (int i = 0; i < allLevels.Count; i++) {
			allLevelButtons[i] = Instantiate(levelButtonPrefab, levelButtonParent).GetComponent<MiniGUI_LevelEditorLevelButton>().SetUp(allLevels[i]);
		}

		RefreshCurrentSelectedLevel();
	}
	
	void RefreshCurrentSelectedLevel() {
		for (int i = 0; i < allLevels.Count; i++) {
			if (allLevels[i] == selectedLevel) {
				allLevelButtons[i].SetSelected(true);
			} else {
				allLevelButtons[i].SetSelected(false);
			}
		}
		
	}


	public LevelData selectedLevel;
	public MiniGUI_LevelEditorLane[] lanes;
	public GameObject enemyLaneDisplayPrefab;

	public void SelectLevel(LevelData data) {
		PlayerPrefs.SetInt("lastEditedLevel", data.levelMenuOrder);
		selectedLevel = data;

		for (int i = 0; i < lanes.Length; i++) {
			lanes[i].enemiesParent.DeleteAllChildren();

			for (int j = 0; j < selectedLevel.enemyWaves.Length; j++) {
				var curWave = selectedLevel.enemyWaves[j];

				if (curWave.enemyPathType == lanes[i].myPathType) {
					var lanePos = lanes[i].transform.position;
					lanePos.x = LevelPositionToScreenPosition(curWave.startDistance);
					Instantiate(enemyLaneDisplayPrefab, lanePos, Quaternion.identity, lanes[i].enemiesParent);
				}
			}
		}
		
		
		RefreshCurrentSelectedLevel();
	}


	private const float screenEdge = 10;
	public float ScreenPositionToLevelPosition(float screenPosition) {
		return ((screenPosition-screenEdge) / (Screen.width-2*screenEdge)) * selectedLevel.missionDistance;
	}
	
	public float LevelPositionToScreenPosition (float levelPosition) {
		return ((levelPosition / selectedLevel.missionDistance) * (Screen.width-2*screenEdge)) + screenEdge;
	}
	
	public void OnPointerClick(PointerEventData eventData, MiniGUI_LevelEditorLane pathType) {
		throw new System.NotImplementedException();
	}

	public void OnBeginDrag(PointerEventData eventData, MiniGUI_LevelEditorLane pathType) {
		throw new System.NotImplementedException();
	}

	public void OnDrag(PointerEventData eventData, MiniGUI_LevelEditorLane pathType) {
		throw new System.NotImplementedException();
	}

	public void OnEndDrag(PointerEventData eventData, MiniGUI_LevelEditorLane pathType) {
		throw new System.NotImplementedException();
	}


	[Button]
	public void SaveSelectedLevel() {
		Debug.LogError("You cannot save scriptable objects at playtime!");
		//LevelDataLoader.s.SaveLevel(selectedLevel);
	}
}
