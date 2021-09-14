using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.InputSystem;

public class PlayerBuildingController : MonoBehaviour { 
    public static PlayerBuildingController s;
    private void Awake() {
        s = this;
    }

    private void OnDestroy() {
        s = null;
    }

    public TrainBuilding tempBuilding;
    public Slot activeSlot;
    public int activeIndex;
    public bool isBuilding = true;

    public InputActionProperty build;
    public InputActionProperty cancelBuild; 
    public InputActionProperty scroll;
    public InputActionProperty multiBuild;

    public InputActionAsset inputActionMap;
    public string buildingActionMap;
    public string nonbuildingActionMap;

    public LayerMask buildingLayerMask;

    TrueFalseCallback finishBuildingCallback;

    private void Start() {
        StopBuilding();
    }
    
    protected void OnEnable()
    {
        build.action.Enable();
        scroll.action.Enable();
        build.action.performed += TryToPutDownBuilding;
        cancelBuild.action.Enable();
        cancelBuild.action.performed += CancelBuilding;
        multiBuild.action.Enable();
    }

    

    protected void OnDisable()
    {
        build.action.Disable();
        scroll.action.Disable();
        build.action.performed -= TryToPutDownBuilding;
        cancelBuild.action.Disable();
        cancelBuild.action.performed -= CancelBuilding;
        multiBuild.action.Disable();
    }

    private void Update() {
        if (isBuilding) {
            if (activeSlot == null) {
                tempBuilding.transform.position = GetMousePositionOnPlane();
            } else {
                tempBuilding.transform.position = activeSlot.transform.position;
            }

            CastRayToUpdateBuildingSlot();
            
            ProcessScroll(scroll.action.ReadValue<float>());
        } 
    }
    
/*void CastRayToActivateBuildingOptions() {
    RaycastHit hit;
    Ray ray = LevelReferences.s.mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

    if (Physics.Raycast(ray, out hit, 100f, buildingLayerMask)) {
        var slot = hit.collider.gameObject.GetComponentInParent<Slot>();

        if (slot != activeSlot) {
            var index = NormalToIndex(hit.normal);
            
            activeSlot = slot;
            if (index != -1 && index != lastRaycastIndex) {
                activeIndex = index;
                lastRaycastIndex = index;
            }

            activeIndex = tempBuilding.SetRotationBasedOnIndex(activeIndex);
            UpdateCanBuildable();
            
        } else { //if it is still the same slot but a different side, then rotate our building
            var index = NormalToIndex(hit.normal);

            if (index != -1 && index != lastRaycastIndex) {
                lastRaycastIndex = index;
                activeIndex = index;
                activeIndex = tempBuilding.SetRotationBasedOnIndex(index);
                UpdateCanBuildable();
            }
        }
    } else {
        activeSlot = null;
    }
}*/

    private int lastRaycastIndex = 0;
    void CastRayToUpdateBuildingSlot() {
        RaycastHit hit;
        Ray ray = LevelReferences.s.mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hit, 100f, buildingLayerMask)) {
            var slot = hit.collider.gameObject.GetComponentInParent<Slot>();

            if (slot != activeSlot) {
                var index = NormalToIndex(hit.normal);
                
                activeSlot = slot;
                if (index != -1 && index != lastRaycastIndex) {
                    activeIndex = index;
                    lastRaycastIndex = index;
                }

                activeIndex = tempBuilding.SetRotationBasedOnIndex(activeIndex);
                
            } else { //if it is still the same slot but a different side, then rotate our building
                var index = NormalToIndex(hit.normal);

                if (index != -1 && index != lastRaycastIndex) {
                    lastRaycastIndex = index;
                    activeIndex = index;
                    activeIndex = tempBuilding.SetRotationBasedOnIndex(index);
                }
            }
        } else {
            activeSlot = null;
        }

        UpdateCanBuildable();
    }

    int NormalToIndex(Vector3 normal) {
        var index = -1;
        if (Vector3.Angle(normal, Vector3.right) < 10) {
            index = 0;
        }
        if (Vector3.Angle(normal, Vector3.up) < 10) {
            index = 1;
        }
        if (Vector3.Angle(normal, Vector3.left) < 10) {
            index = 2;
        }

        return index;
    }

    public float scrollTime = 0.2f;
    private float curScrollTime = 0;
    void ProcessScroll(float value) {
        if (curScrollTime <= 0f) {
            if (value > 0) {
                activeIndex = tempBuilding.CycleRotation();
                UpdateCanBuildable();
                curScrollTime = scrollTime;
            }

            if (value < 0) {
                tempBuilding.CycleRotation();
                tempBuilding.CycleRotation();
                activeIndex = tempBuilding.CycleRotation();
                UpdateCanBuildable();
                curScrollTime = scrollTime;
            }
        } else {
            curScrollTime -= Time.deltaTime;
        }
    }

    void UpdateCanBuildable() {
        if (tempBuilding) {
            if (activeSlot != null) {
                tempBuilding.SetBuildingMode(true, activeSlot.CanBuiltInSlot(tempBuilding, activeIndex));
            } else {
                tempBuilding.SetBuildingMode(true, false);
            }
        }
    }

    private void TryToPutDownBuilding(InputAction.CallbackContext context) {
        if (activeSlot != null) {
            if (activeSlot.CanBuiltInSlot(tempBuilding, activeIndex)) {
                var canBuildMore = true;
                if (finishBuildingCallback != null) {
                    // This means we are doing building in menu
                    canBuildMore = finishBuildingCallback.Invoke(true);
                } else {
                    canBuildMore = MoneyController.s.money >= tempBuilding.cost;
                }

                var newBuilding = tempBuilding;
                var currentlyMultiBuilding = false;
                
                if (canBuildMore && multiBuild.action.ReadValue<float>() > 0f) {
                    currentlyMultiBuilding = true;
                    newBuilding = Instantiate(tempBuilding.gameObject).GetComponent<TrainBuilding>();
                }
                
                activeSlot.AddBuilding(newBuilding, activeIndex);
                newBuilding.transform.position = activeSlot.transform.position;
                newBuilding.CompleteBuilding();
                if (LevelLoader.s.isLevelStarted)
                    MoneyController.s.SubtractMoney(newBuilding.cost);

                LogData(currentlyMultiBuilding, newBuilding);

                if (!currentlyMultiBuilding) {
                    tempBuilding = null;
                    activeSlot = null;
                    isBuilding = false;
                
                    StopBuilding();
                } else {
                    UpdateCanBuildable();
                }
            }
        }
    }

    private void LogData(bool currentlyMultiBuilding, TrainBuilding newBuilding) {
        var buildingName = tempBuilding.gameObject.name;
        var bData = new BuildingBuildData() {
            buildTrainPercent = activeSlot.DistancePercent(),
            isMultiBuild = currentlyMultiBuilding,
            buildLevelDistance = SpeedController.s.currentDistance,
            buildRotation = newBuilding.myRotation,
        };

        if (currentLevelBuilds.TryGetValue(buildingName, out List<BuildingBuildData> data)) {
            data.Add(bData);
        } else {
            currentLevelBuilds.Add(buildingName, new List<BuildingBuildData>() { bData });
        }
    }

    public void LogCurrentLevelBuilds(bool isWon) {
        foreach (var keyValPair in currentLevelBuilds) {
            var bName = keyValPair.Key;
            var statsList = keyValPair.Value;

            Dictionary<string, object> resultingDictionary = new Dictionary<string, object>();

            resultingDictionary["currentLevel"] = LevelLoader.s.currentLevel.levelName;
            resultingDictionary["isWon"] = isWon;

            resultingDictionary["buildCount"] = statsList.Count;

            var averageTrainPosition = statsList.Average(x => x.buildTrainPercent);
            resultingDictionary["buildTrainPercent"] = RatioToStatsPercent(averageTrainPosition);

            var multiBuildRatio = (float)statsList.Count(x => x.isMultiBuild) / (float)statsList.Count;
            resultingDictionary["isMultiBuild"] = RatioToStatsPercent(multiBuildRatio);
            
            
            var averageBuildLevelDistance = statsList.Average(x => x.buildLevelDistance);
            resultingDictionary["buildMissionDistance"] = DistanceToStats(averageBuildLevelDistance);
            
            TrainBuilding.Rots maxRepeated = statsList.GroupBy(s => s.buildRotation)
                .OrderByDescending(s => s.Count())
                .First().Key;
            resultingDictionary["buildRotation"] = maxRepeated;

            //print(resultingDictionary);

            AnalyticsResult analyticsResult = Analytics.CustomEvent(
                bName,
                resultingDictionary
                );
            
            Debug.Log("Building Build Data Analytics " + analyticsResult);
        }
    }

    int DistanceToStats(float distance) {
        return Mathf.RoundToInt(distance / 10) * 10;
    }

    int RatioToStatsPercent(float ratio) {
        // only in sets of 5 > 0% 5% 10% etc.
        return Mathf.RoundToInt(ratio * 20) * 5;
    }

    public Dictionary<string, List<BuildingBuildData>> currentLevelBuilds = new Dictionary<string, List<BuildingBuildData>>();

    public struct BuildingBuildData {
        public float buildTrainPercent;
        public bool isMultiBuild;
        public float buildLevelDistance;
        public TrainBuilding.Rots buildRotation;
    }

    void CancelBuilding(InputAction.CallbackContext context) {
        if (isBuilding) {
            finishBuildingCallback?.Invoke(false);
            finishBuildingCallback = null;
            StopBuilding();
        }
    }



    Vector3 GetMousePositionOnPlane() {
        Plane plane = new Plane(Vector3.up, new Vector3(0,1,0));

        float distance;
        Ray ray = LevelReferences.s.mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        } else {
            return Vector3.zero;
        }
    }


    public void StartBuilding(TrainBuilding building, TrueFalseCallback callback = null) {
        inputActionMap.FindActionMap(buildingActionMap).Enable();
        inputActionMap.FindActionMap(nonbuildingActionMap).Disable();
        
        if (tempBuilding != null) {
            Destroy(tempBuilding.gameObject);
        }
        tempBuilding = Instantiate(building.gameObject, Vector3.zero, Quaternion.identity).GetComponent<TrainBuilding>();
        tempBuilding.SetBuildingMode(true);
        tempBuilding.SetGfxBasedOnRotation();
        isBuilding = true;
        
        MenuToggle.HideAllToggleMenus();
        finishBuildingCallback = callback;
    }

    public void StopBuilding() {
        inputActionMap.FindActionMap(buildingActionMap).Disable();
        inputActionMap.FindActionMap(nonbuildingActionMap).Enable();

        if (tempBuilding) {
            Destroy(tempBuilding.gameObject);
            activeSlot = null;
        }
        isBuilding = false;
    }
}


public delegate bool TrueFalseCallback(bool isSuccess);
