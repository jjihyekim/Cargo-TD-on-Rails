using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetRidOfCargoAction : ModuleAction, IActiveDuringCombat {

    protected override void _EngageAction() {
        GetComponent<ModuleHealth>().Die();
    }

    public void ActivateForCombat() {
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }
}

