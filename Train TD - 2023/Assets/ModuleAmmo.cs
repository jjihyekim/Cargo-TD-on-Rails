using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ModuleAmmo : MonoBehaviour, IResupplyAble, IActiveDuringCombat, IActiveDuringShopping {

    public float curAmmo { get; private set; }
    public int maxAmmo = 100;
    public float ammoPerBarrage = 1;

    private GunModule myGunModule;

    private bool listenerAdded = false;

    float AmmoUseWithMultipliers() {
        var ammoUse = ammoPerBarrage;

        if (myGunModule.beingDirectControlled)
            ammoUse /= DirectControlMaster.s.directControlAmmoConservationBoost;

        ammoUse /= TweakablesMaster.s.myTweakables.magazineSizeMultiplier;

        return ammoUse;
    }
    
    public void UseAmmo() {
        curAmmo -= AmmoUseWithMultipliers();
        
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

    public GameObject myUINoAmmoWarningThing;
    void UpdateModuleState() {
        if (myGunModule != null) {
            myGunModule.hasAmmo = curAmmo >= AmmoUseWithMultipliers();

            if (myUINoAmmoWarningThing == null) {
                myUINoAmmoWarningThing = Instantiate(LevelReferences.s.noAmmoWarning,LevelReferences.s.uiDisplayParent);
                myUINoAmmoWarningThing.GetComponent<UIElementFollowWorldTarget>().SetUp(GetComponentInParent<Cart>().GetUITargetTransform());
            }
            
            myUINoAmmoWarningThing.SetActive(!myGunModule.hasAmmo);
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
        if (PlayStateMaster.s.isShop()) {
            Resupply(maxAmmo);
        } else {
            curAmmo = ammo;
            myGunModule = GetComponent<GunModule>();
            UpdateModuleState();
        }
    }

    private bool giveBackAmmoListenerAdded = false;

    public void ActivateForCombat() {
        this.enabled = true;

       Resupply(maxAmmo);

        myGunModule = GetComponent<GunModule>();
        if (!listenerAdded && myGunModule != null) {
            myGunModule.barrageShot.AddListener(UseAmmo);
            listenerAdded = true;
        }

        UpdateModuleState();

        if (!giveBackAmmoListenerAdded) {
            giveBackAmmoListenerAdded = true;
            GetComponent<ModuleHealth>().dieEvent.AddListener(GiveBackStoredAmmo);
            //GetComponent<SellAction>()?.sellEvent.AddListener(GiveBackStoredAmmo);
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
            //GetComponent<SellAction>()?.sellEvent.RemoveListener(GiveBackStoredAmmo);
        }
    }
    
    
    void GiveBackStoredAmmo() {
        //GetComponent<ReloadAction>().GiveBackCurrentStoredAmmo();
    }

    [Button]
    public void FillAmmoDebug() {
        Resupply(1000000);
    }

    private void OnDestroy() {
        if (myUINoAmmoWarningThing != null) {
            Destroy(myUINoAmmoWarningThing);
        }
    }
}
