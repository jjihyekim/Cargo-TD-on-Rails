using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cart : MonoBehaviour {

    public UpgradesController.CartRarity myRarity;
    
    public bool isMainEngine = false;
    public bool isCriticalComponent = false;
    public bool isCargo = false;

    public int trainIndex;

    public bool isRepairable => !isMainEngine && !isCargo && !isCriticalComponent;
    //public bool isRepairable => !isCargo;

    public UpgradesController.CartLocation myLocation = UpgradesController.CartLocation.train;

    public float length = 1.4f;
    [HideInInspector]
    public bool canPlayerDrag = true;

    public string displayName = "Unnamed But Nice in game name";
    public string uniqueName = "unnamed";

    public Sprite Icon;

    public AudioClip[] moduleBuiltSound;

    public int weight = 50;

    [Space] 
    public bool isDestroyed = false;

    public Transform uiTargetTransform;
    public Transform shootingTargetTransform;
    public Transform modulesParent;

    [NonSerialized] public GameObject currentlyRepairingUIThing;
    private void Start() {
        SetComponentCombatShopMode();
        PlayStateMaster.s.OnCombatEntered.AddListener(SetComponentCombatShopMode);
        PlayStateMaster.s.OnShopEntered.AddListener(SetComponentCombatShopMode);
        SetUpOutlines();
    }

    private void Update() {
        var pos = transform.position;
        if (pos.y < -1) {
            pos.y = 1;
            transform.position = pos;
        }
    }

    public Transform GetShootingTargetTransform() {
        return shootingTargetTransform;
    }

    public Transform GetUITargetTransform() {
        return uiTargetTransform;
    }

    public void SetComponentCombatShopMode() {
        var duringCombat = GetComponentsInChildren<IActiveDuringCombat>();
        var duringShopping = GetComponentsInChildren<IActiveDuringShopping>();
        
        for (int i = 0; i < duringCombat.Length; i++) { duringCombat[i].Disable(); }
        for (int i = 0; i < duringShopping.Length; i++) { duringShopping[i].Disable(); }

        if (PlayStateMaster.s.isCombatStarted()) {
            for (int i = 0; i < duringCombat.Length; i++) {
                duringCombat[i].ActivateForCombat();
            }
        } else if (PlayStateMaster.s.isShop()) {
            for (int i = 0; i < duringShopping.Length; i++) {
                duringShopping[i].ActivateForShopping();
            }
        }
    }

    
    private void OnDestroy() {
        if(GetComponentInParent<Train>() != null)
            Train.s.CartDestroyed(this);
        
        if(currentlyRepairingUIThing != null)
            Destroy(currentlyRepairingUIThing);
        
        PlayStateMaster.s.OnCombatEntered.RemoveListener(SetComponentCombatShopMode);
        PlayStateMaster.s.OnShopEntered.RemoveListener(SetComponentCombatShopMode);
    }


    [ReadOnly]
    public List<Outline> _outlines = new List<Outline>();

    void SetUpOutlines() {
        if (_outlines.Count == 0) {

            var outlines = GetComponentsInChildren<Outline>(true);
            for (int i = 0; i < outlines.Length; i++) {
                if(outlines[i] != null)
                    _outlines.Add(outlines[i]);
            }
        }
    }

    public void SetHighlightState(bool isHighlighted) {
        if (_outlines.Count == 0) {
            SetUpOutlines();
        }
        
        foreach (var outline in _outlines) {
            if (outline != null) {
                outline.enabled = isHighlighted;
            }
        }
    }

    public ModuleHealth GetHealthModule() {
        return GetComponent<ModuleHealth>();
    }

    public int GetCurrentHealth() {
        return (int)GetComponent<ModuleHealth>().currentHealth;
    }

    public void SetCurrentHealth(float health) {
        GetComponent<ModuleHealth>().SetHealth(health);
    }

    public void SetAttachedToTrainModulesMode(bool isAttached) {
        var attachedToTrain = GetComponentsInChildren<IActivateWhenAttachedToTrain>();
        for (int i = 0; i < attachedToTrain.Length; i++) {
            if (isAttached) {
                attachedToTrain[i].AttachedToTrain();
            } else {
                attachedToTrain[i].DetachedFromTrain();
            }
        }
    }
}


public interface IActiveDuringCombat {
    public void ActivateForCombat();
    public void Disable();
}
public interface IActiveDuringShopping {
    public void ActivateForShopping();
    public void Disable();
    
}