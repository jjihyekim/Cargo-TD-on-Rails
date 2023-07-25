using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleAmmoProvider : ActivateWhenAttachedToTrain {

    public int boostReloadCountPerLevel = 1;
    
    [Space]
    public float boostFireDamageBase = 0;
    public float boostFireDamagePercentPerLevel = 0;
    public float activeFireDamageBoost = 0;
    
    [Space]
    public float boostExplosionRangeBase = 0;
    public float boostExplosionRangePercentPerLevel = 0;
    public float activeExplosionRangeBoost = 0;
    public void ResetState(int level) {
        PlayerWorldInteractionController.s.reloadAmountPerClickBoost += (boostReloadCountPerLevel*level);
        activeFireDamageBoost = boostFireDamageBase + (boostFireDamagePercentPerLevel * level);
        activeExplosionRangeBoost = boostExplosionRangeBase + (boostExplosionRangePercentPerLevel * level);
    }

    protected override void _AttachedToTrain() {
        for (int i = 0; i < Train.s.carts.Count; i++) {
            ApplyBoost(Train.s.carts[i], true);
        }
    }

    protected override bool CanApply(Cart target) {
        return target.GetComponentInChildren<GunModule>() != null;
    }

    protected override void _ApplyBoost(Cart target, bool doApply) {
        if (doApply) {
            foreach (var gunModule in target.GetComponentsInChildren<GunModule>()) {
                gunModule.regularToBurnDamageConversionMultiplier += activeFireDamageBoost;
                gunModule.regularToRangeConversionMultiplier += activeExplosionRangeBoost;
            }
        }
    }

    protected override void _DetachedFromTrain() {
        // do nothing
    }
}
