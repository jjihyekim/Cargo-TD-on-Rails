using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainBuilding : MonoBehaviour {

    public string uniqueName = "unnamed";

    public Sprite Icon;

    public Slot mySlot;
    public int mySlotIndex = 0;

    public bool canPointSide;
    public bool canPointUp;
    public bool canBeRotatedSides = true;
    public bool canBeRotatedUp = true;

    public MonoBehaviour[] disabledWhenBuilding;

    public int cost = 50;
    
    public enum Rots {
        left, right, forward, backwards
    }

    public Rots myRotation;

    public GenericCallback rotationChangedEvent;

    public bool occupiesEntireSlot = false;


    public GameObject sideGfx;
    public GameObject topGfx;


    public List<TransformWithRotation> myUITargetTransforms = new List<TransformWithRotation>();

    [Serializable]
    public class TransformWithRotation {
        public List<Rots> possibleRotations = new List<Rots>() { Rots.left, Rots.right, Rots.forward, Rots.backwards };
        public Transform transform;
    }

    public Transform uiTargetTransform {
        get {
            for (int i = 0; i < myUITargetTransforms.Count; i++) {
                if (myUITargetTransforms[i].possibleRotations.Contains(myRotation)) {
                    return myUITargetTransforms[i].transform;
                }
            }

            return transform;
        }
    }

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
        {Rots.left, 0},
        {Rots.forward, 1},
        {Rots.backwards, 1},
        {Rots.right, 2},
    };
    
    private Dictionary<int, Rots> indexToRotation = new Dictionary<int, Rots>() {
        { 0, Rots.left},
        { 1, Rots.backwards},
        { 2, Rots.right},
    };

    public bool isBuilt = false;
    private void Start() {
        if (isBuilt) {
            mySlot = GetComponentInParent<Slot>();
            if(mySlot)
                mySlot.AddBuilding(this, mySlotIndex);
        }
        SetGfxBasedOnRotation();
        SetBuildingMode(!isBuilt);
        SetUpOutlines();
    }


    public void SetBuildingMode(bool isBuildingHologram, bool isBuildable = true) {
        for (int i = 0; i < disabledWhenBuilding.Length; i++) {
            disabledWhenBuilding[i].enabled = !isBuildingHologram;
        }

        var moduleGfxs = GetComponentsInChildren<ModuleGraphics>();

        foreach (var moduleGfx in moduleGfxs) {
            moduleGfx.SetBuildingMode(isBuildingHologram, isBuildable);
        }
    }

    public void CompleteBuilding() {
        isBuilt = true;
        SetGfxBasedOnRotation();
        SetBuildingMode(!isBuilt);
        GetComponentInChildren<ConstructionSounds>().PlayConstructionSound();
    }
    
    public int SetRotationBasedOnIndex(int index) {
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
        SetUpBasedOnRotation();

        return rotationToIndex[myRotation];
    }

    public int CycleRotation() {
            /*if (canPointSide && canPointUp) {
                myRotation = fourWayRotation[myRotation];
            } else if (canPointSide) {
                myRotation = sideRotation[myRotation];
            } else if (canPointUp) {
                myRotation = topRotation[myRotation];
            }*/

        if ((myRotation == Rots.left || myRotation == Rots.right) && canBeRotatedSides) {
            myRotation = sideRotation[myRotation];
        }
        if ((myRotation == Rots.forward || myRotation == Rots.backwards) && canBeRotatedUp) {
            myRotation = topRotation[myRotation];
        }

        SetUpBasedOnRotation();
        

        return rotationToIndex[myRotation];
    }

    public void SetUpBasedOnRotation() {
        SetGfxBasedOnRotation();
        rotationChangedEvent?.Invoke();
    }
    
    public void SetGfxBasedOnRotation() {
        if (canBeRotatedSides || canBeRotatedUp) {
            switch (myRotation) {
                case Rots.left:
                    transform.rotation = Quaternion.Euler(0, 90, 0);
                    break;
                case Rots.right:
                    transform.rotation = Quaternion.Euler(0, -90, 0);
                    break;
                case Rots.forward:
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case Rots.backwards:
                    transform.rotation = Quaternion.Euler(0, 180, 0);
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
    }


    public List<Outline> _outlines = new List<Outline>();

    void SetUpOutlines() {
        if (_outlines.Count == 0) {
            var renderers = GetComponentsInChildren<MeshRenderer>(true);

            foreach (var rend in renderers) {
                if (rend.GetComponent<Outline>() == null) {
                    var outline = rend.gameObject.AddComponent<Outline>();
                    outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
                    outline.OutlineWidth = 5;
                    outline.OutlineColor = new Color(0.0f, 0.833f, 1.0f, 1.0f);
                    outline.enabled = false;
                    _outlines.Add(outline);
                }
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
    
}
