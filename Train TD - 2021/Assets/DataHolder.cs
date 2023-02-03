using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DataHolder : MonoBehaviour {
    public static DataHolder s;

    public GameObject cartPrefab;
    public float cartLength;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init() { s = null; }
    
    private void Awake() {
        if (s == null)
            s = this;
        //ResetTweakable();
    }


    public TrainBuilding[] buildings;
    public EnemyHolder[] enemies;
    public CharacterDataScriptable[] characters;
    public CityDataScriptable[] cities;
    public EncounterTitle[] encounters;

    public LevelDataScriptable[] levels;
    
    public Color moneyBackColor = Color.green;

    public GameObject repairPrefab;
    public GameObject sellPrefab;
    //public GameObject trainModuleGenericExplodePrefab;

    //public AudioClip[] sellSounds;

    
    /*[SerializeField]
    private TweakablesParentScriptable myTweakable;
    public TweakablesParent fullCopy;

    public void OverrideData(TweakablesParent data) {
        fullCopy = data;
        tweakableChanged?.Invoke(fullCopy);
        var state = Train.s.GetTrainState();
        Train.s.DrawTrain(state);
    }

    public void ResetTweakable() {
        fullCopy = myTweakable.myTweakable.Clone();
    }


    public UnityEvent<TweakablesParent> tweakableChanged;
    
    /// <summary>
    /// Make sure to also sign up to the "tweakableChanged" event if you are getting tweaks.
    /// </summary>
    public TweakablesParent GetTweaks() {
        return fullCopy;
    }*/

    public GameObject GetEncounter(string encounterUniqueName) {
        for (int i = 0; i < encounters.Length; i++) {
            if ("e_" + PreProcess(encounters[i].gameObject.name) == PreProcess(encounterUniqueName)) {
                return encounters[i].gameObject;
            }
        }

        Debug.LogError($"Can't find encounter {encounterUniqueName}");
        return null;
    }
    
    public LevelData GetLevel(string levelUniqueName) {
        if (levelUniqueName.StartsWith("e_")) {
            return new LevelData() { levelName = levelUniqueName, isEncounter = true };
        }
        
        for (int i = 0; i < levels.Length; i++) {
            var data = levels[i].GetData();
            if (PreProcess(data.levelName) == PreProcess(levelUniqueName)) {
                return data;
            }
        }

        Debug.LogError($"Can't find level {levelUniqueName}");
        return null;
    }
    
    public Sprite GetCitySprite(string cityNameSuffix) {
        for (int i = 0; i < cities.Length; i++) {
            if (PreProcess(cities[i].cityData.nameSuffix) == PreProcess(cityNameSuffix)) {
                return cities[i].sprite;
            }
        }

        Debug.LogError($"Can't find city {cityNameSuffix}");
        return null;
    }
    
    public GameObject GetCityPrefab(string cityNameSuffix) {
        for (int i = 0; i < cities.Length; i++) {
            if (PreProcess(cities[i].cityData.nameSuffix) == PreProcess(cityNameSuffix)) {
                return cities[i].worldMapCastle;
            }
        }

        Debug.LogError($"Can't find city {cityNameSuffix}");
        return null;
    }

    public TrainBuilding GetBuilding(string buildingName) {
        for (int i = 0; i < buildings.Length; i++) {
            if (PreProcess(buildings[i].uniqueName) == PreProcess(buildingName)) {
                return buildings[i];
            }
        }

        Debug.LogError($"Can't find building {buildingName}");
        return null;
    }
    
    public GameObject GetEnemy(string enemyName) {
        for (int i = 0; i < enemies.Length; i++) {
            if (PreProcess(enemies[i].uniqueName) == PreProcess(enemyName)) {
                return enemies[i].data;
            }
        }

        Debug.LogError($"Can't find enemy {enemyName}");
        return null;
    }
    
    public CharacterData GetCharacter(string charName) {
        for (int i = 0; i < characters.Length; i++) {
            if (PreProcess(characters[i].myCharacter.uniqueName) == PreProcess(charName)) {
                return characters[i].myCharacter;
            }
        }

        Debug.LogError($"Can't find character {charName}");
        return null;
    }

    string PreProcess(string input) {
        return input.Replace(" ", "").ToLower();
    } 
}


[Serializable]
public class EnemyHolder {
    public string uniqueName = "unset";
    public GameObject data;
}