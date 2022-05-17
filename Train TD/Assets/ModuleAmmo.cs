using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleAmmo : MonoBehaviour, IResupplyAble {

    public int curAmmo = 0;
    public int maxAmmo = 100;
    public int ammoPerShot = 5;

    public float damageMultiplier = 2f;
    public float fireSpeedBoost = 1f;

    private float defFireDelay;
    private float defDamage;

    private GunModule myGunModule;

    private bool isUsingSpecialAmmo = false;
    
    
    public bool isUnlocked = false;
    public Upgrade unlockingUpgrade;
    
    private  void Start() {
        if (!isUnlocked) {
            if (UpgradesController.s.unlockedUpgrades.Contains(unlockingUpgrade.upgradeUniqueName)) {
                isUnlocked = true;
            }
        }

        if (!isUnlocked) {
            maxAmmo = 0;
            this.enabled = false;
            return;
        }
        
        myGunModule = GetComponent<GunModule>();
        defDamage = myGunModule.projectileDamage;
        defFireDelay = myGunModule.fireDelay;
        myGunModule.barrageShot.AddListener(UseAmmo);
    }

    public bool ShowAmmoBar() {
        // add option to toggle all the ammo bars?
        if (curAmmo != 0 && curAmmo != maxAmmo) {
            return true;
        } else {
            return false;
        }
    }

    public void UseAmmo() {
        curAmmo -= ammoPerShot;
        curAmmo = Mathf.Clamp(curAmmo, 0, maxAmmo);
        UpdateModuleState();
    }

    public void Resupply(int amount) {
        curAmmo += amount;
        curAmmo = Mathf.Clamp(curAmmo, 0, maxAmmo);
        UpdateModuleState();
    }

    private bool isCurrentlyUsingSpecialAmmo = false;
    void UpdateModuleState() {
        isUsingSpecialAmmo = curAmmo > ammoPerShot;

        if (isCurrentlyUsingSpecialAmmo != isUsingSpecialAmmo) {
            if (isUsingSpecialAmmo) {
                myGunModule.projectileDamage *= damageMultiplier;
                myGunModule.fireDelay /= fireSpeedBoost;
            } else {
                myGunModule.projectileDamage /= damageMultiplier;
                myGunModule.fireDelay *= fireSpeedBoost;
            }
        }

        isCurrentlyUsingSpecialAmmo = isUsingSpecialAmmo;
    }

    public int MissingSupplies() {
        return curAmmo - maxAmmo;
    }

    public Transform GetResupplyEffectSpawnTransform() {
        return transform;
    }
}
