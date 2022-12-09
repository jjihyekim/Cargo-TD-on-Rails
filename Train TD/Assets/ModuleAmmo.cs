using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleAmmo : MonoBehaviour, IResupplyAble, IActiveDuringCombat, IActiveDuringShopping {

    public float curAmmo { get; private set; }
    public int maxAmmo = 100;
    public float ammoPerShot = 1;

    private GunModule myGunModule;

    private bool listenerAdded = false;
    

    public bool ShowAmmoBar() {
        // add option to toggle all the ammo bars?
        return true;
        /*if (curAmmo != 0 && curAmmo != maxAmmo) {
            return true;
        } else {
            return false;
        }*/
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
        myGunModule.hasAmmo = curAmmo >= ammoPerShot;
    }

    public int MissingSupplies() {
        return (int)curAmmo - maxAmmo;
    }

    public Transform GetResupplyEffectSpawnTransform() {
        return transform;
    }

    public void SetAmmo(float ammo) {
        curAmmo = ammo;
        myGunModule = GetComponent<GunModule>();
        UpdateModuleState();
    }

    public void ActivateForCombat() {
        this.enabled = true;
        
        myGunModule = GetComponent<GunModule>();
        if (!listenerAdded) {
            myGunModule.barrageShot.AddListener(UseAmmo);
            listenerAdded = true;
        }

        UpdateModuleState();
        GetComponent<ModuleHealth>().dieEvent.AddListener(GiveBackStoredAmmo);
        GetComponent<SellAction>()?.sellEvent.AddListener(GiveBackStoredAmmo);
    }

    public void ActivateForShopping() {
        ActivateForCombat();
        GetComponent<ModuleHealth>().dieEvent.AddListener(GiveBackStoredAmmo);
        GetComponent<SellAction>()?.sellEvent.AddListener(GiveBackStoredAmmo);
    }


    public void Disable() {
        this.enabled = false;
        myGunModule = GetComponent<GunModule>();
        GetComponent<ModuleHealth>().dieEvent.RemoveListener(GiveBackStoredAmmo);
        GetComponent<SellAction>()?.sellEvent.RemoveListener(GiveBackStoredAmmo);
    }
    
    
    void GiveBackStoredAmmo() {
        GetComponent<ReloadAction>().GiveBackCurrentStoredAmmo();
    }
}
