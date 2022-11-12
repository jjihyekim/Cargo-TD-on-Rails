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
	public TMP_Text amountText;
	
	public bool canBuild = true;


	public int currentlyBuild = 0;
	public int maxCount = 0;

	[Space] 
	public GameObject armorPenetration;
	public void StartBuilding() {
		if(canBuild)
			PlayerBuildingController.s.StartBuilding(myBuilding, UpdateBuildingCount);
	}

	void BuildingSuccess() {
		currentlyBuild += 1;
		if (currentlyBuild >= maxCount) {
			myButton.interactable = false;
		}
	}
	
	bool UpdateBuildingCount(bool isSuccess) {
		if (isSuccess) {
			currentlyBuild += 1;
		} else {
			//count += 1;
		}

		amountText.text = $"{currentlyBuild}/{maxCount}";

		canBuild = currentlyBuild < maxCount && MoneyController.s.HasResource(ResourceTypes.scraps, myBuilding.cost);
		myButton.interactable = canBuild;

		return canBuild;
	}

	private void Update() {
		canBuild = currentlyBuild < maxCount && MoneyController.s.HasResource(ResourceTypes.scraps, myBuilding.cost);
		myButton.interactable = canBuild;
	}

	

	private void OnDestroy() {
		Train.s.trainUpdatedThroughNonBuildingActions.RemoveListener(TrainUpdated);
		UpgradesController.s.callWhenUpgradesOrModuleAmountsChanged.RemoveListener(UpdateButtonStatus);
	}

	private void Start() {
		UpdateButtonStatus();
		TrainUpdated();
		UpgradesController.s.callWhenUpgradesOrModuleAmountsChanged.AddListener(UpdateButtonStatus);
		Train.s.trainUpdatedThroughNonBuildingActions.AddListener(TrainUpdated);
	}

	void TrainUpdated() {
		var train = Train.s.GetTrainState();
		currentlyBuild = 0;

		var skipCount = 0;
		for (int i = 0; i < train.myCarts.Count; i++) {
			for (int j = 0; j < train.myCarts[i].buildingStates.Length; j++) {
				if (skipCount > 0) { // we skip the next three duplicates if the prev building was an entire slot building
					skipCount -= 1;
					continue;
				}

				var building = train.myCarts[i].buildingStates[j].uniqueName;
				if (building == myBuilding.uniqueName) {
					currentlyBuild += 1;
					if (myBuilding.occupiesEntireSlot) { // skip rest of the slots because they have the same thing
						skipCount = 3;
					}
				}
			}
		}
		
		
		amountText.text = $"{currentlyBuild}/{maxCount}";
	}

	void UpdateButtonStatus() {
		var currentSave = DataSaver.s.GetCurrentSave();
		if (currentSave.isInARun) {
			var myBuilds = currentSave.currentRun.trainBuildings;
			TrainModuleHolder currentBuilding = null;

			for (int i = 0; i < myBuilds.Count; i++) {
				if (myBuilds[i].moduleUniqueName == myBuilding.uniqueName) {
					currentBuilding = myBuilds[i];
					break;
				}
			}

			if (currentBuilding == null) {
				//Debug.LogError($"building not found {myBuilding}");
				gameObject.SetActive(false);
				return;
			}

			maxCount = currentBuilding.amount;

			if (maxCount == 0)
				gameObject.SetActive(false);
			else
				gameObject.SetActive(true);


			costText.text = myBuilding.cost.ToString();
			myButton = GetComponent<Button>();
			icon.sprite = myBuilding.Icon;

			var gunModule = myBuilding.GetComponent<GunModule>();

			if (gunModule != null) {
				armorPenetration.SetActive(gunModule.canPenetrateArmor);
			} else {
				armorPenetration.SetActive(false);
			}

			var tooltip = GetComponent<UITooltipDisplayer>();
			var moduleTooltip = myBuilding.GetComponent<ClickableEntityInfo>();

			if (tooltip != null && moduleTooltip != null) {
				tooltip.myTooltip = moduleTooltip.GetTooltip();
			}
		}
		
		
		amountText.text = $"{currentlyBuild}/{maxCount}";
	}
}
