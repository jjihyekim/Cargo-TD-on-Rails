using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;

public class TweakablesMaster : MonoBehaviour {
    public static TweakablesMaster s;
    
    private Tweakables _myTweakables;
    public Tweakables myTweakables;

    private void Awake() {
        s = this;
        _myTweakables = myTweakables.Copy();
    }

    public void ResetTweakable() {
        myTweakables = _myTweakables.Copy();
    }


    [HideInInspector]
    public UnityEvent tweakableChanged = new UnityEvent();


    public void ApplyTweakableChange() {
        tweakableChanged?.Invoke();
        
        //myTweakables.GetType().GetField("yeet").SetValue();
    }
}


[System.Serializable]
public class Tweakables {
    public float scrapEnemyRewardMultiplier = 1f;

    
    public float enemyDamageMutliplier = 2.2f;
    public float playerDamageMultiplier = 2f;

    public float enemyFirerateBoost = 0.5f;
    public float playerFirerateBoost = 0.5f;

    public float cartDamageReductionMultiplier = 1f;
    public float magazineSizeMultiplier = 1f;
    public float gunSteamUseMultiplier = 1f;
    
    public float engineOverloadDamageMultiplier = 1f;

    public Tweakables Copy() {
        var serialized = SerializationUtility.SerializeValue(this, DataFormat.Binary);
        return SerializationUtility.DeserializeValue<Tweakables>(serialized, DataFormat.Binary);
    }
}
