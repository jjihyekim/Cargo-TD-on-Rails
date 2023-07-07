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

    public bool canHaveNoShields = false;

    public int boosterRangeModification = 0;
    public float boosterEffectMultiplier = 1;


    protected override void _Arm() {
        var carts = GetAllCarts();
        for (int i = 0; i < carts.Count; i++) {
            var cart = carts[i];

            if (engineOnly && !cart.isMainEngine) {
                return;
            }

            if (cargoOnly && (!cart.isCargo && !cart.isMysteriousCart)) {
                return;
            }

            if (canHaveNoShields) {
                cart.GetHealthModule().canHaveShields = false;
                cart.GetHealthModule().maxShields = 0;
            }

            if (cart.GetHealthModule() != null) {
                if (cart.GetHealthModule().canHaveShields) {
                    cart.GetHealthModule().maxShields += addShieldAsHpPercent * cart.GetHealthModule().maxHealth;
                    cart.GetHealthModule().maxShields += addShieldAmount;
                }

                cart.GetHealthModule().maxHealth *= healthMultiplier;
                cart.GetHealthModule().currentHealth *= healthMultiplier;
            }

            foreach (var gunModule in cart.GetComponentsInChildren<GunModule>()) {
                gunModule.damageMultiplier += damageMultiplier-1;
            }

            if (cart.GetComponentInChildren<ModuleAmmo>() != null) {
                cart.GetComponentInChildren<ModuleAmmo>().ChangeMaxAmmo(ammoMultiplier-1);
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
