using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact_GlassTrain : ActivateWhenOnArtifactRow {
    public float damageMultiplier = 20;
    public float healthMultiplier = 10;

    protected override void _Arm() {
        for (int i = 0; i < Train.s.carts.Count; i++) {
            var cart = Train.s.carts[i];

            cart.GetHealthModule().maxHealth /= healthMultiplier;
            cart.GetHealthModule().currentHealth /= healthMultiplier;

            foreach (var gunModule in cart.GetComponentsInChildren<GunModule>()) {
                gunModule.artifactDamageMultiplier += damageMultiplier-1;
            }
        }
    }

    protected override void _Disarm() {
        for (int i = 0; i < Train.s.carts.Count; i++) {
            var cart = Train.s.carts[i];

            cart.GetHealthModule().maxHealth *= healthMultiplier;
            cart.GetHealthModule().currentHealth *= healthMultiplier;

            foreach (var gunModule in cart.GetComponentsInChildren<GunModule>()) {
                gunModule.artifactDamageMultiplier -= damageMultiplier-1;
            }
        }
    }
}
