using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradesController : MonoBehaviour {
	public static UpgradesController s;
	private void Awake() {
		s = this;
	}

	public Transform UpgradesParent;
	public Upgrade[] allUpgrades;

	private void Start() {
		allUpgrades = UpgradesParent.GetComponentsInChildren<Upgrade>(true);
	}

	public void BuyUpgrade(Upgrade toBuy) {
		var mySave = DataSaver.s.GetCurrentSave();
		if (!toBuy.isUnlocked && mySave.money > toBuy.cost) {
			var index = mySave.upgradeDatas.FindIndex(x => x.upgradeName == toBuy.upgradeName);
			if (index != -1) {
				mySave.upgradeDatas[index].isUnlocked = true;
			} else {
				mySave.upgradeDatas.Add(new DataSaver.UpgradeData() { isUnlocked = true, upgradeName = toBuy.upgradeName });
			}

			mySave.money -= toBuy.cost;
			DataSaver.s.SaveActiveGame();
		}
	}
}
