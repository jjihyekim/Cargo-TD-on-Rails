using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleAmmo : MonoBehaviour, IResupplyAble {

    public int curAmmo = 0;
    public int maxAmmo = 100;
    public int ammoPerShot = 5;

    private GunModule myGunModule;
    public bool isUnlocked = false;
    public Upgrade unlockingUpgrade;
    
    private  void Start() {
        this.enabled = false;
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

    void UpdateModuleState() {
        myGunModule.CanShoot = curAmmo >= ammoPerShot;
    }

    public int MissingSupplies() {
        return curAmmo - maxAmmo;
    }

    public Transform GetResupplyEffectSpawnTransform() {
        return transform;
    }

    public void SetAmmo(int ammo) {
        curAmmo = ammo;
    }
}
