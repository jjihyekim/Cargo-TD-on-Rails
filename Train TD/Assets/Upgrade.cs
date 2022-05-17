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
    
    [TextArea(2,20)]
    public string upgradeDescription;

    public ModuleUnlockUpgrade parentUpgrade;

    public virtual void Initialize() {
        var mySave = DataSaver.s.GetCurrentSave();

        var index = mySave.upgradeDatas.FindIndex(x => x.upgradeName == upgradeUniqueName);

        if (index != -1) {
            isUnlocked = mySave.upgradeDatas[index].isUnlocked;
        }

        // if this upgrade isnt unlocked yet, but need to be unlocked at start
        if (!isUnlocked && isUnlockedAtNewProfileStart) {
            UnlockUpgrade();
        }
        
        isUnlocked = isUnlocked || isUnlockedAtNewProfileStart;

        ApplyUpgradeEffects();
    }

    void UnlockUpgrade() {
        var mySave = DataSaver.s.GetCurrentSave();
        var index = mySave.upgradeDatas.FindIndex(x => x.upgradeName == this.upgradeUniqueName);
        if (index != -1) {
            mySave.upgradeDatas[index].isUnlocked = true;
        } else {
            mySave.upgradeDatas.Add(new DataSaver.UpgradeData() { isUnlocked = true, upgradeName = this.upgradeUniqueName });
        }
    }

    public void ApplyUpgradeEffects() {
        _ApplyUniqueName();
        
        if (isUnlocked) {
            if (!UpgradesController.s.unlockedUpgrades.Contains(upgradeUniqueName)) {
                UpgradesController.s.unlockedUpgrades.Add(upgradeUniqueName);
                if (this is ModuleUnlockUpgrade) {
                    var module = (ModuleUnlockUpgrade)this;
                    UpgradesController.s.unlockedUpgrades.Add(module.module.uniqueName);
                }
            }
        }
    }

    public virtual void _ApplyUniqueName() { }
}
