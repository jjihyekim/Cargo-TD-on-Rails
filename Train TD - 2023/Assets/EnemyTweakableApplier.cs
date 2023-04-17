using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyTweakableApplier : MonoBehaviour {
    
    private void Start() {
        //ApplyStats(DataHolder.s.GetTweaks());
        TweakablesMaster.s.tweakableChanged.AddListener(ApplyStats);
        ApplyStats();
    }

	
    private void ApplyStats() {
		
    }
}
