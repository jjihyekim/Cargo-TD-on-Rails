using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_DirectControlBoosts : MonoBehaviour {
    private TMP_Text myText;
    private void Start() {
        myText = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update() {
        myText.text = $"Direct Control Boosts:\n" +
                      $"Damage: {DirectControlMaster.s.directControlDamageBoost:F2}x\n" +
                      $"Fire Rate: {DirectControlMaster.s.directControlFireRateBoost:F2}x\n" +
                      $"Ammo Conservation: {DirectControlMaster.s.directControlAmmoConservationBoost:F2}x";
    }
}
