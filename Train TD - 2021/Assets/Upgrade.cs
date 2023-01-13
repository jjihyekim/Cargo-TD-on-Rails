using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Upgrade")]
public class Upgrade : ScriptableObject {
    public string upgradeUniqueName = "unset";
    
    [NonSerialized]
    public bool isUnlocked = false;


    public int shopCost = 100;

    public int activationCost = -1;
    
    [Space]
    
    public Sprite icon;
    public string upgradeName;
    
    [TextArea(2,20)]
    public string upgradeDescription;

    public ModuleUnlockUpgrade parentUpgrade;

    public virtual void Initialize() {
        var mySave = DataSaver.s.GetCurrentSave();

        if (parentUpgrade == this) {
            isUnlocked = true;
        } else {

            var index = mySave.currentRun.upgrades.FindIndex(x => x == upgradeUniqueName);

            if (index != -1) {
                isUnlocked = true;
            }
        }

        ApplyUpgradeEffects();
    }

    void UnlockUpgrade() {
        var mySave = DataSaver.s.GetCurrentSave();
        mySave.currentRun.upgrades.Add(upgradeUniqueName);
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
