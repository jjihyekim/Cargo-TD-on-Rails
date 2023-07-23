using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class ModuleAmmo : MonoBehaviour, IActiveDuringCombat, IActiveDuringShopping {

    [ShowInInspector]
    public float curAmmo { get; private set; }
    public int _maxAmmo = 100;
    public float maxAmmoMultiplier = 1f;
    public bool isFire;
    public bool isSticky;
    
    public int maxAmmo {
        get { return Mathf.RoundToInt(_maxAmmo * maxAmmoMultiplier); }
    }
    
    public float ammoPerBarrage = 1;
    public float ammoPerBarrageMultiplier = 1;

    public GunModule[] myGunModules;

    private bool listenerAdded = false;

    float AmmoUseWithMultipliers() {
        var ammoUse = ammoPerBarrage * ammoPerBarrageMultiplier;

        /*if (myGunModule.beingDirectControlled)
            ammoUse /= DirectControlMaster.s.directControlAmmoConservationBoost;*/

        ammoUse /= TweakablesMaster.s.myTweakables.magazineSizeMultiplier;

        return ammoUse;
    }

    public void ResetState() {
        ammoPerBarrageMultiplier = 1;
        maxAmmoMultiplier = 1;
        reloadEfficiency = 1;
        ChangeMaxAmmo(0);
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


    public float reloadEfficiency = 1;

    [Button]
    public void Reload(float amount = -1, bool showEffect = true) {
        amount *= reloadEfficiency;
        if (curAmmo < maxAmmo) {
            if (amount < 0) {
                amount = maxAmmo;
            }

            if (showEffect)
                Instantiate(LevelReferences.s.reloadEffect_regular, transform);

            curAmmo += amount;
            curAmmo = Mathf.Clamp(curAmmo, 0, maxAmmo);
            
        }

        UpdateModuleState();
        OnReload?.Invoke(showEffect);
    }

    public void ApplyBulletEffect(PlayerWorldInteractionController.CursorState effect) {
        switch (effect) {
            case PlayerWorldInteractionController.CursorState.reload_fire:
                if (!isFire) {
                    Instantiate(LevelReferences.s.reloadEffect_fire, transform);
                    isFire = true;
                    OnAmmoTypeChange?.Invoke();
                }

                break;
            case PlayerWorldInteractionController.CursorState.reload_sticky:
                if (!isSticky) {
                    Instantiate(LevelReferences.s.reloadEffect_sticky, transform);
                    isSticky = true;
                    OnAmmoTypeChange?.Invoke();
                }
                break;
        }
        
        for (int i = 0; i < myGunModules.Length; i++) {
            myGunModules[i].isFire = isFire;
            myGunModules[i].isSticky = isSticky;
        }
    }
    
    public void SetAmmo(int amount, bool _isFire, bool _isSticky) {
        curAmmo = amount;
        curAmmo = Mathf.Clamp(curAmmo, 0, maxAmmo);

        isFire = _isFire;
        isSticky = _isSticky;
        
        for (int i = 0; i < myGunModules.Length; i++) {
            myGunModules[i].isFire = isFire;
            myGunModules[i].isSticky = isSticky;
        }
        
        
        OnAmmoTypeChange?.Invoke();
        UpdateModuleState();
        OnReload?.Invoke(false);
    }
    
    public void ChangeMaxAmmo(float multiplierChange) {
        maxAmmoMultiplier += multiplierChange;
        if (PlayStateMaster.s.isCombatInProgress()) {
            curAmmo = Mathf.Clamp(curAmmo, 0, maxAmmo);
        } else {
            if(PlayerWorldInteractionController.s.autoReloadAtStation)
                curAmmo = maxAmmo;
            else
                curAmmo = Mathf.Clamp(curAmmo, 0, maxAmmo);
        }

        OnUse?.Invoke();
        OnReload?.Invoke(false);
    }

    public UnityEvent OnUse;
    public UnityEvent<bool> OnReload;
    public UnityEvent OnAmmoTypeChange;

    [ReadOnly]
    public GameObject myUINoAmmoWarningThing;
    void UpdateModuleState() {
        var hasAmmo = curAmmo >= AmmoUseWithMultipliers() ;

        for (int i = 0; i < myGunModules.Length; i++) {
            myGunModules[i].hasAmmo = hasAmmo;
        }
        

        if (myUINoAmmoWarningThing == null) {
            myUINoAmmoWarningThing = Instantiate(LevelReferences.s.noAmmoWarning,LevelReferences.s.uiDisplayParent);
            myUINoAmmoWarningThing.GetComponent<UIElementFollowWorldTarget>().SetUp(GetComponentInParent<Cart>().GetUITargetTransform());
        }
        
        myUINoAmmoWarningThing.SetActive(!hasAmmo);
        

        /*if (GetComponent<EngineModule>())
            GetComponent<EngineModule>().hasFuel = curAmmo > 0;*/

        if (!hasAmmo) {
            isFire = false;
            isSticky = false;
            
            for (int i = 0; i < myGunModules.Length; i++) {
                myGunModules[i].isFire = isFire;
                myGunModules[i].isSticky = isSticky;
            }
            OnAmmoTypeChange?.Invoke();
        }
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
