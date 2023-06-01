using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBoostModule : ActivateWhenAttachedToTrain, IExtraInfo
{
    public float damageBoost = 1;
    
    protected override void _AttachedToTrain() {
        ApplyBoost(Train.s.GetNextBuilding(true, GetComponentInParent<Cart>()), true);
        ApplyBoost(Train.s.GetNextBuilding(false, GetComponentInParent<Cart>()), true);
    }

    protected override bool CanApply(Cart target) {
        var gun = target.GetComponentInChildren<GunModule>();
        return gun != null;
    }

    protected override void _ApplyBoost(Cart target, bool doApply) {
        var gun = target.GetComponentInChildren<GunModule>();
        if (doApply) {
            gun.damageMultiplier += damageBoost;
        } else {
            gun.damageMultiplier -= damageBoost;
        }
    }

    protected override void _DetachedFromTrain() {
        ApplyBoost(Train.s.GetNextBuilding(true, GetComponentInParent<Cart>()), false);
        ApplyBoost(Train.s.GetNextBuilding(false, GetComponentInParent<Cart>()), false);
    }
    
    public string GetInfoText() {
        return "Doubles the damage of connected carts";
    }
}
