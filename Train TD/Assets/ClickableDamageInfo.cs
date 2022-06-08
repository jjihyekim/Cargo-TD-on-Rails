using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableDamageInfo : ClickableEntityInfo {
    [Header("Info Text will be auto populated")]
    private GunModule _gunModule;
    
    private bool isPlayer = false;
    private void Start() {
        _gunModule = GetComponent<GunModule>();

        if (GetComponent<ModuleHealth>()) {
            isPlayer = true;
        }
    }

    // Update is called once per frame
    void Update() {
        var armorPenet = "";
        if (isPlayer && SceneLoader.s.currentLevel.HasArmoredEnemy()) {
            armorPenet = _gunModule.canPenetrateArmor ? "\narmor penetrating" : "\nweak against armor";
        }

        info = $"{_gunModule.projectileDamage} dmg{armorPenet}";
    }
}
