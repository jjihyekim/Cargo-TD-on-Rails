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

    public DataSaver.TrainState starterTrain;

    public DataSaver.RunResources starterResources;
}
