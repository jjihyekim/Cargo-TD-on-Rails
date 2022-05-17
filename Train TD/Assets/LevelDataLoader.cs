using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;

public class LevelDataLoader : MonoBehaviour {
    public static LevelDataLoader s;

    // public const string levelDataFolderName = "Levels";
    //
    // public static string GetLevelDataPath () {
    //     return Path.Combine( Application.dataPath, levelDataFolderName);
    // }
    //
    //
    // public static string[] GetAllLevelDataPaths() {
    //     return Directory.GetFiles(GetLevelDataPath(), "*.json");
    // }
    

    private void Awake() {
        if (s == null) {
            s = this;
            LoadLevels();
        }

        //MoveScriptablesToJson();
    }


    public LevelDataScriptable[] levelDataScriptables;
    
    public List<LevelData> allLevels = new List<LevelData>();

    void LoadLevels() {
        allLevels = new List<LevelData>();

        for (int i = 0; i < levelDataScriptables.Length; i++) {
            allLevels.Add(ConvertScriptableToLevelData(levelDataScriptables[i]));
        }
        
        Debug.Log("Levels Loaded");
    }
    
    // void LoadLevels() {
    //     var allPaths = GetAllLevelDataPaths();
    //     allLevels = new List<LevelDataJson>();
    //
    //     
    //     Debug.Log($"Loading {allPaths.Length} levels from {GetLevelDataPath()}");
    //
    //     for (int i = 0; i < allPaths.Length; i++) {
    //         var level = DataSaver.ReadFile<LevelDataJson>(allPaths[i]);
    //
    //         if (level.levelName == "Test Level") {
    //             allLevels.InsertWithNullFill(0,level);
    //         }else if (level.levelMenuOrder > 0) {
    //             allLevels.InsertWithNullFill(level.levelMenuOrder,level);
    //         } else {
    //             allLevels.Add(level);
    //         }
    //     }
    //
    //     allLevels.RemoveAll(item => item == null);
    //
    //     for (int i = 0; i < allLevels.Count; i++) {
    //         if (i != allLevels[i].levelMenuOrder) {
    //             allLevels[i].levelMenuOrder = i;
    //             SaveLevel(allLevels[i]);
    //         }
    //     }
    // }
    //
    // public void SaveLevel(LevelData level) {
    //     var path = Path.Combine(GetLevelDataPath(), level.levelName + ".json");
    //     Debug.Log($"Saving level:\"{level.levelName}\" to \"{path}\"");
    //     DataSaver.WriteFile(path, level);
    // }


    // [Button]
    // void MoveJsonToScriptables() {
    //     var allPaths = GetAllLevelDataPaths();
    //     var allLevels = new List<LevelDataJson>();
    //
    //     
    //     Debug.Log($"Loading {allPaths.Length} levels from {GetLevelDataPath()}");
    //
    //     for (int i = 0; i < allPaths.Length; i++) {
    //         var level = DataSaver.ReadFile<LevelDataJson>(allPaths[i]);
    //
    //         if (level.levelName == "Test Level") {
    //             allLevels.InsertWithNullFill(0,level);
    //         }else if (level.levelMenuOrder > 0) {
    //             allLevels.InsertWithNullFill(level.levelMenuOrder,level);
    //         } else {
    //             allLevels.Add(level);
    //         }
    //     }
    //
    //     allLevels.RemoveAll(item => item == null);
    //
    //     for (int i = 0; i < allLevels.Count; i++) {
    //         if (i != allLevels[i].levelMenuOrder) {
    //             allLevels[i].levelMenuOrder = i;
    //         }
    //     }
    //
    //     for (int i = 0; i < allLevels.Count; i++) {
    //         ConvertJsonToScriptable(allLevels[i]);
    //     }
    // }
    //
    // void ConvertJsonToScriptable(LevelDataJson dataJson) {
    //     var scriptable = ScriptableObject.CreateInstance<LevelData>();
    //     scriptable.levelName = dataJson.levelName;
    //     scriptable.levelMenuOrder = dataJson.levelMenuOrder;
    //     scriptable.trainLength = dataJson.trainLength;
    //     scriptable.starterModules = (TrainBuildingData[])dataJson.starterModules.Clone();
    //     scriptable.enemyWaves = (EnemyWaveData[])dataJson.enemyWaves.Clone();
    //     
    //     scriptable.startingMoney = dataJson.startingMoney;
    //     scriptable.moneyGainSpeed = dataJson.moneyGainSpeed;
    //
    //     scriptable.missionDistance = dataJson.missionDistance;
    //     
    //     scriptable.bestEngineSpeed = dataJson.bestEngineSpeed;
    //     scriptable.mediumEngineSpeed = dataJson.mediumEngineSpeed;
    //     scriptable.worstEngineSpeed = dataJson.worstEngineSpeed;
    //
    //
    //     // path has to start at "Assets"
    //     string path = $"Assets/_Scriptable Objects/Levels/{dataJson.levelName}.asset";
    //     AssetDatabase.CreateAsset(scriptable, path);
    //     AssetDatabase.SaveAssets();
    // }


    LevelData ConvertScriptableToLevelData(LevelDataScriptable input) {
        var output = new LevelData();
        output.levelName = input.levelName;
        output.levelMenuOrder = input.levelMenuOrder;
        output.trainLength = input.trainLength;
        output.starterModules = (TrainBuildingData[])input.starterModules.Clone();
        output.enemyWaves = (EnemyWaveData[])input.enemyWaves.Clone();
        
        output.startingMoney = input.startingMoney;
        output.moneyGainSpeed = input.moneyGainSpeed;
        
        output.missionDistance = input.missionDistance;
        
        output.bestEngineSpeed = input.bestEngineSpeed;
        output.mediumEngineSpeed = input.mediumEngineSpeed;
        output.worstEngineSpeed = input.worstEngineSpeed;

        return output;
    }
}
