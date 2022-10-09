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
	
	
	public TMP_Text costText;
	public TMP_Text rewardText;

	public int count = 3;

	private bool canBuild = true;

	public void StartBuilding() {
		if (canBuild) {
			PlayerBuildingController.s.StartBuilding(myBuilding, UpdateBuildingCount, AttachBuildingToButton);
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
		
		UpdateCountText();

		return count > 0;
	}

	void AttachBuildingToButton(TrainBuilding building) {
		building.GetComponent<ReturnCargoAction>().myButton = this;
		

		DataSaver.s.GetCurrentSave().currentRun.myResources.money -= mod.moneyCost;
		DataSaver.s.GetCurrentSave().currentRun.myTrain = Train.s.GetTrainState();
		
		DataSaver.s.SaveActiveGame();
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

	private CargoModule mod;
	private void Start() {
		myButton = GetComponent<Button>();
		mod = myBuilding.GetComponent<CargoModule>();
	}

	private void Update() {
		var cost = mod.moneyCost;
		var reward = mod.moneyReward;
		canBuild = DataSaver.s.GetCurrentSave().currentRun.myResources.money >= cost && count > 0;
		costText.text = cost.ToString();
		rewardText.text = $"+{reward}";
		myButton.interactable = canBuild;
	}

	public void ReturnCargo(ReturnCargoAction source) {
		count += 1;
		var cost = mod.moneyCost;
		DataSaver.s.GetCurrentSave().currentRun.myResources.money += cost;
		
		Destroy(source.gameObject);
		DataSaver.s.GetCurrentSave().currentRun.myTrain = Train.s.GetTrainState();
		
		DataSaver.s.SaveActiveGame();
	}
}
