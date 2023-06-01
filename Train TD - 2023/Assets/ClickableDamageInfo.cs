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


    public bool ShowInfo() {
        return true;
    }

    public string GetInfo() {
        return $"{_gunModule.GetDamage():F1}dmg/{_gunModule.GetFireDelay()} secs";
    }

    public Tooltip GetTooltip() {
        return null;
    }
}
