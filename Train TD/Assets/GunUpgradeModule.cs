using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunUpgradeModule : MonoBehaviour {


    public float attackSpeedBoost = 1f;
    
    private void OnEnable() {
        GetComponentInParent<Cart>().attackSpeedModifier += attackSpeedBoost;
    }

    private void OnDisable() {
        GetComponentInParent<Cart>().attackSpeedModifier -= attackSpeedBoost;
    }
}
