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

	public int cost;
	public int reward;

	public TMP_Text cargoNameText;
	public TMP_Text costText;
	public TMP_Text rewardText;

	public int count = 3;

	private bool canBuild = true;

	public List<TrainBuilding> myBuildings = new List<TrainBuilding>();

	public void StartBuilding() {
		if (canBuild) {
			PlayerBuildingController.s.StartBuilding(myBuilding, UpdateBuildingCount, AttachBuildingToButton, true);
			UpdateCountText();
		}
	}


	bool UpdateBuildingCount(bool isSuccess) {
		if (isSuccess) {
			count -= 1;
		} else {
			//count += 1;
		}
		
		if (isSuccess) {
			MoneyController.s.ModifyResource(ResourceTypes.money, -cost);
			DataSaver.s.SaveActiveGame();
		}
		
		Update();
		UpdateCountText();

		return canBuild;
	}

	void AttachBuildingToButton(TrainBuilding building) {
		building.GetComponent<ReturnCargoAction>().myButton = this;
		building.GetComponent<ReturnCargoAction>().enabled = true;

		building.GetComponent<CargoModule>().moneyCost = cost;
		building.GetComponent<CargoModule>().moneyReward = reward;

		myBuildings.Add(building);

		DataSaver.s.GetCurrentSave().currentRun.myTrain = Train.s.GetTrainState();
		DataSaver.s.SaveActiveGame();
	}

	public void SetUp(TrainBuilding building, int _count, int _cost, int _reward) {
		count = _count;
		myBuilding = building;
		myIcon.sprite = myBuilding.Icon;
		cargoNameText.text = myBuilding.displayName;
		cost = _cost;
		reward = _reward;
		UpdateCountText();
	}

	public void UpdateCountText() {
		countText.text = count + "x";
	}

	private CargoModule mod;
	private void Start() {
		myButton = GetComponent<Button>();
		mod = myBuilding.GetComponent<CargoModule>();
		//SetUp(myBuilding, count);
	}

	private void Update() {
		var currentSave = DataSaver.s.GetCurrentSave();
		if (currentSave.isInARun) {
			canBuild = MoneyController.s.HasResource(ResourceTypes.money, cost) && count > 0;
			costText.text = cost.ToString();
			rewardText.text = $"+{reward}";
			myButton.interactable = canBuild;
			UpdateCountText();
		}
	}

	public void ReturnCargo(TrainBuilding source) {
		count += 1;
		MoneyController.s.ModifyResource(ResourceTypes.money, cost);
		
		myBuildings.Remove(source);
		
		Destroy(source.gameObject);

		DataSaver.s.GetCurrentSave().currentRun.myTrain = Train.s.GetTrainState();
		DataSaver.s.SaveActiveGame();
	}

	public void SellAllCargo() {
		for (int i = myBuildings.Count-1; i >= 0; i--) {
			ReturnCargo(myBuildings[i]);
		}
	}
}
