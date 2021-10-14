using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelDataLoader : MonoBehaviour {
    public static LevelDataLoader s;

    public const string levelDataFolderName = "Levels";
    
    public static string GetLevelDataPath () {
        return Path.Combine( Application.dataPath, levelDataFolderName);
    }


    public static string[] GetAllLevelDataPaths() {
        return Directory.GetFiles(GetLevelDataPath(), "*.json");
    }


    private void Awake() {
        if (s == null) {
            s = this;
            LoadLevels();
        }

        //MoveScriptablesToJson();
    }


    public List<LevelDataJson> allLevels = new List<LevelDataJson>();
    void LoadLevels() {
        var allPaths = GetAllLevelDataPaths();
        allLevels = new List<LevelDataJson>();

        
        Debug.Log($"Loading {allPaths.Length} levels from {GetLevelDataPath()}");

        for (int i = 0; i < allPaths.Length; i++) {
            var level = DataSaver.ReadFile<LevelDataJson>(allPaths[i]);

            if (level.levelName == "Test Level") {
                allLevels.InsertWithNullFill(0,level);
            }else if (level.levelMenuOrder > 0) {
                allLevels.InsertWithNullFill(level.levelMenuOrder,level);
            } else {
                allLevels.Add(level);
            }
        }

        allLevels.RemoveAll(item => item == null);

        for (int i = 0; i < allLevels.Count; i++) {
            if (i != allLevels[i].levelMenuOrder) {
                allLevels[i].levelMenuOrder = i;
                SaveLevel(allLevels[i]);
            }
        }
    }

    public void SaveLevel(LevelDataJson level) {
        var path = Path.Combine(GetLevelDataPath(), level.levelName + ".json");
        Debug.Log($"Saving level:\"{level.levelName}\" to \"{path}\"");
        DataSaver.WriteFile(path, level);
    }
}
