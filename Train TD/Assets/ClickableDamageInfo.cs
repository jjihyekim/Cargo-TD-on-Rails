using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableDamageInfo : MonoBehaviour, IClickableInfo {
    [Header("Info Text will be auto populated")]
    private GunModule _gunModule;
    
    private bool isPlayer = false;
    private void Start() {
        _gunModule = GetComponent<GunModule>();

        if (GetComponent<ModuleHealth>()) {
            isPlayer = true;
        }
    }

    public string GetInfo() {
        return $"{_gunModule.GetDamage()}dmg/{_gunModule.fireDelay} secs";
    }

    public Tooltip GetTooltip() {
        return null;
    }
}
