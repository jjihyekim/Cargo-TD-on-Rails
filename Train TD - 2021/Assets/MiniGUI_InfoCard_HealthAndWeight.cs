using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class MiniGUI_InfoCard_HealthAndWeight : MonoBehaviour, IBuildingInfoCard {

    public TMP_Text health;
    public TMP_Text weight;

    [ReadOnly] public ModuleHealth healthModule;
    public void SetUp(TrainBuilding building) {
	    healthModule = building.GetComponent<ModuleHealth>();
        
        if (healthModule == null) {
            gameObject.SetActive(false);
            return;
        }else{
            gameObject.SetActive(true);
        }
        
        Update();

        weight.text = $"Weight: {building.weight}";
    }
    
    private void Update() {
        health.text = $"Health: {healthModule.currentHealth}/{healthModule.maxHealth}";
    }
}
