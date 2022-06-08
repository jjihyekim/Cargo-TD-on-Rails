using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableHealthInfo : ClickableEntityInfo {
    [Header("Info Text will be auto populated")]
    private IHealth myHealth;

    private void Start() {
        myHealth = GetComponent<IHealth>();
    }
    // Update is called once per frame
    void Update() {
        var isArmor = myHealth.HasArmor() ? "\narmored" : "";
        info = myHealth.GetHealthRatioString() + "hp" + isArmor;
    }
}
