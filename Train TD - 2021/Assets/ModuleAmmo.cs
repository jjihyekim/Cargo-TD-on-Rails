using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ModuleAmmo : MonoBehaviour, IResupplyAble, IActiveDuringCombat, IActiveDuringShopping {

    public float curAmmo { get; private set; }
    public int maxAmmo = 100;
    public float ammoPerShot = 1;

    private GunModule myGunModule;

    private bool listenerAdded = false;

    public void UseAmmo() {
        var ammoUse = ammoPerShot;

        if (myGunModule.beingDirectControlled)
            ammoUse /= DirectControlMaster.s.directControlAmmoConservationBoost;
        
        curAmmo -= ammoUse;
        
        curAmmo = Mathf.Clamp(curAmmo, 0, maxAmmo);
        UpdateModuleState();
    }

    public void UseFuel(float amount) {
        curAmmo -= amount;
        curAmmo = Mathf.Clamp(curAmmo, 0, maxAmmo);
        UpdateModuleState();
    }

    public void Resupply(int amount) {
        curAmmo += amount;
        curAmmo = Mathf.Clamp(curAmmo, 0, maxAmmo);
        UpdateModuleState();
    }

    void UpdateModuleState() {
        if (myGunModule != null) {
            myGunModule.hasAmmo = curAmmo >= ammoPerShot;
        }

        if (GetComponent<EngineModule>())
            GetComponent<EngineModule>().hasFuel = curAmmo > 0;
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

    private bool giveBackAmmoListenerAdded = false;

    public void ActivateForCombat() {
        this.enabled = true;

        myGunModule = GetComponent<GunModule>();
        if (!listenerAdded && myGunModule != null) {
            myGunModule.barrageShot.AddListener(UseAmmo);
            listenerAdded = true;
        }

        UpdateModuleState();

        if (!giveBackAmmoListenerAdded) {
            giveBackAmmoListenerAdded = true;
            GetComponent<ModuleHealth>().dieEvent.AddListener(GiveBackStoredAmmo);
            GetComponent<SellAction>()?.sellEvent.AddListener(GiveBackStoredAmmo);
        }
    }

    public void ActivateForShopping() {
        ActivateForCombat();
    }


    public void Disable() {
        this.enabled = false;
        myGunModule = GetComponent<GunModule>();
        if (giveBackAmmoListenerAdded) {
            giveBackAmmoListenerAdded = false;
            GetComponent<ModuleHealth>().dieEvent.RemoveListener(GiveBackStoredAmmo);
            GetComponent<SellAction>()?.sellEvent.RemoveListener(GiveBackStoredAmmo);
        }
    }
    
    
    void GiveBackStoredAmmo() {
        GetComponent<ReloadAction>().GiveBackCurrentStoredAmmo();
    }

    [Button]
    public void FillAmmoDebug() {
        Resupply(1000000);
    }
}
