using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class ModuleAmmo : MonoBehaviour, IActiveDuringCombat, IActiveDuringShopping {

    [ShowInInspector]
    public float curAmmo { get; private set; }
    public int maxAmmo = 100;
    public float ammoPerBarrage = 1;

    public GunModule[] myGunModules;

    private bool listenerAdded = false;

    float AmmoUseWithMultipliers() {
        var ammoUse = ammoPerBarrage;

        /*if (myGunModule.beingDirectControlled)
            ammoUse /= DirectControlMaster.s.directControlAmmoConservationBoost;*/

        ammoUse /= TweakablesMaster.s.myTweakables.magazineSizeMultiplier;

        return ammoUse;
    }
    
    public void UseAmmo() {
        curAmmo -= AmmoUseWithMultipliers();
        
        curAmmo = Mathf.Clamp(curAmmo, 0, maxAmmo);
        UpdateModuleState();
        OnUse?.Invoke();
    }

    public void UseFuel(float amount) {
        curAmmo -= amount;
        curAmmo = Mathf.Clamp(curAmmo, 0, maxAmmo);
        UpdateModuleState();
    }
    
    [Button]
    public void Reload(float amount = -1, bool showEffect = true) {
        curAmmo = Mathf.Clamp(curAmmo, 0, maxAmmo);
        if (curAmmo < maxAmmo) {
            if (amount < 0) {
                amount = maxAmmo;
            }

            if (showEffect)
                Instantiate(LevelReferences.s.reloadEffectPrefab, transform);

            curAmmo += amount;
            curAmmo = Mathf.Clamp(curAmmo, 0, maxAmmo);
            
        }

        UpdateModuleState();
        OnReload?.Invoke(showEffect);
    }

    public void ChangeMaxAmmo(int newMax) {
        maxAmmo = newMax;
        curAmmo = maxAmmo;
        OnUse?.Invoke();
        OnReload?.Invoke(false);
    }

    public UnityEvent OnUse;
    public UnityEvent<bool> OnReload;

    [ReadOnly]
    public GameObject myUINoAmmoWarningThing;
    void UpdateModuleState() {
        var hasAmmo = curAmmo > 0 ;

        for (int i = 0; i < myGunModules.Length; i++) {
            myGunModules[i].hasAmmo = hasAmmo;
        }
        

        if (myUINoAmmoWarningThing == null) {
            myUINoAmmoWarningThing = Instantiate(LevelReferences.s.noAmmoWarning,LevelReferences.s.uiDisplayParent);
            myUINoAmmoWarningThing.GetComponent<UIElementFollowWorldTarget>().SetUp(GetComponentInParent<Cart>().GetUITargetTransform());
        }
        
        myUINoAmmoWarningThing.SetActive(!hasAmmo);
        

        if (GetComponent<EngineModule>())
            GetComponent<EngineModule>().hasFuel = curAmmo > 0;
    }

    public float AmmoPercent() {
        return curAmmo / maxAmmo;
    }


    public void ActivateForCombat() {
        this.enabled = true;

        Reload(-1,false);

        myGunModules = GetComponentsInChildren<GunModule>();
        if (!listenerAdded) {

            for (int i = 0; i < myGunModules.Length; i++) {
                myGunModules[i].barrageShot.AddListener(UseAmmo);
            }
            listenerAdded = true;
        }

        UpdateModuleState();
    }

    public void ActivateForShopping() {
        ActivateForCombat();
    }


    public void Disable() {
        this.enabled = false;
        myGunModules = GetComponentsInChildren<GunModule>();
    }
    

    private void OnDestroy() {
        if (myUINoAmmoWarningThing != null) {
            Destroy(myUINoAmmoWarningThing);
        }
    }
}
