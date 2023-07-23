using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact_CartValueAffector : ActivateWhenOnArtifactRow {

    public bool engineOnly;
    public bool cargoOnly;
    
    public float addShieldAsHpPercent = 0f;
    public float addShieldAmount = 0;

    public float fireRateMultiplier = 1;
    public float damageMultiplier = 1;
    public float healthMultiplier = 1;
    public float ammoMultiplier = 1;

    public float addFireDamage = 0;

    public bool canHaveNoShields = false;

    public int boosterRangeModification = 0;
    public float boosterEffectMultiplier = 1;

    public float repairEfficiency = 1;
    public float reloadEfficiency = 1;
    public float shieldUpEfficiency = 1;
    public float shieldDecayMultiplier = 1f;


    protected override void _Arm() {

        var targets = new List<Cart>();
        var myCart = GetComponentInParent<Cart>();
        
        targets.Add(myCart);
        targets.Add(Train.s.GetNextBuilding(1,myCart));
        targets.Add(Train.s.GetNextBuilding(-1,myCart));
        
        for (int i = 0; i < targets.Count; i++) {
            var cart = targets[i];
            if(cart == null)
                continue;

            if (engineOnly && !cart.isMainEngine) {
                return;
            }

            if (cargoOnly && (!cart.isCargo && !cart.isMysteriousCart)) {
                return;
            }

            var healthModule = cart.GetHealthModule();
            

            if (healthModule != null) {
                if (canHaveNoShields) {
                    healthModule.canHaveShields = false;
                    healthModule.maxShields = 0;
                }
                
                if (healthModule.canHaveShields) {
                    healthModule.maxShields += addShieldAsHpPercent * healthModule.maxHealth;
                    healthModule.maxShields += addShieldAmount;
                }

                healthModule.maxHealth *= healthMultiplier;
                healthModule.currentHealth *= healthMultiplier;

                healthModule.repairEfficiency += repairEfficiency - 1;
                healthModule.shieldUpEfficiency += shieldUpEfficiency - 1;
            }

            foreach (var gunModule in cart.GetComponentsInChildren<GunModule>()) {
                gunModule.damageMultiplier += damageMultiplier-1;
                gunModule.fireRateMultiplier += fireRateMultiplier - 1;
                gunModule.bonusBurnDamage += addFireDamage;
            }

            var moduleAmmo = cart.GetComponentInChildren<ModuleAmmo>();
            if (moduleAmmo != null) {
                moduleAmmo.ChangeMaxAmmo(ammoMultiplier-1);
                moduleAmmo.reloadEfficiency += reloadEfficiency - 1;
            }

            foreach (var booster in cart.GetComponentsInChildren<IBooster>()) {
                booster.ModifyStats(boosterRangeModification, boosterEffectMultiplier-1f);
            }
        }
    }

    protected override void _Disarm() {
        // do nothing
    }
}
