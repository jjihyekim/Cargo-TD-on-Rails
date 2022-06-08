using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_StarterBuildingButton : MonoBehaviour {
	
	public TrainBuilding myBuilding;

	public TMP_Text countText;
	public Image myIcon;
	private Button myButton;

	public int count = 1;

	private bool canBuild = true;

	public void StartBuilding() {
		if (canBuild) {
			PlayerBuildingController.s.StartBuilding(myBuilding, UpdateBuildingCount);
			UpdateCountText();
		}
	}


	bool UpdateBuildingCount(bool isSuccess) {
		if (isSuccess) {
			count -= 1;
		} else {
			//count += 1;
		}

		canBuild = count > 0;
		myButton.interactable = canBuild;
		
		
		StarterUIController.s.UpdateCanStartStatus();
		UpdateCountText();

		return count > 0;
	}

	public void SetUp(TrainBuilding building, int count) {
		this.count = count;
		myBuilding = building;
		myIcon.sprite = myBuilding.Icon;
		UpdateCountText();
	}

	public void UpdateCountText() {
		countText.text = count + "x";
	}

	private void Start() {
		myButton = GetComponent<Button>();
	}
}
