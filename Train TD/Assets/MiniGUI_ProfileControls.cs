using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_ProfileControls : MonoBehaviour {
	private MiniGUI_ProfileDisplay _display;

	public int mySaveId = 0;

	DataSaver.SaveFile mySave {
		get {
			return DataSaver.s.allSaves[mySaveId];
		}
	}

	public Button selectButton;
	public TMP_Text selectButtonText;
	public Button setNameButton;
	public TMP_InputField nameField;
	public Image background;
	
	public Color activeColor = Color.white;
	public Color unActiveColor;


	private static GenericCallback updateActiveStatus;
	private void Awake() {
		updateActiveStatus += SetActiveStatus;
	}

	private void OnDestroy() {
		updateActiveStatus -= SetActiveStatus;
	}

	private void OnEnable() {
		_display = GetComponentInChildren<MiniGUI_ProfileDisplay>();
		_display.SetStats(mySave);

		nameField.text = mySave.saveName;

		SetActiveStatus();
	}


	void SetActiveStatus() {
		if (mySaveId == DataSaver.s.ActiveSave) {
			background.color = activeColor;
			
			setNameButton.interactable = true;
			selectButton.interactable = false;

			selectButtonText.text = "Active";
		} else {
			background.color = unActiveColor;
			
			setNameButton.interactable = false;
			selectButton.interactable = true;

			selectButtonText.text = "Select";
		}
	}

	public void ActivateSave() {
		DataSaver.s.SetActiveSave(mySaveId);
		updateActiveStatus?.Invoke();
		ProfileSelectionMenu.s.SaveChanged();
	}

	public void SetSaveName() {
		DataSaver.s.SetActiveSave(mySaveId);
		mySave.saveName = nameField.text;
		_display.SetStats(mySave);
		DataSaver.s.SaveActiveGame();
		updateActiveStatus?.Invoke();
		ProfileSelectionMenu.s.SaveChanged();
	}

	public void DeleteSave() {
		DataSaver.s.SetActiveSave(mySaveId);
		updateActiveStatus?.Invoke();
		ProfileSelectionMenu.s.SaveChanged();
		ProfileSelectionMenu.s.DeleteSaveDialog(mySave);
	}
}
