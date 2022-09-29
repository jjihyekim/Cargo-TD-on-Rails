using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_BuildingButton : MonoBehaviour {
	
	public TrainBuilding myBuilding;

	public TMP_Text costText;
	public Image icon;
	private Button myButton;
	
	public bool canBuild = true;

	public GameObject button;
	public void StartBuilding() {
		if(canBuild)
			PlayerBuildingController.s.StartBuilding(myBuilding);
	}

	private void Update() {
		canBuild = MoneyController.s.scraps >= myBuilding.cost;
		myButton.interactable = canBuild;
	}

	private void Start() {
		UpgradesController.s.callWhenUpgradesChanged.AddListener(UpdateButtonStatus);
		UpdateButtonStatus();
	}

	void UpdateButtonStatus() {
		gameObject.SetActive(UpgradesController.s.unlockedUpgrades.Contains(myBuilding.uniqueName));
		
		costText.text = myBuilding.cost.ToString();
		myButton = GetComponent<Button>();
		icon.sprite = myBuilding.Icon;
	}
}
