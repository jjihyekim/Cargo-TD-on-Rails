using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadeModule : ActivateWhenAttachedToTrain, IExtraInfo
{
    public void ProtectFromDamage(float damage) {
        GetComponentInParent<ModuleHealth>().DealDamage(damage);
    }
    
    public void ProtectFromBurn(float damage) {
        GetComponentInParent<ModuleHealth>().BurnDamage(damage);
    }
    
    
    protected override void _AttachedToTrain() {
        ApplyBoost(Train.s.GetNextBuilding(true, GetComponentInParent<Cart>()), true);
        ApplyBoost(Train.s.GetNextBuilding(false, GetComponentInParent<Cart>()), true);
    }

    protected override bool CanApply(Cart target) {
        var health = target.GetComponentInChildren<ModuleHealth>();
        return health != null;
    }

    protected override void _ApplyBoost(Cart target, bool doApply) {
        var health = target.GetComponentInChildren<ModuleHealth>();
        if (doApply) {
            health.damageDefenders.Add(ProtectFromDamage);
            health.burnDefenders.Add(ProtectFromBurn);
        } else {
            health.damageDefenders.Remove(ProtectFromDamage);
            health.burnDefenders.Remove(ProtectFromBurn);
        }
    }

    protected override void _DetachedFromTrain() {
        ApplyBoost(Train.s.GetNextBuilding(true, GetComponentInParent<Cart>()), false);
        ApplyBoost(Train.s.GetNextBuilding(false, GetComponentInParent<Cart>()), false);
    }

    public string GetInfoText() {
        return "Redirects damage from attached carts to itself";
    }
}
