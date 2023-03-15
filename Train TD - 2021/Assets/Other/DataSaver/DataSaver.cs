﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FullSerializer;
using Sirenix.OdinInspector;

[Serializable]
public class DataSaver {

	public static DataSaver s;

	private int activeSave = 0;
	public SaveFile[] allSaves = new SaveFile[3];
	public const string saveName1 = "save1.data";
	public const string saveName2 = "save2.data";
	public const string saveName3 = "save3.data";

	public bool loadingComplete = false;

	private static readonly fsSerializer _serializer = new fsSerializer();

	public int ActiveSave {
		get => activeSave;
		private set => activeSave = value;
	}

	public string GetSaveFilePathAndFileName(int index) {
		var saveName = saveName1;
		if (index == 1) {
			saveName = saveName2;
		}

		if (index == 2) {
			saveName = saveName3;
		}

		return Application.persistentDataPath + "/" + saveName;
	}

	public delegate void SaveYourself();

	public static event SaveYourself earlyLoadEvent;
	public static event SaveYourself loadEvent;
	public static event SaveYourself earlySaveEvent;
	public static event SaveYourself saveEvent;


	public SaveFile GetCurrentSave() {
		return allSaves[ActiveSave];
	}

	private float saveStartTime = 0f;

	public void SetActiveSave(int id) {
		if(allSaves[activeSave].isInARun)
			allSaves[activeSave].currentRun.playtime += Time.realtimeSinceStartup - saveStartTime;
		allSaves[activeSave].isActiveSave = false;
		SaveActiveGame();

		saveStartTime = Time.realtimeSinceStartup;
		ActiveSave = id;
		allSaves[activeSave].isActiveSave = true;
		SaveActiveGame();

		earlyLoadEvent?.Invoke();
		loadEvent?.Invoke();
	}

	public float GetTimeSpentSinceLastSaving() {
		return Time.realtimeSinceStartup - saveStartTime;
	}

	public void ClearCurrentSave() {
		Debug.Log("Clearing Save");
		allSaves[ActiveSave] = MakeNewSaveFile(ActiveSave);
		saveStartTime = Time.realtimeSinceStartup;
	}

	public SaveFile MakeNewSaveFile(int id) {
		var file = new SaveFile();
		file.isRealSaveFile = true;
		file.saveName = $"Slot {id + 1}";
		return file;
	}


	public bool dontSave = false;

	public bool saveInNextFrame = false;
	[Button]
	public void SaveActiveGame() {
		if (!dontSave) {
			saveInNextFrame = true;
		}
	}

	void DoSaveActiveGame() {
		earlySaveEvent?.Invoke();
		saveEvent?.Invoke();
		if(GetCurrentSave().isInARun)
			GetCurrentSave().currentRun.playtime += Time.realtimeSinceStartup - saveStartTime;
		saveStartTime = Time.realtimeSinceStartup;
		Save(ActiveSave);
	}

	public void Update() {
		if (saveInNextFrame) {
			DoSaveActiveGame();
			saveInNextFrame = false;
		}
	}

	void Save(int saveId) {
		var path = GetSaveFilePathAndFileName(saveId);

		allSaves[saveId].isRealSaveFile = true;
		SaveFile data = allSaves[saveId];

		WriteFile(path, data);
	}

	public static void WriteFile(string path, object file) {
		Directory.CreateDirectory(Path.GetDirectoryName(path));

		StreamWriter writer = new StreamWriter(path);

		fsData serialized;
		_serializer.TrySerialize(file, out serialized);
		var json = fsJsonPrinter.PrettyJson(serialized);

		writer.Write(json);
		writer.Close();

		Debug.Log($"IO OP: file \"{file.GetType()}\" saved to \"{path}\"");
	}

	[Button]
	public void Load() {
		if (loadingComplete) {
			return;
		}

		for (int i = 0; i < 3; i++) {
			var path = GetSaveFilePathAndFileName(i);
			try {
				if (File.Exists(path)) {
					allSaves[i] = ReadFile<SaveFile>(path);
				} else {
					Debug.Log($"No Data Found on slot: {i}");
					allSaves[i] = MakeNewSaveFile(i);
				}
			} catch {
				File.Delete(path);
				Debug.Log("Corrupt Data Deleted");
				allSaves[i] = MakeNewSaveFile(i);
			}
		}

		ActiveSave = -1;
		for (int i = 0; i < 3; i++) {
			if (allSaves[i].isActiveSave) {
				ActiveSave = i;
			}
		}

		if (ActiveSave == -1) {
			ActiveSave = 0;
			allSaves[ActiveSave].isActiveSave = true;
		}


		saveStartTime = Time.realtimeSinceStartup;
		earlyLoadEvent?.Invoke();
		loadEvent?.Invoke();
		loadingComplete = true;
	}

	public static T ReadFile<T>(string path) where T : class, new() {
		StreamReader reader = new StreamReader(path);
		var json = reader.ReadToEnd();
		reader.Close();

		fsData serialized = fsJsonParser.Parse(json);

		T file = new T();
		_serializer.TryDeserialize(serialized, ref file).AssertSuccessWithoutWarnings();

		Debug.Log($"IO OP: file \"{file.GetType()}\" read from \"{path}\"");
		return file;
	}


	[Serializable]
	public class SaveFile {
		public string saveName = "unnamed";
		public bool isActiveSave = false;
		public bool isRealSaveFile = false;

		public bool isInARun = false;
		public RunState currentRun = new RunState(); // assumed to be never null

		public XPProgress xpProgress = new XPProgress();
		public TutorialProgress tutorialProgress = new TutorialProgress();
	}

	[Serializable]
	public class XPProgress {
		public int xp;
	}
	
	[Serializable]
	public class TutorialProgress {
		public bool tutorialDone;
		public bool cameraDone;
		public bool cargoPutOnTrain;
		public bool scrapsScrapped;
		public bool mapTargetSelected;
		public bool levelStarted;
		
		public bool shiftToGoFast;
		public bool changeTracks;
		public int reload;
		public int repair;
		public bool directControl;
		public bool powerup;
		
		public bool trainSpeed;

		public bool levelFinishedOnce;
		public bool getRewards;
		
		public bool repairAtTheStation;
		public bool moveThingsAround;
		public bool putTheNewStuff;
	}

	[Serializable]
	public class RunState {
		public CharacterData character = new CharacterData();

		public TrainState myTrain = new TrainState();

		public int currentAct = 1;
		
		public StarMapState map = new StarMapState();
		public List<TrainModuleHolder> trainBuildings = new List<TrainModuleHolder>();

		public float playtime;
		public RunResources myResources = new RunResources();

		public List<string> powerUps = new List<string>();

		public List<string> unclaimedRewards = new List<string>();

		public bool shopInitialized = false;
		public UpgradesController.ShopState shopState;
		
		public void SetCharacter(CharacterData characterData) {
			character = characterData;
			myTrain = characterData.starterTrain.Copy();

			for (int i = 0; i < myTrain.myCarts.Count; i++) {
				for (int j = 0; j < myTrain.myCarts[i].buildingStates.Length; j++) {
					var build = myTrain.myCarts[i].buildingStates[j];

					build.ammo = -2;
				}
			}

			/*for (int i = 0; i < characterData.starterUpgrades.Length; i++) {
				upgrades.Add(characterData.starterUpgrades[i].upgradeUniqueName);
			}*/

			for (int i = 0; i < characterData.starterModules.Length; i++) {
				var mod = characterData.starterModules[i];
				trainBuildings.Add(new TrainModuleHolder(){moduleUniqueName = mod.moduleUniqueName, amount = mod.amount});
			}

			myResources = characterData.starterResources.Copy();

			powerUps = new List<string>();
			for (int i = 0; i < 3; i++) {
				powerUps.Add("");
			}
		}
	}

	[Serializable]
	public class RunResources {
		public int money = 200;
		public int scraps = 200;
		public int maxScraps = 400;
		public int ammo = 50;
		public int maxAmmo = 100;
		public int fuel = 25;
		public int maxFuel = 100;
		
		public static string GetTypeInNiceString(ResourceTypes types) {
			return types.ToString();
		}

		public RunResources Copy() {
			var copy = new RunResources();
			copy.money = money;
			copy.scraps = scraps;
			copy.maxScraps = maxScraps;
			copy.ammo = ammo;
			copy.maxAmmo = maxAmmo;
			copy.fuel = fuel;
			copy.maxFuel = maxFuel;
			return copy;
		}
	}


	[Serializable]
	public class TrainState {
		public List<CartState> myCarts = new List<CartState>();

		[Serializable]
		public class CartState {
			[Serializable]
			public class BuildingState {
				[ValueDropdown("GetAllModuleNames")]
				public string uniqueName = "";

				[HideInInspector]
				public int health = -1;
				[HideInInspector]
				public int ammo = -1;
				//[HideInInspector]
				public bool isBuildingCargo;
				//[HideInInspector]
				public string cargoReward;

				public void EmptyState() {
					uniqueName = "";
					health = -1;
					ammo = -1;
					/*cargoCost = -1;
					cargoReward = -1;*/
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
			//public string cartType = "cart";
			public BuildingState[] buildingStates;

			public CartState() {
				buildingStates = new BuildingState[8];
				for (int i = 0; i < buildingStates.Length; i++) {
					buildingStates[i] = new BuildingState();
				}
			}

			public CartState Copy() {
				var copyState = new CartState();
				//copyState.cartType = cartType;
				for (int i = 0; i < buildingStates.Length; i++) {
					copyState.buildingStates[i] = buildingStates[i];
				}

				return copyState;
			}
		}

		public TrainState Copy() {
			var copyState = new TrainState();

			for (int i = 0; i < myCarts.Count; i++) {
				copyState.myCarts.Add(myCarts[i].Copy());
			}

			return copyState;
		}
	}
}

[Serializable]
public enum ResourceTypes {
	fuel, scraps, ammo, money
}
