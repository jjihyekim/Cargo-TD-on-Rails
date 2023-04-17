using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossibleTarget : MonoBehaviour, IActiveDuringCombat {
    
    public enum Type {
        enemy, player
    };

    public Type myType;

    public Transform targetTransform;
    private void OnEnable() {
        if (targetTransform == null) {
            var building = GetComponent<TrainBuilding>();

            if (building != null) {
                targetTransform = building.GetShootingTargetTransform();
            } else {
                targetTransform = transform;
            }
        }
        LevelReferences.allTargets.Add(this);
    }

    private void OnDisable() {
	    LevelReferences.allTargets.Remove(this);
    }

    public float GetHealth() {
        if (myType == Type.player) {
            return GetComponent<ModuleHealth>().currentHealth;
        } else {
            return GetComponent<EnemyHealth>().currentHealth;
        }
    }
    
    public void ActivateForCombat() {
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }
}
