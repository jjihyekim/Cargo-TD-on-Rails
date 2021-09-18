using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairModule : MonoBehaviour {
    public float repairPerSecond = 5;


    private Cart myCart;
    private void Start() {
        myCart = GetComponentInParent<Cart>();
    }

    void Update() {
        var healths = myCart.GetComponentsInChildren<ModuleHealth>();

        for (int i = 0; i < healths.Length; i++) {
            healths[i].DealDamage(-repairPerSecond * Time.deltaTime);
        }
    }
}
