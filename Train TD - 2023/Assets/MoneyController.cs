using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class MoneyController : MonoBehaviour {
    public static MoneyController s;

    private void Awake() {
        s = this;
    }

    public float scraps { get; private set; }
    

    public void OnCharacterSelected() {
        if (DataSaver.s.GetCurrentSave().isInARun) {
            var resources = DataSaver.s.GetCurrentSave().currentRun.myResources;

            scraps = resources.scraps;
        }
    }
    

    void ModifyScraps(float amount) {
        if (PlayStateMaster.s.isCombatInProgress()) {
            scraps += amount;
            
        } else {
            var currentRunMyResources = DataSaver.s.GetCurrentSave().currentRun.myResources;
            currentRunMyResources.scraps += (int)amount;
            scraps = currentRunMyResources.scraps;
            
            DataSaver.s.SaveActiveGame();
        }
        
        /*if (scraps <= 0) {
            SoundscapeController.s.PlayNoMoreResource(ResourceTypes.scraps);
        }*/
    }

    public bool HasResource(ResourceTypes type, float amount) {
        switch (type) {
            case ResourceTypes.scraps:
                return scraps >= amount;
            default:
                return false;
        }
    }
    
    public float GetAmountPossibleToPay(ResourceTypes type, float amount) {
        switch (type) {
            case ResourceTypes.scraps:
                return Mathf.Min(amount,scraps);
            default:
                return 0;
        }
    }

    public void ModifyResource(ResourceTypes type, float amount, Transform source = null) {
        if (amount != 0) {
            switch (type) {
                case ResourceTypes.scraps:
                    ModifyScraps(amount);
                    break;
            }

            if (amount < 0 && source != null) {
                Instantiate(LevelReferences.s.GetResourceParticle(type), ResourceParticleSpawnLocation.GetSpawnLocation(type))
                    .GetComponent<ResourceParticleScript>().SetUp(source);
            }

            if (!PlayStateMaster.s.isCombatInProgress())
                DataSaver.s.SaveActiveGame();
        }
    }

    public float GetResource(ResourceTypes type) {
        switch (type) {
            case ResourceTypes.scraps:
                return scraps;
        }

        return 0;
    }


    [Button]
    void DebugAddResource(ResourceTypes type, int amount) {
        ModifyResource(type, amount);
    }
}
