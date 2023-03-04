using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UpgradesController : MonoBehaviour {
	public static UpgradesController s;
	private void Awake() {
		s = this;
	}
	

	[ValueDropdown("GetAllModuleNames")]
	public List<string> tier1Buildings = new List<string>();
	[ValueDropdown("GetAllModuleNames")]
	public List<string> tier2Buildings = new List<string>();
	[ValueDropdown("GetAllModuleNames")]
	public List<string> tier3Buildings = new List<string>();
	[ValueDropdown("GetAllModuleNames")]
	public List<string> bossBuildings = new List<string>();

	public UnityEvent callWhenUpgradesOrModuleAmountsChanged = new UnityEvent();

	public Transform cargoLocationsParent;
	private TrainStationCargoLocation[] _cargoLocations;
	public Transform shopableComponentsParent;
	public GameObject buildingCargo;
	public GameObject powerupCargo;
	public GameObject scrapPile;
	public GameObject fuelPile;

	public MiniGUI_BuySupplies[] supplies;

	private void Start() {
		_cargoLocations = cargoLocationsParent.GetComponentsInChildren<TrainStationCargoLocation>();
		
		if (DataSaver.s.GetCurrentSave().isInARun) {
			DrawShopOptions();
		}
		
		//MissionWinFinisher.s.OnLevelFinished.AddListener(DrawShopOptions);
	}

	[System.Serializable]
	public class ShopState {
		public List<string> buildingCargos = new List<string>();
		public List<string> powerUpCargos = new List<string>();
		public int scrapPileCount;
		public int fuelPilecount;
	}
	void InitializeShop(DataSaver.RunState state) {
		state.shopState = new ShopState();

		state.shopState.scrapPileCount = Random.Range(1, 5);
		state.shopState.fuelPilecount = Random.Range(0, 1);
		var buildingCargoCount = Random.Range(2, 4);
		var powerupCargoCount = Random.Range(0, 2);

		for (int i = 0; i < buildingCargoCount; i++) {
			state.shopState.buildingCargos.Add(GetRandomBuildingCargo());
		}

		for (int i = 0; i < powerupCargoCount; i++) {
			state.shopState.powerUpCargos.Add(GetRandomPowerup());
		}

		state.shopInitialized = true;
	}

	public void RemoveCargoFromShopArea(CargoModule module) {
		if (module.isBuildingReward) {
			DataSaver.s.GetCurrentSave().currentRun.shopState.buildingCargos.Remove(module.myReward);
		} else {
			DataSaver.s.GetCurrentSave().currentRun.shopState.powerUpCargos.Remove(module.myReward);
		}
	}

	public void AddCargoToShopArea(CargoModule module) {
		if (module.isBuildingReward) {
			DataSaver.s.GetCurrentSave().currentRun.shopState.buildingCargos.Add(module.myReward);
		} else {
			DataSaver.s.GetCurrentSave().currentRun.shopState.powerUpCargos.Add(module.myReward);
		}
	}

	public void RemoveScrapFromShopArea() {
		DataSaver.s.GetCurrentSave().currentRun.shopState.scrapPileCount -= 1;
	}

	public void RemoveFuelFromShopArea() {
		DataSaver.s.GetCurrentSave().currentRun.shopState.fuelPilecount -= 1;
	}
	public void DrawShopOptions() {
		var currentRun = DataSaver.s.GetCurrentSave().currentRun;
		if (!currentRun.shopInitialized) {
			InitializeShop(currentRun);
		} 
		
		for (int i = 0; i < _cargoLocations.Length; i++) {
			var buildings = _cargoLocations[i].GetComponentsInChildren<TrainBuilding>();
			for (int j = 0; j < buildings.Length; j++) {
				Destroy(buildings[j].gameObject);
			}
			
			_cargoLocations[i].transform.rotation = Quaternion.Euler(0,Random.Range(0,360),0);
		}
		
		//var shopOptions = ModuleRewardsMaster.s.GetShopContent(); // module master is ded
		/*for (int i = 0; i < supplies.Length; i++) {
			supplies[i].Setup();
		}*/ // suplies are ded

		for (int i = 0; i < _cargoLocations.Length; i++) {
			_cargoLocations[i].isOccupied= false;
		}


		for (int i = 0; i < currentRun.shopState.buildingCargos.Count; i++) {
			var location = GetRandomLocation();
			if(location == null)
				return;

			var thingy = Instantiate(buildingCargo, shopableComponentsParent);
			thingy.transform.position = location.transform.position;
			thingy.transform.rotation = location.transform.rotation;
			thingy.GetComponent<CargoModule>().myReward =currentRun.shopState.buildingCargos[i];
			thingy.GetComponent<CargoModule>().isBuildingReward = true;
			thingy.GetComponent<TrainBuilding>().CompleteBuilding(false, false);
			location.GetComponentInChildren<Slot>().AddBuilding(thingy.GetComponent<TrainBuilding>(),0);
		}
		
		for (int i = 0; i < currentRun.shopState.powerUpCargos.Count; i++) {
			var location = GetRandomLocation();
			if(location == null)
				return;

			var thingy = Instantiate(powerupCargo, shopableComponentsParent);
			thingy.transform.position = location.transform.position;
			thingy.transform.rotation = location.transform.rotation;
			thingy.GetComponent<CargoModule>().myReward = currentRun.shopState.powerUpCargos[i];
			thingy.GetComponent<CargoModule>().isBuildingReward = false;
			thingy.GetComponent<TrainBuilding>().CompleteBuilding(false, false);
			location.GetComponentInChildren<Slot>().AddBuilding(thingy.GetComponent<TrainBuilding>(),0);
		}
		
		for (int i = 0; i < currentRun.shopState.scrapPileCount; i++) {
			var location = GetRandomLocation();
			if(location == null)
				return;

			var thingy = Instantiate(scrapPile, shopableComponentsParent);
			thingy.transform.position = location.transform.position;
			thingy.transform.rotation = location.transform.rotation;
			thingy.GetComponent<TrainBuilding>().CompleteBuilding(false, false);
			location.GetComponentInChildren<Slot>().AddBuilding(thingy.GetComponent<TrainBuilding>(),0);
		}
		
		for (int i = 0; i < currentRun.shopState.fuelPilecount; i++) {
			var location = GetRandomLocation();
			if(location == null)
				return;

			var thingy = Instantiate(fuelPile, shopableComponentsParent);
			thingy.transform.position = location.transform.position;
			thingy.transform.rotation = location.transform.rotation;
			thingy.GetComponent<TrainBuilding>().CompleteBuilding(false, false);
			location.GetComponentInChildren<Slot>().AddBuilding(thingy.GetComponent<TrainBuilding>(),0);
		}

		// SetUpBuyCargo.s.SetUpCargos(); //setup buy cargo is ded
	}

	TrainStationCargoLocation GetRandomLocation() {
		var availableCount = 0;
		for (int i = 0; i < _cargoLocations.Length; i++) {
			if (!_cargoLocations[i].isOccupied)
				availableCount += 1;
		}

		if (availableCount < 0)
			return null;

		var selection = Random.Range(0, availableCount);
		for (int i = 0; i < _cargoLocations.Length; i++) {
			if (!_cargoLocations[i].isOccupied)
				selection -= 1;
			if (selection <= 0) {
				if (!_cargoLocations[i].isOccupied) {
					_cargoLocations[i].isOccupied = true;
					return _cargoLocations[i];
				}
			}
		}

		return null;
	}
	
	public void AddModulesToAvailableModules(string buildingUniqueName, int count) {
		var curRun = DataSaver.s.GetCurrentSave().currentRun;
		TrainModuleHolder myHolder = null;
		for (int i = 0; i < curRun.trainBuildings.Count; i++) {
			if (curRun.trainBuildings[i].moduleUniqueName == buildingUniqueName) {
				myHolder = curRun.trainBuildings[i];
				break;
			}
		}

		if (myHolder == null) {
			myHolder = new TrainModuleHolder();
			myHolder.moduleUniqueName = buildingUniqueName;
			myHolder.amount = 0;
			curRun.trainBuildings.Add(myHolder);
		}

		myHolder.amount += 1;
		
		DataSaver.s.SaveActiveGame();
		callWhenUpgradesOrModuleAmountsChanged?.Invoke();
	}

	public void GetBuilding(string buildingUniqueName) {
		AddModulesToAvailableModules(buildingUniqueName, 1);
	}

	public string GetRandomPowerup() {
		return DataHolder.s.powerUps[Random.Range(0, DataHolder.s.powerUps.Length)].name;
	}
	
	public string GetRandomBuildingCargo() {
		string results;

		switch (DataSaver.s.GetCurrentSave().currentRun.currentAct) {
			case 1:
				results = tier1Buildings[Random.Range(0,tier1Buildings.Count)];
				break;
			case 2:
				if (Random.value > 0.5f) {
					results = tier2Buildings[Random.Range(0,tier2Buildings.Count)];
				} else {
					results = tier1Buildings[Random.Range(0,tier1Buildings.Count)];
				}
				break;
			case 3:
				if (Random.value > 0.5f) {
					results = tier3Buildings[Random.Range(0,tier3Buildings.Count)];
				} else if(Random.value > 0.5f){
					results = tier2Buildings[Random.Range(0,tier2Buildings.Count)];
				} else {
					results = tier1Buildings[Random.Range(0,tier1Buildings.Count)];
				}
				//results = tier3Buildings[Random.Range(0,tier3Buildings.Count)];
				break;
			default:
				results = tier3Buildings[Random.Range(0,tier3Buildings.Count)];
				Debug.LogError($"Illegal Act Number {DataSaver.s.GetCurrentSave().currentRun.currentAct}");
				break;
		}
		
		return results;
	}
	
	public string[] GetRandomLevelRewards() {
		string[] results;

		switch (DataSaver.s.GetCurrentSave().currentRun.currentAct) {
			case 1:
				results = GetBuildingsFromList(tier1Buildings);
				break;
			case 2:
				if (Random.value > 0.5f) {
					results = GetBuildingsFromList(tier2Buildings);
				} else {
					results = GetBuildingsFromList(tier1Buildings);
				}
				break;
			case 3:
				if (Random.value > 0.5f) {
					results = GetBuildingsFromList(tier3Buildings);
				} else if(Random.value > 0.5f){
					results = GetBuildingsFromList(tier2Buildings);
				} else {
					results = GetBuildingsFromList(tier1Buildings);
				}
				results = GetBuildingsFromList(tier3Buildings);
				break;
			default:
				results = GetBuildingsFromList(tier3Buildings);
				Debug.LogError($"Illegal Act Number {DataSaver.s.GetCurrentSave().currentRun.currentAct}");
				break;
		}
		
		return results;
	}

	public string[] GetRandomBossRewards() {
		var results = GetBuildingsFromList(bossBuildings);
		return results;
	}

	private string[] GetBuildingsFromList(List<string> buildings) {
		var count = 3;
		
		
		var eligibleRewards = new List<string>();
		
		while (eligibleRewards.Count < count) {
			eligibleRewards.AddRange(buildings);
		}
		
		eligibleRewards.Shuffle();
		

		var results = new String[count];
		eligibleRewards.CopyTo(0, results, 0, count);
		return results;
	}
	
	private static IEnumerable GetAllModuleNames() {
		var buildings = GameObject.FindObjectOfType<DataHolder>().buildings;
		var buildingNames = new List<string>();
		buildingNames.Add("");
		for (int i = 0; i < buildings.Length; i++) {
			buildingNames.Add(buildings[i].uniqueName);
		}
		return buildingNames;
	}
}
