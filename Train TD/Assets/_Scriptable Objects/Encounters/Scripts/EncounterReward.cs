using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EncounterReward : MonoBehaviour {
    public Sprite icon;

    public ResourceTypes myType;
    public int amount = 50;
    public bool randomizeAmount = true;
    readonly float randomPercent = 0.1f;

    public TrainBuilding building;


    public bool damageTrain = false;

    public void RandomizeReward() {
        if (randomizeAmount) {
            amount = (int)(amount * (1 + Random.Range(-randomPercent, randomPercent)));
        }
    }

    public void GainReward() {
        if (!damageTrain) {
            if (building != null) {
                UpgradesController.s.AddModulesToAvailableModules(building, amount);
            } else {
                MoneyController.s.ModifyResource(myType, amount);
            }
        } else {
            var healths = Train.s.GetComponentsInChildren<ModuleHealth>();

            for (int i = 0; i < healths.Length; i++) {
                if (Random.value > 0.5f) {
                    healths[i].DealDamage(healths[i].maxHealth/4);
                    Instantiate(LevelReferences.s.rocketExplosionEffectPrefab, healths[i].transform.position, Quaternion.identity);
                }
            }
            
            Train.s.SaveTrainState();
        }
    }
}
