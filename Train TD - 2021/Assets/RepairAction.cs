using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairAction : ModuleAction, IActiveDuringShopping, IActiveDuringCombat {

    private TrainBuilding _building;
    private ModuleHealth _health;

    float percentPerRepairDuringCombat = 0.5f;
    
    protected override void _Start() {
        _building = GetComponent<TrainBuilding>();
        _health = GetComponent<ModuleHealth>();

        if (!_building || !_health) {
            this.enabled = false;
        }
    }

    protected override void _EngageAction() {
        /*if (SceneLoader.s.isLevelInProgress) {
            StartCoroutine(Repair());
        } else {*/
            Instantiate(DataHolder.s.repairPrefab, transform.position, transform.rotation);
            _health.Heal((_health.maxHealth + 10));
        //}
    }

    /*IEnumerator Repair() {
        var totalRepairTime = 10f;
        var totalRepairCount = 5;
        var repairAmount = _health.maxHealth * percentPerRepairDuringCombat * (1f/totalRepairCount);
        var count = 0;
        
        while (_health.currentHealth < _health.maxHealth && count < totalRepairCount) {
            Repair(repairAmount);
            count += 1;
            yield return new WaitForSeconds(totalRepairTime/totalRepairCount);
        }
    }*/

    public void Repair(float amount) {
        Instantiate(DataHolder.s.repairPrefab, transform.position, transform.rotation);
        _health.Heal((amount));
    }

    protected override void _Update() {
        /*if (!SceneLoader.s.isLevelInProgress) {
            multiplier = 0.25f; // it is cheaper to repair if you are not in combat
        }*/

        var hpMissingPercent = 1-(_health.currentHealth / _health.maxHealth);

        if (SceneLoader.s.isLevelInProgress) {
            if (hpMissingPercent > percentPerRepairDuringCombat) {
                hpMissingPercent = percentPerRepairDuringCombat;
                actionName = $"Repair 50% health";
            } else {
                actionName = $"Repair to full health";
            }
        } else {
            actionName = $"Repair to full health";
        }

        cost = Mathf.CeilToInt(_building.cost * hpMissingPercent * LevelReferences.s.hpRepairCostMultiplier);
        
        if (Mathf.Approximately(_health.currentHealth,_health.maxHealth)) {
            canEngage = false;
        } else {
            canEngage = true;
        }
    }

    public float GetCostPerHealth(float health) {
        var hpMissingPercent = (health/ _health.maxHealth);

        var _cost = _building.cost * hpMissingPercent * LevelReferences.s.hpRepairCostMultiplier;

        return _cost;
    }

    public void ActivateForShopping() {
        this.enabled = true;
        cooldown = -1;
    }

    public void ActivateForCombat() {
        this.enabled = true;
        cooldown = 10f;
    }

    public void Disable() {
        this.enabled = false;
    }
}
