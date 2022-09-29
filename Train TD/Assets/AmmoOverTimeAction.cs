using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoOverTimeAction : ModuleAction, IActiveDuringCombat {
    public float ammoPerSecond = 20;
    public float duration = 10;

    protected override void _EngageAction() {
        StartCoroutine(RepairOverTime());
    }


    IEnumerator RepairOverTime() {
        var timer = duration;
        var healths = LevelReferences.s.train.GetComponentsInChildren<ModuleAmmo>();

        while (timer > 0) {


            for (int i = 0; i < healths.Length; i++) {
                healths[i].Resupply(Mathf.CeilToInt(ammoPerSecond * 0.5f));
            }


            timer -= Time.deltaTime;
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    public void ActivateForCombat() {
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }
}
