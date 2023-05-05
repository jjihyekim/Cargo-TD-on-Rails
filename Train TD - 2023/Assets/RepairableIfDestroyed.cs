using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairableIfDestroyed : MonoBehaviour, IClickableInfo {
    private Cart myBuilding;
    private Tooltip myTooltip;
    void Start() {
        myBuilding = GetComponent<Cart>();
        myTooltip = new Tooltip() { text = "Repair this to at least half health to restore functionality." };
    }

    public bool ShowInfo() {
        return myBuilding.isDestroyed;
    }

    public string GetInfo() {
        return "Destroyed";
    }

    public Tooltip GetTooltip() {
        return myTooltip;
    }
}
