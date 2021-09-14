using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossibleTarget : MonoBehaviour {
    
    public enum Type {
        enemy, player
    };

    public Type myType;

    public Transform targetTransform;
    private void OnEnable() {
        if (targetTransform == null) {
            targetTransform = transform;
        }
        LevelReferences.allTargets.Add(this);
    }

    private void OnDisable() {
	    LevelReferences.allTargets.Remove(this);
    }
}
