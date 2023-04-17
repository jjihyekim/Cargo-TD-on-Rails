using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairableIfDestroyed : MonoBehaviour, IClickableInfo {
    private TrainBuilding myBuilding;
    private Tooltip myTooltip;
    void Start() {
        myBuilding = GetComponent<TrainBuilding>();
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
