using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrainBuilding : MonoBehaviour {
    [HideInInspector]
    public bool canSelect = true;

    public string displayName = "Unnamed But Nice in game name";
    public string uniqueName = "unnamed";

    public Sprite Icon;

    public AudioClip[] moduleBuiltSound;

    [ReadOnly] 
    public Slot mySlot;

    public int mySlotIndex {
        get {
            return rotationToIndex[myRotation];
        }
    }

        [HideIf("occupiesEntireSlot")]
    public bool canPointSide;
    [HideIf("occupiesEntireSlot")]
    public bool canPointUp;
    [HideIf("occupiesEntireSlot")]
    public bool canBeRotatedSides = true;
    [HideIf("occupiesEntireSlot")]
    public bool canBeRotatedUp = true;

    public int rotationCount {
        get {
            int count = 0;
            count += canBeRotatedSides ? 2 : 0;
            count += canPointUp ? 1 : 0;
            count += canBeRotatedUp ? 1 : 0;
            return count;
        }
    }

    //public MonoBehaviour[] disabledWhenBuilding;

    public int localShopCost = 100;
    public int cost = 50;
    public int weight = 50;
    
    public enum Rots {
        left, right, forward, backwards
    }

    
    [HideIf("occupiesEntireSlot")]
    public Rots myRotation;

    public bool canBeTargetingAreaRotatable = false;

    public GenericCallback rotationChangedEvent;

    public bool occupiesEntireSlot = false;
    
    [HideIf("occupiesEntireSlot")]
    public GameObject sideGfx;
    [HideIf("occupiesEntireSlot")]
    public GameObject topGfx;
    
    [ShowIf("occupiesEntireSlot")]
    public GameObject frontGfx;
    [ShowIf("occupiesEntireSlot")]
    public GameObject backGfx;
    

    private Dictionary<Rots, Rots> fourWayRotation = new Dictionary<Rots, Rots>() {
        {Rots.left, Rots.forward},
        {Rots.forward, Rots.backwards},
        {Rots.backwards, Rots.right},
        {Rots.right, Rots.left},
    };
    
    private Dictionary<Rots, Rots> sideRotation = new Dictionary<Rots, Rots>() {
        {Rots.left, Rots.right},
        {Rots.right, Rots.left},
    };
    
    private Dictionary<Rots, Rots> topRotation = new Dictionary<Rots, Rots>() {
        {Rots.forward, Rots.backwards},
        {Rots.backwards, Rots.forward},
    };
    
    private Dictionary<Rots, int> rotationToIndex = new Dictionary<Rots, int>() {
        {Rots.forward, 0},
        {Rots.backwards, 1},
        {Rots.left, 2},
        {Rots.right, 3},
    };
    
    private Dictionary<int, Rots> indexToRotation = new Dictionary<int, Rots>() {
        { 0, Rots.forward},
        { 1, Rots.backwards},
        { 2, Rots.left},
        { 3, Rots.right},
    };

    public bool IsPointingSide() {
        return myRotation == Rots.left || myRotation == Rots.right;
    }

    public bool isBuilt = false;
    public bool autoSetUp = false;
    private void Start() {
        if (autoSetUp) {
            isBuilt = true;
            mySlot = GetComponentInParent<Slot>();
            if (mySlot) {
                mySlot.AddBuilding(this, mySlotIndex);

                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }
        SetGfxBasedOnRotation();
        SetBuildingMode(!isBuilt);
        SetUpOutlines();
    }

    public Transform GetUITargetTransform(bool isBottom) {
        if (!occupiesEntireSlot)
            return mySlot.GetUITargetTransform(mySlotIndex, isBottom);
        else
            return mySlot.GetUITargetTransform(4, isBottom);
    }

    public Transform GetShootingTargetTransform() {
        return transform;
    }


    public void SetBuildingMode(bool isBuildingHologram, bool isBuildable = true) {
        var duringCombat = GetComponents<IActiveDuringCombat>();
        var duringShopping = GetComponents<IActiveDuringShopping>();
        
        for (int i = 0; i < duringCombat.Length; i++) { duringCombat[i].Disable(); }
        for (int i = 0; i < duringShopping.Length; i++) { duringShopping[i].Disable(); }

        if (!isBuildingHologram) {
            if (SceneLoader.s.isLevelStarted()) {
                for (int i = 0; i < duringCombat.Length; i++) {
                    duringCombat[i].ActivateForCombat();
                }
            } else if (SceneLoader.s.isStarterMenu()) {
                for (int i = 0; i < duringShopping.Length; i++) {
                    duringShopping[i].ActivateForShopping();
                }
            }
        }


        var moduleGfxs = GetComponentsInChildren<ModuleGraphics>(true);

        foreach (var moduleGfx in moduleGfxs) {
            moduleGfx.SetBuildingMode(isBuildingHologram, isBuildable);
        }
    }

    void OnLevelStateChanged() {
        SetBuildingMode(!isBuilt);
    }

    private void OnEnable() {
        if (Train.s != null) {
            Train.s.onLevelStateChanged.AddListener(OnLevelStateChanged);
        } else {
            Invoke(nameof(OnEnable), 0.05f);
        }
    }

    private void OnDisable() {
        Train.s.onLevelStateChanged.RemoveListener(OnLevelStateChanged);
    }

    private void OnDestroy() {
        if(mySlot != null)
            mySlot.RemoveBuilding(this);
    }

    public void CompleteBuilding(bool playSound = true, bool playVoiceline = true) {
        isBuilt = true;
        SetGfxBasedOnRotation();
        SetBuildingMode(!isBuilt);
        if (playSound) {
            GetComponentInChildren<ConstructionSounds>().PlayConstructionSound();
            
            if (playVoiceline) {
                if (moduleBuiltSound.Length > 0)
                    SoundscapeController.s.PlayModuleBuilt(moduleBuiltSound[Random.Range(0, moduleBuiltSound.Length)]);
            }
        }
    }

    public int SetRotationBasedOnIndex(int index, bool isFrontSlot) {
        var rotTarget = indexToRotation[index];
        switch (rotTarget) {
            case Rots.left:
            case Rots.right:
                if (canPointSide) {
                    myRotation = rotTarget;
                }
                break;
            case Rots.forward:
            case Rots.backwards:
                if (canPointUp) {
                    myRotation = rotTarget;
                }
                break;
        }
        
        SetUpBasedOnRotationWithoutSlot(isFrontSlot);

        return rotationToIndex[myRotation];
    }

    public int CycleRotation(bool isTotalRotation = false) {
        if (!occupiesEntireSlot) {
            if (isTotalRotation) {
                if (canPointSide && canPointUp) {
                    myRotation = fourWayRotation[myRotation];
                } else if (canPointSide) {
                    myRotation = sideRotation[myRotation];
                } else if (canPointUp) {
                    myRotation = topRotation[myRotation];
                }
            } else {

                if ((myRotation == Rots.left || myRotation == Rots.right) && canBeRotatedSides) {
                    myRotation = sideRotation[myRotation];
                }

                if ((myRotation == Rots.forward || myRotation == Rots.backwards) && canBeRotatedUp) {
                    myRotation = topRotation[myRotation];
                }
            }
        }

        SetUpBasedOnRotationWithoutSlot();


        if (canBeTargetingAreaRotatable) {
            TargetingAreaRotate(true);
        }
        

        return rotationToIndex[myRotation];
    }

    public void TargetingAreaRotate(bool isClockwise) {
        if (isClockwise) {
            GetComponent<IComponentWithTarget>().GetRangeOrigin().Rotate(0,90,0);
        } else {
            GetComponent<IComponentWithTarget>().GetRangeOrigin().Rotate(0,-90,0);
        }
        rotationChangedEvent?.Invoke();
    }


    public void SetUpBasedOnRotation() {
        SetGfxBasedOnRotation();
        rotationChangedEvent?.Invoke();
    }
    public void SetUpBasedOnRotationWithoutSlot() {
        SetGfxBasedOnRotation(true, true);
        rotationChangedEvent?.Invoke();
    }
    
    public void SetUpBasedOnRotationWithoutSlot(bool slotTypeOverrideIsFront) {
        SetGfxBasedOnRotation(slotTypeOverrideIsFront, true);
        rotationChangedEvent?.Invoke();
    }
    
    public void SetGfxBasedOnRotation(bool slotTypeOverrideIsFront = false, bool isSlotTypeOverride = false) {
        if (!occupiesEntireSlot) {
            if (canBeRotatedSides || canBeRotatedUp) {
                switch (myRotation) {
                    case Rots.left:
                        transform.localRotation = Quaternion.Euler(0, 90, 0);
                        break;
                    case Rots.right:
                        transform.localRotation = Quaternion.Euler(0, -90, 0);
                        break;
                    case Rots.forward:
                        transform.localRotation = Quaternion.Euler(0, 0, 0);
                        break;
                    case Rots.backwards:
                        transform.localRotation = Quaternion.Euler(0, 180, 0);
                        break;
                }

                switch (myRotation) {
                    case Rots.left:
                    case Rots.right:
                        if (sideGfx != null)
                            sideGfx.SetActive(true);
                        if (topGfx != null)
                            topGfx.SetActive(false);
                        break;
                    case Rots.forward:
                    case Rots.backwards:
                        if (sideGfx != null)
                            sideGfx.SetActive(false);
                        if (topGfx != null)
                            topGfx.SetActive(true);
                        break;
                }
            }
        } else {
            if (frontGfx != null && backGfx != null) {
                if (isSlotTypeOverride) {
                    if (slotTypeOverrideIsFront) {
                        frontGfx.SetActive(true);
                        backGfx.SetActive(false);
                    } else {
                        frontGfx.SetActive(false);
                        backGfx.SetActive(true);
                    }
                } else {
                    if (mySlot != null) {
                        if (mySlot.isFrontSlot) {
                            frontGfx.SetActive(true);
                            backGfx.SetActive(false);
                        } else {
                            frontGfx.SetActive(false);
                            backGfx.SetActive(true);
                        }
                    } else {
                        frontGfx.SetActive(false);
                        backGfx.SetActive(true);
                    }
                }
            }
        }
    }


    [ReadOnly]
    public List<Outline> _outlines = new List<Outline>();

    void SetUpOutlines() {
        if (_outlines.Count == 0) {
            /*var renderers = GetComponentsInChildren<MeshRenderer>(true);

            foreach (var rend in renderers) {
                if (rend.GetComponent<Outline>() == null && !rend.gameObject.CompareTag("NoOutline")) {
                    var outline = rend.gameObject.AddComponent<Outline>();
                    outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
                    outline.OutlineWidth = 5;
                    outline.OutlineColor = new Color(0.0f, 0.833f, 1.0f, 1.0f);
                    outline.enabled = false;
                    _outlines.Add(outline);
                }
            }*/

            var moduleGraphics = GetComponentsInChildren<ModuleGraphics>(true);
            for (int i = 0; i < moduleGraphics.Length; i++) {
                var outline = moduleGraphics[i].GetComponent<Outline>();
                if(outline != null)
                    _outlines.Add(outline);
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

    public int GetCurrentHealth() {
        return (int)GetComponent<ModuleHealth>().currentHealth;
    }

    public void SetCurrentHealth(float health) {
        GetComponent<ModuleHealth>().currentHealth = health;
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