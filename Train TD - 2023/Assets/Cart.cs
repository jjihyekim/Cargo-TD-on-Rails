using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cart : MonoBehaviour {

    public int level;

    public UpgradesController.CartRarity myRarity;
    
    public bool isMainEngine = false;
    public bool isMysteriousCart = false;
    public bool isCargo = false;

    public int trainIndex;

    public bool isRepairable => !isMainEngine && !isCargo && !isMysteriousCart;
    public bool loseGameIfYouLoseThis => isMainEngine || isMysteriousCart;
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
        SetUpOverlays();
        SetUpOutlines();
    }


    public Material cartOverlayMaterial;

    public MeshRenderer cartMaterial;

    public Transform genericParticlesParent;

    public void ResetState() {
        genericParticlesParent.DeleteAllChildren();
        GetHealthModule().ResetState(level);

        cartMaterial.material = LevelReferences.s.cartLevelMats[level];

        var gunModules = GetComponentsInChildren<GunModule>();
        for (int i = 0; i < gunModules.Length; i++) {
            gunModules[i].ResetState(level);
        }

        var ammoModules = GetComponentsInChildren<ModuleAmmo>();
        for (int i = 0; i < ammoModules.Length; i++) {
            ammoModules[i].ResetState();
        }

        var boosterModules = GetComponentsInChildren<IBooster>();
        for (int i = 0; i < boosterModules.Length; i++) {
            boosterModules[i].ResetState(level);
        }
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
        // Destroy material instances
        Destroy(cartOverlayMaterial);
        
        if(GetComponentInParent<Train>() != null)
            Train.s.CartDestroyed(this);
        
        if(currentlyRepairingUIThing != null)
            Destroy(currentlyRepairingUIThing);
        
        PlayStateMaster.s.OnCombatEntered.RemoveListener(SetComponentCombatShopMode);
        PlayStateMaster.s.OnShopEntered.RemoveListener(SetComponentCombatShopMode);
    }


    [ReadOnly]
    public Outline[] _outlines;
    [ReadOnly]
    public MeshRenderer[] _meshes;

    void SetUpOutlines() {
        if (_outlines == null || _outlines.Length ==0) {
            _outlines = GetComponentsInChildren<Outline>(true);
        }
    }

    void SetUpOverlays() {
        if (_meshes == null || _meshes.Length == 0) {
            cartOverlayMaterial = Instantiate(cartOverlayMaterial);
            
            _meshes = GetComponentsInChildren<MeshRenderer>(true);

            for (int i = 0; i < _meshes.Length; i++) {
                var materials = _meshes[i].sharedMaterials.ToList();
                materials.Add(cartOverlayMaterial);
                _meshes[i].materials = materials.ToArray();
            }
        }
    }
    
    public void SetHighlightState(bool isHighlighted) {
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
        var attachedToTrain = GetComponentsInChildren<ActivateWhenAttachedToTrain>();
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