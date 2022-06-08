using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class UnlockableEffect : MonoBehaviour {
    
    public bool isUnlocked = false;
    [HideIf("isUnlocked")]
    public Upgrade unlockingUpgrade;



    protected virtual void _Start() { }
    void  Start()
    {
        if (!isUnlocked) {
            if (UpgradesController.s.unlockedUpgrades.Contains(unlockingUpgrade.upgradeUniqueName)) {
                isUnlocked = true;
            }
        }

        if (!isUnlocked) {
            this.enabled = false;
        }
        _Start();
    }
}
