using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossibleTarget : MonoBehaviour, IActiveDuringCombat {
    [HideInInspector]
    public int myId;
    
    public enum Type {
        enemy, player
    };

    public Type myType;

    public Transform targetTransform;

    public bool avoid = false;
    public bool flying = false;
    private void OnEnable() {
        if (targetTransform == null) {
            var building = GetComponent<Cart>();

            if (building != null) {
                targetTransform = building.GetShootingTargetTransform();
            } else {
                targetTransform = transform;
            }
        }
        LevelReferences.allTargets.Add(this);
        LevelReferences.targetsDirty = true;
    }

    private void OnDisable() {
	    LevelReferences.allTargets.Remove(this);
        LevelReferences.targetsDirty = true;
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
    
    Vector3 previous;
    public Vector3 velocity = Vector3.zero;

    void Update() {
        if (Time.deltaTime > 0) {
            var newVelocity = ((transform.position - previous)) / Time.deltaTime;
            velocity = Vector3.Lerp(velocity, newVelocity, 1 * Time.deltaTime);
            previous = transform.position;
        }
    }
}
