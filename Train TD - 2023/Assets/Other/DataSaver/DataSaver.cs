using UnityEngine;
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

	public float saveLockTimer = 0;

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
		if (saveInNextFrame && saveLockTimer <= 0) {
			DoSaveActiveGame();
			saveInNextFrame = false;
			saveLockTimer = 2;
		}

		if (saveLockTimer > 0) {
			saveLockTimer -= Time.deltaTime;
		}
	}

	public void CheckAndDoSave() {
		if (saveInNextFrame) {
			saveInNextFrame = false;
			DoSaveActiveGame();
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
		public bool firstCityTutorialDone;
		public bool initialCutscenePlayed;
		public bool cameraDone;

		public bool directControlHint;
		public bool reloadHint;
		public bool repairCriticalHint;
		public bool repairHint;
		public bool deliverCargoHint;
	}

	[Serializable]
	public class RunState {
		public CharacterData character = new CharacterData();

		public TrainState myTrain = new TrainState();

		public int currentAct = 1;
		
		public StarMapState map = new StarMapState();
		public string targetStar;

		public float playtime;
		public RunResources myResources = new RunResources();

		public List<string> powerUps = new List<string>();


		public bool isInEndRunArea = false;
		
		public bool shopInitialized = false;
		public UpgradesController.ShopState shopState;

		public float fleaMarketRarityBoost = -0.05f;
		public float destinationRarityBoost = -0.15f;
		
		public void SetCharacter(CharacterData characterData) {
			character = characterData;
			myTrain = characterData.starterTrain.Copy();

			shopInitialized = false;

			for (int i = 0; i < myTrain.myCarts.Count; i++) {
				var build = myTrain.myCarts[i];
				//build.ammo = -2;
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
		public int scraps = 200;
		
		public static string GetTypeInNiceString(ResourceTypes types) {
			return types.ToString();
		}

		public RunResources Copy() {
			var copy = new RunResources();
			copy.scraps = scraps;
			return copy;
		}
	}


	[Serializable]
	public class TrainState {
		public List<CartState> myCarts = new List<CartState>();

		[Serializable]
		public class CartState {
			[ValueDropdown("GetAllModuleNames")]
			public string uniqueName = "";

			public CargoState cargoState;
			
			[Serializable]
			public class CargoState { // dont forget to update the copy function
				[ValueDropdown("GetAllModuleNames")]
				public string cargoReward;
				public bool isLeftCargo;
				
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

			public void EmptyState() {
				uniqueName = "";
				//health = -1;
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

			public CartState Copy() {
				var copyState = new CartState();
				copyState.uniqueName = uniqueName;
				//copyState.health = health;
				//copyState.ammo = ammo;
				copyState.cargoState = new CargoState();
				copyState.cargoState.cargoReward = cargoState.cargoReward;
				copyState.cargoState.isLeftCargo = cargoState.isLeftCargo;
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
	scraps
}
