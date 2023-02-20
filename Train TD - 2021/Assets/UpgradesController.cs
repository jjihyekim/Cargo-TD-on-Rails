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

	public Transform shopOptionsParent;
	public GameObject shopOptionPrefab;

	public MiniGUI_BuySupplies[] supplies;

	private void Start() {
		if (DataSaver.s.GetCurrentSave().isInARun) {
			DrawShopOptions();
		}

		//MissionWinFinisher.s.OnLevelFinished.AddListener(DrawShopOptions);
	}

	public void DrawShopOptions() {
		shopOptionsParent.DeleteAllChildren();
		
		// must initialize shop modules before supplies so that things can be set up
		var shopOptions = ModuleRewardsMaster.s.GetShopContent();
		for (int i = 0; i < supplies.Length; i++) {
			supplies[i].Setup();
		}

		for (int i = 0; i < shopOptions.Length; i++) {
			var option = Instantiate(shopOptionPrefab, shopOptionsParent).GetComponent<MiniGUI_BuyBuilding>();
			option.Setup(shopOptions[i]);
		}
		
		SetUpBuyCargo.s.SetUpCargos();
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
