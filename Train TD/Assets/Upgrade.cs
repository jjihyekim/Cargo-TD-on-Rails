using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Upgrade")]
public class Upgrade : ScriptableObject {
    public string upgradeUniqueName = "unset";

    public bool isUnlockedAtNewProfileStart = false;
    
    [NonSerialized]
    public bool isUnlocked = false;
    
    public int cost = 100;
    public int starRequirement;
    
    [Space]
    
    public Sprite icon;
    public string upgradeName;
    [TextArea]
    public string upgradeDescription;

    public ModuleUnlockUpgrade parentUpgrade;

    public virtual void Initialize() {
        var mySave = DataSaver.s.GetCurrentSave();

        isUnlocked = isUnlockedAtNewProfileStart;

        var index = mySave.upgradeDatas.FindIndex(x => x.upgradeName == upgradeUniqueName);

        if (index != -1) {
            isUnlocked = mySave.upgradeDatas[index].isUnlocked;
        }

        ApplyUpgradeEffects();
    }

    public void ApplyUpgradeEffects() {
        _ApplyUniqueName();
        
        if (isUnlocked) {
            if (!UpgradesController.s.unlockedUpgrades.Contains(upgradeUniqueName)) {
                UpgradesController.s.unlockedUpgrades.Add(upgradeUniqueName);
            }
        }
    }

    public virtual void _ApplyUniqueName() { }
}
