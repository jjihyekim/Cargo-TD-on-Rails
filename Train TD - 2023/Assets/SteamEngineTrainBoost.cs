using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamEngineTrainBoost : ActivateWhenAttachedToTrain, IExtraInfo
{

    protected override void _AttachedToTrain() {
        for (int i = 0; i < Train.s.carts.Count; i++) {
            ApplyBoost(Train.s.carts[i], true);
        }
    }

    protected override bool CanApply(Cart target) {
        var ammo = target.GetComponentInChildren<ModuleAmmo>();
        var guns = target.GetComponentsInChildren<GunModule>();
        return ammo == null && guns.Length > 0;
    }

    protected override void _ApplyBoost(Cart target, bool doApply) {
        var guns = target.GetComponentsInChildren<GunModule>();
        if (doApply) {
            for (int j = 0; j < guns.Length; j++) {
                guns[j].projectileDamage *= 1.2f;
            }
        } else {
            for (int j = 0; j < guns.Length; j++) {
                guns[j].projectileDamage /= 1.2f;
            }
        }
    }

    protected override void _DetachedFromTrain() {
        for (int i = 0; i < Train.s.carts.Count; i++) {
            ApplyBoost(Train.s.carts[i], false);
        }
    }
    
    public string GetInfoText() {
        return "Boosts the damage of every gun that doesn't need ammo by 25%";
    }
}

