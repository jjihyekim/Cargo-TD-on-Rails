using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradesController : MonoBehaviour {
	public static UpgradesController s;
	private void Awake() {
		s = this;
	}

	public Transform UpgradesParent;
	[ReadOnly]
	public List<Upgrade> allUpgrades = new List<Upgrade>();
	public Upgrade selectedUpgrade;

	[ReadOnly]
	public MiniGUI_UpgradeCompound[] myUpgradeCompounds;
	[ReadOnly]
	public MiniGUI_UpgradeButton[] myUpgradeButtons;

	public bool isUpgradeScreenActive = false;

	public HashSet<string> unlockedUpgrades = new HashSet<string>();

	private void Start() {
		myUpgradeCompounds = UpgradesParent.GetComponentsInChildren<MiniGUI_UpgradeCompound>(true);
		myUpgradeButtons = UpgradesParent.GetComponentsInChildren<MiniGUI_UpgradeButton>(true);
		foreach (var upgradeCompound in myUpgradeCompounds) {
			upgradeCompound.Initialize();
			allUpgrades.Add(upgradeCompound.unlockUpgrade);
			allUpgrades.Add(upgradeCompound.skillUpgrade1);
			allUpgrades.Add(upgradeCompound.skillUpgrade2);
		}
		
		foreach (var upgrade in allUpgrades) {
			upgrade.Initialize();
		}
		
		ChangeSelectedUpgrade(selectedUpgrade);
	}

	

	public void SetUpgradeScreenStatus(bool isOpen) {
		isUpgradeScreenActive = isOpen;
		previewCam.enabled = isOpen;
	}



	[Header("Upgrade Info Display")] 
	public Image icon;
	public TMP_Text moduleName;
	public TMP_Text moduleCost;
	[Space] 
	public TMP_Text upgradeName;
	public TMP_Text upgradeDescription;
	public MiniGUI_UpgradeButton upgradeButton;
	

	public void ChangeSelectedUpgrade(Upgrade toSelect) {
		selectedUpgrade = toSelect;
		var parentUpgrade = selectedUpgrade.parentUpgrade;
		
		if (previewBuilding == null || previewBuilding.uniqueName != parentUpgrade.module.uniqueName) {
			if (previewBuilding != null)
				Destroy(previewBuilding.gameObject);

			previewBuilding = Instantiate(parentUpgrade.module.gameObject, previewSlots[curSlot].transform).GetComponent<TrainBuilding>();
			previewBuilding.transform.localPosition = Vector3.zero;
			previewBuilding.transform.localRotation = Quaternion.identity;
			previewBuilding.CompleteBuilding(false);
			
			var rangeShower = previewBuilding.GetComponentInChildren<RangeVisualizer>(true);
			//rangeShower.ChangeVisualizerStatus(true);
			if(rangeShower != null)
				Destroy(rangeShower.gameObject);

			var audioSources = previewBuilding.GetComponentsInChildren<AudioSource>(true);

			foreach (var audio in audioSources) {
				audio.enabled = false;
			}
		}

		icon.sprite = parentUpgrade.icon;
		moduleName.text = parentUpgrade.module.displayName;
		moduleCost.text = parentUpgrade.module.cost.ToString();
		
		upgradeName.text = selectedUpgrade.upgradeName;
		upgradeDescription.text = selectedUpgrade.upgradeDescription;
		upgradeButton.SetUp(selectedUpgrade);
		

		RefreshAllButtons();
	}

	void RefreshAllButtons() {
		upgradeButton.Refresh();
		upgradeButton.SetSelectionStatus(false);
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
		if (!toBuy.isUnlocked && mySave.money > toBuy.cost) {
			var index = mySave.upgradeDatas.FindIndex(x => x.upgradeName == toBuy.upgradeUniqueName);
			if (index != -1) {
				mySave.upgradeDatas[index].isUnlocked = true;
			} else {
				mySave.upgradeDatas.Add(new DataSaver.UpgradeData() { isUnlocked = true, upgradeName = toBuy.upgradeUniqueName });
			}

			toBuy.isUnlocked = true;
			toBuy.ApplyUpgradeEffects();

			mySave.money -= toBuy.cost;
			DataSaver.s.SaveActiveGame();
		}
		
		
		RefreshAllButtons();
	}


	[Header("Gun Preview Area")] 
	public Camera previewCam;
	public Cart previewCart;
	public Slot[] previewSlots;
	private int curSlot = 0;
	public TrainBuilding previewBuilding;

	public Vector3 cartRotate = new Vector3(0, 20, 0);

	public float buildStateSwitchTime = 0.5f;
	private float curTime;
	private int curCycleCount = 0;
	private void Update() {
		if (isUpgradeScreenActive) {
			previewCart.transform.Rotate(cartRotate * Time.deltaTime);

			if (curTime <= 0) {
				curTime = buildStateSwitchTime;

				if (curCycleCount < previewBuilding.rotationCount) {
					previewBuilding.CycleRotation(true);
					curCycleCount += 1;
				} else {
					curCycleCount = 0;
					curSlot += 1;
					curSlot %= 2;
					previewBuilding.transform.position = previewSlots[curSlot].transform.position;
				}
			} else {
				curTime -= Time.deltaTime;
			}
		}
	}
}
