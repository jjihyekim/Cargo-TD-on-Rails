using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NuclearEngineTrainBoost : ActivateWhenAttachedToTrain,IExtraInfo {

    public float ammoBoost = 0.2f;
    protected override void _AttachedToTrain() {
        for (int i = 0; i < Train.s.carts.Count; i++) {
            ApplyBoost(Train.s.carts[i], true);
        }
    }

    protected override bool CanApply(Cart target) {
        var ammo = target.GetComponentInChildren<ModuleAmmo>();
        return ammo != null;
    }

    protected override void _ApplyBoost(Cart target, bool doApply) {
        var ammo = target.GetComponentInChildren<ModuleAmmo>();
        if (doApply) {
            ammo.ChangeMaxAmmo(ammoBoost);
        } else {
            ammo.ChangeMaxAmmo(-ammoBoost);
        }
    }

    protected override void _DetachedFromTrain() {
        //do nothing
    }
    
    public string GetInfoText() {
        return "Boosts ammo capacity of every cart on train by 20%";
    }
}
