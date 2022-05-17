using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunOptimizationsAction : ModuleAction
{
    public float damageImprovementMultiplier = 1.1f;
    public float speedImprovementMultiplier = 1.1f;
    protected override void _EngageAction() {
        for (int i = 0; i < LevelReferences.s.train.carts.Count; i++) {
            var cart = LevelReferences.s.train.carts[i].GetComponent<Cart>();

            cart.damageModifier *= damageImprovementMultiplier;
            cart.attackSpeedModifier *= speedImprovementMultiplier;
        }
    }
}
