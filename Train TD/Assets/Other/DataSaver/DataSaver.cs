using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FullSerializer;

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

	public string GetSaveFilePathAndFileName (int index) {
		var saveName = saveName1;
		if (index == 1) {
			saveName =saveName2;
		}

		if (index == 2) {
			saveName = saveName3;
		}
		
		return Application.persistentDataPath + "/" + saveName;
	}

	public delegate void SaveYourself ();
	public static event SaveYourself earlyLoadEvent;
	public static event SaveYourself loadEvent;
	public static event SaveYourself earlySaveEvent;
	public static event SaveYourself saveEvent;


	public SaveFile GetCurrentSave() {
		return allSaves[ActiveSave];
	}

	public void SetActiveSave(int id) {
		allSaves[activeSave].isActiveSave = false;
		SaveActiveGame();
		
		ActiveSave = id;
		allSaves[activeSave].isActiveSave = true;
		SaveActiveGame();
	}

	public void ClearCurrentSave() {
		Debug.Log("Clearing Save");
		allSaves[ActiveSave] = MakeNewSaveFile(ActiveSave);
	}
	
	public SaveFile MakeNewSaveFile(int id) {
		var file = new SaveFile();
		file.isRealSaveFile = true;
		file.saveName = $"Slot {id+1}";
		return file;
	}


	public bool dontSave = false;
	public void SaveActiveGame () {
		if (!dontSave) {
			earlySaveEvent?.Invoke();
			saveEvent?.Invoke();
			Save(ActiveSave);
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

	public void Load () {
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


	[System.Serializable]
	public class SaveFile {
		public string saveName = "unnamed";
		public bool isActiveSave = false;
		public bool isRealSaveFile = false;

		public int money = 0;

		public int reputation {
			get {
				var rep = 0;
				foreach (var mission in missionStatsList) {
					rep += mission.isWon ? 1 : 0 + mission.cargoStars + mission.speedStars;
				}

				return rep;
			}
		}

		public List<MissionStats> missionStatsList = new List<MissionStats>();
		public List<UpgradeData> upgradeDatas = new List<UpgradeData>();


		public MissionStats GetCurrentMission() {
			var mySave = s.GetCurrentSave();
			var existingMissionIndex =  mySave.missionStatsList.FindIndex(x => x.levelName == SceneLoader.s.currentLevel.levelName);
			MissionStats myMission;
			if (existingMissionIndex != -1) {
				myMission = mySave.missionStatsList[existingMissionIndex];
			} else {
				myMission = new MissionStats(){levelName =  SceneLoader.s.currentLevel.levelName};
				mySave.missionStatsList.Add(myMission);
			}

			return myMission;
		}
	}

	
	[Serializable]
	public class UpgradeData {
		public string upgradeName;
		public bool isUnlocked;
	}

	[Serializable]
	public class MissionStats {
		public string levelName;
		public bool isWon = false;
		public int speedStars = 0;
		public int cargoStars = 0;
		public float bestTime = 60000; // aka infinity
		public int bestCargoCount = 0;
	}


}
