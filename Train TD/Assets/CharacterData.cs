using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Character")]
public class CharacterData : ScriptableObject {
    public string uniqueName;

    public Upgrade[] starterUpgrades;
    public DataSaver.TrainState starterTrain;


    public DataSaver.RunResources starterResources;
}
