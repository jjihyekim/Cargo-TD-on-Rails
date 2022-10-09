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
	public Upgrade selectedUpgrade;
	
	[ReadOnly]
	public MiniGUI_UpgradeCompound[] myUpgradeCompounds;
	[ReadOnly]
	public MiniGUI_UpgradeButton[] myUpgradeButtons;
	

	public HashSet<string> unlockedUpgrades = new HashSet<string>();

	public UnityEvent callWhenUpgradesChanged = new UnityEvent();

	private void Start() {
		foreach (var upgrade in allUpgrades) {
			upgrade.Initialize();
		}

		InitializeUpgradeButtons();
		
		callWhenUpgradesChanged?.Invoke();
	}

	void InitializeUpgradeButtons() {
		myUpgradeCompounds = UpgradesParent.GetComponentsInChildren<MiniGUI_UpgradeCompound>(true);
		myUpgradeButtons = UpgradesParent.GetComponentsInChildren<MiniGUI_UpgradeButton>(true);

		foreach (var upgradeCompound in myUpgradeCompounds) {
			upgradeCompound.Initialize();
		}

		ChangeSelectedUpgrade(selectedUpgrade);
	}
	
	public void SetUpgradeScreenStatus(bool isOpen) {
		if (isOpen) {
			InitializeUpgradeButtons();
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
	

	public void BuyUpgrade(Upgrade toBuy) {
		var mySave = DataSaver.s.GetCurrentSave();
		if (!toBuy.isUnlocked && mySave.currentRun.myResources.money >= toBuy.shopCost) {
			mySave.currentRun.myResources.money -= toBuy.shopCost;
			GetUpgrade(toBuy);
		}
	}

	public void GetUpgrade(Upgrade toGet) {
		var mySave = DataSaver.s.GetCurrentSave();
		if (!toGet.isUnlocked) {
			mySave.currentRun.upgrades.Add(toGet.upgradeUniqueName);
			
			toGet.isUnlocked = true;
			toGet.ApplyUpgradeEffects();

			callWhenUpgradesChanged?.Invoke();
			DataSaver.s.SaveActiveGame();
		}
	}

	public Upgrade[] GetRandomLevelRewards() {
		var eligibleRewards = new List<Upgrade>();

		for (int i = 0; i < allUpgrades.Count; i++) {
			if (!allUpgrades[i].isUnlocked) {
				if (allUpgrades[i].parentUpgrade.upgradeUniqueName == allUpgrades[i].upgradeUniqueName || allUpgrades[i].parentUpgrade.isUnlocked) {
					eligibleRewards.Add(allUpgrades[i]);
				} 
			}
		}
		
		eligibleRewards.Shuffle();

		var count = 2;
		
		var results = new Upgrade[count];
		eligibleRewards.CopyTo(0, results, 0, count);
		


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
		return GetRandomLevelRewards();
	}
}
