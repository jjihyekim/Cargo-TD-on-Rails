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
    
    public float GetShopCostMultiplier(ResourceTypes type) {
        switch (type) {
            case ResourceTypes.ammo:
                return 0;
            case ResourceTypes.scraps:
                //return myTweakables.scrapShopCostMultiplier;
            case ResourceTypes.fuel:
                //return myTweakables.fuelShopCostMultiplier;
            default:
                return 1;
        }
    }
}


[System.Serializable]
public class Tweakables {

    public float ammoEnemyRewardMultiplier = 1f;
    public float scrapEnemyRewardMultiplier = 1f;
    public float fuelEnemyRewardMultiplier = 1f;

    //public float hpRepairCostMultiplier = 0.25f;
    public float hpRepairScrapCount = 1;
    public float hpRepairAmount = 50;
    
    public float enemyDamageMutliplier = 2.2f;
    public float playerDamageMultiplier = 2f;

    public float enemyFirerateBoost = 0.5f;
    public float playerFirerateBoost = 0.5f;

    public float cartDamageReductionMultiplier = 1f;
    public float magazineSizeMultiplier = 1f;
    public float gunSteamUseMultiplier = 1f;
    
    public float engineOverloadDamageMultiplier = 1f;

    public float ammoStorageAmmoGenDelay = 40f;
    public float tripleStorageAmmoGenDelay = 80f;

    public Tweakables Copy() {
        var serialized = SerializationUtility.SerializeValue(this, DataFormat.Binary);
        return SerializationUtility.DeserializeValue<Tweakables>(serialized, DataFormat.Binary);
    }
}
