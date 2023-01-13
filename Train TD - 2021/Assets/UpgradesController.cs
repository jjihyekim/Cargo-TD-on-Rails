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

	public Transform UpgradesParent;
	public List<Upgrade> allUpgrades = new List<Upgrade>();
	
	public List<Upgrade> tier1Upgrades = new List<Upgrade>();
	public List<Upgrade> tier2Upgrades = new List<Upgrade>();
	public List<Upgrade> tier3Upgrades = new List<Upgrade>();
	public List<Upgrade> bossUpgrades = new List<Upgrade>();
	public Upgrade selectedUpgrade;
	
	
	
	[ReadOnly]
	public MiniGUI_UpgradeCompound[] myUpgradeCompounds;
	[ReadOnly]
	public MiniGUI_UpgradeButton[] myUpgradeButtons;
	

	public HashSet<string> unlockedUpgrades = new HashSet<string>();

	public UnityEvent callWhenUpgradesOrModuleAmountsChanged = new UnityEvent();

	public Transform shopOptionsParent;
	public GameObject shopOptionPrefab;

	public MiniGUI_BuySupplies[] supplies;

	private void Start() {
		if (DataSaver.s.GetCurrentSave().isInARun) {
			foreach (var upgrade in allUpgrades) {
				upgrade.Initialize();
			}

			StartCoroutine(InitializeUpgradeButtons());
			DrawShopOptions();
		}
	}

	void DrawShopOptions() {
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
	}
	
	IEnumerator InitializeUpgradeButtons() {
		myUpgradeCompounds = UpgradesParent.GetComponentsInChildren<MiniGUI_UpgradeCompound>(true);

		foreach (var upgradeCompound in myUpgradeCompounds) {
			upgradeCompound.Initialize();
		}

		yield return null;
		
		myUpgradeButtons = UpgradesParent.GetComponentsInChildren<MiniGUI_UpgradeButton>(true);

		ChangeSelectedUpgrade(selectedUpgrade);
		
		callWhenUpgradesOrModuleAmountsChanged?.Invoke();
	}

	public void SetUpgradeScreenStatus(bool isOpen) {
		if (isOpen) {
			StartCoroutine(InitializeUpgradeButtons());
		} 
	}

	public UpgradeInfoScreenController upgradeInfoScreenController;

	public void ChangeSelectedUpgrade(Upgrade toSelect) {
		selectedUpgrade = toSelect;
		upgradeInfoScreenController.ChangeSelectedUpgrade(toSelect);
		RefreshAllButtons();
	}

	void RefreshAllButtons() {
		foreach (var button in myUpgradeButtons) {
			button.Refresh();

			if (button.myUpgrade.upgradeUniqueName == selectedUpgrade.upgradeUniqueName) {
				button.SetSelectionStatus(true);
			} else {
				button.SetSelectionStatus(false);
			}
		}
	}


	public void AddModulesToAvailableModules(TrainBuilding module, int count) {
		var curRun = DataSaver.s.GetCurrentSave().currentRun;
		TrainModuleHolder myHolder = null;
		for (int i = 0; i < curRun.trainBuildings.Count; i++) {
			if (curRun.trainBuildings[i].moduleUniqueName == module.uniqueName) {
				myHolder = curRun.trainBuildings[i];
				break;
			}
		}

		if (myHolder == null) {
			myHolder = new TrainModuleHolder();
			myHolder.moduleUniqueName = module.uniqueName;
			myHolder.amount = 0;
			curRun.trainBuildings.Add(myHolder);
		}

		myHolder.amount += 1;
		
		DataSaver.s.SaveActiveGame();
		
		callWhenUpgradesOrModuleAmountsChanged?.Invoke();
	}

	public void BuyUpgrade(Upgrade toBuy) {
		var mySave = DataSaver.s.GetCurrentSave();
		if (!toBuy.isUnlocked && MoneyController.s.HasResource(ResourceTypes.money, toBuy.shopCost)) {
			MoneyController.s.ModifyResource(ResourceTypes.money, -toBuy.shopCost);
			GetUpgrade(toBuy);
		}
	}

	public void GetUpgrade(Upgrade toGet) {
		var mySave = DataSaver.s.GetCurrentSave();
		if (!toGet.isUnlocked) {
			mySave.currentRun.upgrades.Add(toGet.upgradeUniqueName);
			
			toGet.isUnlocked = true;
			toGet.ApplyUpgradeEffects();

			callWhenUpgradesOrModuleAmountsChanged?.Invoke();
			DataSaver.s.SaveActiveGame();
		} else {
			AddModulesToAvailableModules(toGet.parentUpgrade.module, 1);
		}
	}
	
	public Upgrade[] GetRandomLevelRewards() {
		Upgrade[] results;

		switch (DataSaver.s.GetCurrentSave().currentRun.currentAct) {
			case 1:
				results = GetUpgradesFromList(tier1Upgrades);
				break;
			case 2:
				results = GetUpgradesFromList(tier2Upgrades);
				break;
			case 3:
				results = GetUpgradesFromList(tier3Upgrades);
				break;
			default:
				results = GetUpgradesFromList(tier3Upgrades);
				Debug.LogError($"Illegal Act Number {DataSaver.s.GetCurrentSave().currentRun.currentAct}");
				break;
		}
		
		return results;
	}

	public Upgrade GetUpgrade(string upgradeName) {
		for (int i = 0; i < allUpgrades.Count; i++) {
			if (allUpgrades[i].upgradeUniqueName == upgradeName) {
				return allUpgrades[i];
			}
		}

		return null;
	}

	public Upgrade[] GetRandomBossRewards() {
		var results = GetUpgradesFromList(bossUpgrades);
		return results;
	}

	private Upgrade[] GetUpgradesFromList(List<Upgrade> upgrades) {
		var count = 3;
		
		
		var eligibleRewards = new List<Upgrade>();

		for (int i = 0; i < upgrades.Count; i++) {
			if (!upgrades[i].isUnlocked) {
				if (upgrades[i].parentUpgrade.upgradeUniqueName == upgrades[i].upgradeUniqueName || upgrades[i].parentUpgrade.isUnlocked) {
					eligibleRewards.Add(upgrades[i]);
				}
			} else {
				if (upgrades[i].parentUpgrade == upgrades[i]) {
					eligibleRewards.Add(upgrades[i]);
				}
			}
		}

		while (eligibleRewards.Count < count) {
			eligibleRewards.AddRange(eligibleRewards);
		}
		eligibleRewards.Shuffle();



		var results = new Upgrade[count];
		eligibleRewards.CopyTo(0, results, 0, count);
		return results;
	}
}
