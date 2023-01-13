using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableHealthInfo : MonoBehaviour, IClickableInfo {
    private IHealth myHealth;

    private void Start() {
        myHealth = GetComponent<IHealth>();
    }

    public string GetInfo() {
        var isArmor = myHealth.HasArmor() ? "\narmored" : "";
        var info = myHealth.GetHealthRatioString() + "hp" + isArmor;
        return info;
    }

    public Tooltip GetTooltip() {
        return null;
    }

}
