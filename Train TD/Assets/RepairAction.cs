using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairAction : ModuleAction, IActiveDuringShopping {

    private TrainBuilding _building;
    private ModuleHealth _health;
    
    protected override void _Start() {
        _building = GetComponent<TrainBuilding>();
        _health = GetComponent<ModuleHealth>();

        if (!_building || !_health) {
            this.enabled = false;
        }
    }

    protected override void _EngageAction() {
        GetComponentInParent<Slot>().RemoveBuilding(GetComponent<TrainBuilding>());
        Instantiate(DataHolder.s.repairPrefab, transform.position, transform.rotation);
        _health.DealDamage(-(_health.maxHealth + 10));
    }

    protected override void _Update() {
        var hpPercent = _health.currentHealth / _health.maxHealth;
        cost = Mathf.CeilToInt(_building.cost * (1-hpPercent));
        
        if (Mathf.Approximately(_health.currentHealth,_health.maxHealth)) {
            this.enabled = false;
        }
    }

    public void ActivateForShopping() {
        this.enabled = true;
        
    }

    public void Disable() {
        this.enabled = false;
    }
}
