using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Character")]
public class CharacterData : ScriptableObject {
    public string uniqueName;

    public Upgrade[] starterUpgrades;
    public DataSaver.TrainState starterTrain;
    
    
    public int starterMoney = 0;
    public int starterScraps = 200;
    public int starterFuel = 50;
    public int maxFuel = 100;
}
