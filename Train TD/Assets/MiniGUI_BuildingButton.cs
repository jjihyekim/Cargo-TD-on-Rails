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

	public void StartBuilding() {
		PlayerBuildingController.s.StartBuilding(myBuilding);
	}

	private void Update() {
		myButton.interactable = MoneyController.s.money >= myBuilding.cost;
	}

	private void Start() {
		if (!UpgradesController.s.unlockedUpgrades.Contains(myBuilding.uniqueName)) {
			//Debug.Log("enable me so that not unlocked guns dont appear");
			Destroy(gameObject);
		}
		
		costText.text = myBuilding.cost.ToString();
		myButton = GetComponent<Button>();
		icon.sprite = myBuilding.Icon;
	}
}
