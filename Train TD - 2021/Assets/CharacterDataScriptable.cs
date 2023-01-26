using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Character")]
public class CharacterDataScriptable : ScriptableObject {
    public CharacterData myCharacter;
}


[Serializable]
public class CharacterData {
    public string uniqueName = "unset";
    [TextArea]
    public string description;

    /*[Tooltip("No need to include unlock upgrades anymore")]
    public Upgrade[] starterUpgrades;*/
    [Tooltip("You should include anything that is already on the train if you want player to be able to rebuild it")]
    public TrainModuleHolder[] starterModules;
    public DataSaver.TrainState starterTrain;

    public DataSaver.RunResources starterResources;
}

[Serializable]
public class TrainModuleHolder {
    [ValueDropdown("GetAllModuleNames")]
    public string moduleUniqueName = "unset";
    
    public int amount = 1;

    private static IEnumerable GetAllModuleNames() {
        var buildings = GameObject.FindObjectOfType<DataHolder>().buildings;
        var buildingNames = new List<string>();
        for (int i = 0; i < buildings.Length; i++) {
            buildingNames.Add(buildings[i].uniqueName);
        }
        return buildingNames;
    }


    public TrainModuleHolder Copy() {
        return new TrainModuleHolder() { moduleUniqueName = moduleUniqueName, amount = amount };
    }
}
