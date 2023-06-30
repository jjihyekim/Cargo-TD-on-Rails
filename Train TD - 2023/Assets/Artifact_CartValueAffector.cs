using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact_CartValueAffector : ActivateWhenOnArtifactRow {

    public bool engineOnly;
    public bool cargoOnly;
    
    public float addShieldAsHpPercent = 0f;
    public float addShieldAmount = 0;

    public float damageMultiplier = 1;
    public float healthMultiplier = 1;
    public float ammoMultiplier = 1;


    protected override void _Arm() {
        for (int i = 0; i < Train.s.carts.Count; i++) {
            var cart = Train.s.carts[i];

            if (engineOnly && !cart.isMainEngine) {
                return;
            }

            if (cargoOnly && (!cart.isCargo && !cart.isMysteriousCart)) {
                return;
            }

            if (cart.GetHealthModule() != null) {
                cart.GetHealthModule().maxShields += addShieldAsHpPercent * cart.GetHealthModule().maxHealth;
                cart.GetHealthModule().maxShields += addShieldAmount;
                
                cart.GetHealthModule().maxHealth *= healthMultiplier;
                cart.GetHealthModule().currentHealth *= healthMultiplier;
            }

            foreach (var gunModule in cart.GetComponentsInChildren<GunModule>()) {
                gunModule.artifactDamageMultiplier += damageMultiplier-1;
            }

            if (cart.GetComponentInChildren<ModuleAmmo>() != null) {
                cart.GetComponentInChildren<ModuleAmmo>().ChangeMaxAmmo(ammoMultiplier-1);
            }
        }
    }

    protected override void _Disarm() {
        for (int i = 0; i < Train.s.carts.Count; i++) {
            var cart = Train.s.carts[i];
            
            if (engineOnly && !cart.isMainEngine) {
                return;
            }

            if (cargoOnly && (!cart.isCargo && !cart.isMysteriousCart)) {
                return;
            }

            if (cart.GetHealthModule() != null) {
                cart.GetHealthModule().maxHealth /= healthMultiplier;
                cart.GetHealthModule().currentHealth /= healthMultiplier;
                
                cart.GetHealthModule().maxShields -= addShieldAmount;
                cart.GetHealthModule().maxShields -= addShieldAsHpPercent * cart.GetHealthModule().maxHealth;
            }

            foreach (var gunModule in cart.GetComponentsInChildren<GunModule>()) {
                gunModule.artifactDamageMultiplier -= damageMultiplier-1;
            }
            
            if (cart.GetComponentInChildren<ModuleAmmo>() != null) {
                cart.GetComponentInChildren<ModuleAmmo>().ChangeMaxAmmo(-(ammoMultiplier-1));
            }
        }
    }
}
