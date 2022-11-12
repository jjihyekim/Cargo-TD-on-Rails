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

    BuildingDoneCallback finishBuildingCallback;
    GetTheFinishedBuilding returnFinishedBuilding;
    public bool nextBuildIsFree = false;
    public bool nextBuildSayVoiceline = true;

    private void Start() {
        StopBuilding();
    }
    
    protected void OnEnable()
    {
        build.action.Enable();
        scroll.action.Enable();
        cancelBuild.action.Enable();
        multiBuild.action.Enable();
        build.action.performed += TryToPutDownBuilding;
        cancelBuild.action.performed += CancelBuilding;
    }

    

    protected void OnDisable()
    {
        build.action.Disable();
        scroll.action.Disable();
        cancelBuild.action.Disable();
        multiBuild.action.Disable();
        build.action.performed -= TryToPutDownBuilding;
        cancelBuild.action.performed -= CancelBuilding;
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
    

    private int lastRaycastIndex = 0;
    void CastRayToUpdateBuildingSlot() {
        RaycastHit hit;
        Ray ray = LevelReferences.s.mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hit, 100f, LevelReferences.s.buildingLayer)) {
            var slot = hit.collider.gameObject.GetComponentInParent<Slot>();

            if (slot != activeSlot) {
                var index = NormalToIndex(hit.normal);
                
                activeSlot = slot;
                if (index != -1 && index != lastRaycastIndex) {
                    activeIndex = index;
                    lastRaycastIndex = index;
                }

                if (activeIndex < 0) {
                    activeIndex = lastRaycastIndex;
                }

                if (activeIndex < 0) {
                    activeIndex = 0;
                }

                activeIndex = tempBuilding.SetRotationBasedOnIndex(activeIndex, slot.isFrontSlot);
                
                PlayerModuleSelector.s.playerBuildingDisableOverride = false;
            } else { //if it is still the same slot but a different side, then rotate our building
                var index = NormalToIndex(hit.normal);

                if (index != -1 && index != lastRaycastIndex) {
                    if (index <= 1 && activeIndex <= 1) {
                        //do nothing
                    } else {
                        lastRaycastIndex = index;
                        activeIndex = index;
                        activeIndex = tempBuilding.SetRotationBasedOnIndex(index, true);
                    }
                    PlayerModuleSelector.s.playerBuildingDisableOverride = false;
                }
            }
            
            var ranges = tempBuilding.GetComponentsInChildren<RangeVisualizer>();
            for (int i = 0; i < ranges.Length; i++) {
                ranges[i].ChangeVisualizerEdgeShowState(true);
            }
            
        } else {
            activeSlot = null;
            activeIndex = -2;
            
            var ranges = tempBuilding.GetComponentsInChildren<RangeVisualizer>();
            for (int i = 0; i < ranges.Length; i++) {
                ranges[i].ChangeVisualizerEdgeShowState(false);
            }
        }

        UpdateCanBuildable();
    }

    public static int NormalToIndex(Vector3 normal) {
        var index = -1;
        if (Vector3.Angle(normal, Vector3.up) < 10) {
            index = 0;
        }
        
        if (Vector3.Angle(normal, Vector3.right) < 10) {
            index = 2;
        }
        
        if (Vector3.Angle(normal, Vector3.left) < 10) {
            index = 3;
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

            PlayerModuleSelector.s.playerBuildingDisableOverride = true;
        } else {
            curScrollTime -= Time.deltaTime;
        }
    }

    void UpdateCanBuildable() {
        if (tempBuilding) {
            if (activeSlot != null) {
                tempBuilding.SetBuildingMode(true, activeSlot.CanBuiltInSlot(tempBuilding, ref activeIndex));
            } else {
                tempBuilding.SetBuildingMode(true, false);
            }
        }
    }

    private void TryToPutDownBuilding(InputAction.CallbackContext context) {
        if (activeSlot != null) {
            if (activeSlot.CanBuiltInSlot(tempBuilding, ref activeIndex)) {
                var canBuildMore = true;
                if (finishBuildingCallback != null) {
                    // This means we are doing building in menu
                    canBuildMore = finishBuildingCallback.Invoke(true);
                } else {
                    canBuildMore = MoneyController.s.HasResource(ResourceTypes.scraps, tempBuilding.cost*2);
                }

                var newBuilding = tempBuilding;
                var currentlyMultiBuilding = false;
                
                if (canBuildMore && multiBuild.action.ReadValue<float>() > 0f) {
                    currentlyMultiBuilding = true;
                    newBuilding = Instantiate(tempBuilding.gameObject).GetComponent<TrainBuilding>();
                }
                
                activeSlot.AddBuilding(newBuilding, activeIndex);
                newBuilding.transform.position = activeSlot.transform.position;
                newBuilding.CompleteBuilding(true, nextBuildSayVoiceline);
                returnFinishedBuilding?.Invoke(newBuilding);
                
                if(!nextBuildIsFree)
                    MoneyController.s.ModifyResource(ResourceTypes.scraps, -newBuilding.cost);
                nextBuildIsFree = false;
                
                if (!SceneLoader.s.isLevelInProgress) {
                    DataSaver.s.GetCurrentSave().currentRun.myTrain = Train.s.GetTrainState();
                    DataSaver.s.SaveActiveGame();
                }

                LogData(currentlyMultiBuilding, newBuilding);

                PlayerModuleSelector.s.SelectBuilding(newBuilding, false);
                if (!currentlyMultiBuilding) {
                    tempBuilding = null;
                    activeSlot = null;
                    activeIndex = -2;
                    isBuilding = false;
                
                    StopBuilding();
                } else {
                    UpdateCanBuildable();
                }
                
                PlayerModuleSelector.s.playerBuildingDisableOverride = false;
                PlayerModuleSelector.s.playerBuildingSkipOneTimeOverride = true;
            }
        }
    }

    private void LogData(bool currentlyMultiBuilding, TrainBuilding newBuilding) {
        var buildingName = newBuilding.uniqueName;
        var cData = new ConstructionData() {
            buildTrainPercent = activeSlot.DistancePercent(),
            isMultiBuild = currentlyMultiBuilding,
            buildLevelDistance = SpeedController.s.currentDistance,
            buildRotation = newBuilding.myRotation,
        };

        if (currentLevelStats.TryGetValue(buildingName, out BuildingData data)) {
            data.constructionData.Add(cData);
        } else {
            var toAdd = new BuildingData();
            toAdd.uniqueName = buildingName;
            toAdd.constructionData.Add(cData);
            currentLevelStats.Add(buildingName, toAdd);
        }
    }

    public Transform statsParent;
    public GameObject statsPrefab;

    public void LogCurrentLevelBuilds(bool isWon) {
        foreach (var keyValPair in currentLevelStats) {
            var bName = keyValPair.Key;
            var bData = keyValPair.Value;
            var constStats = bData.constructionData;

            Dictionary<string, object> resultingDictionary = new Dictionary<string, object>();

            resultingDictionary["currentLevel"] = SceneLoader.s.currentLevel.levelName;
            resultingDictionary["isWon"] = isWon;

            resultingDictionary["buildCount"] = constStats.Count;

            if (constStats.Count > 0) {
                var averageTrainPosition = constStats.Average(x => x.buildTrainPercent);
                resultingDictionary["buildTrainPercent"] = RatioToStatsPercent(averageTrainPosition);

                var multiBuildRatio = (float)constStats.Count(x => x.isMultiBuild) / (float)constStats.Count;
                resultingDictionary["isMultiBuild"] = RatioToStatsPercent(multiBuildRatio);

                var averageBuildLevelDistance = constStats.Average(x => x.buildLevelDistance);
                resultingDictionary["buildMissionDistance"] = DistanceToStats(averageBuildLevelDistance);

                TrainBuilding.Rots maxRepeated = constStats.GroupBy(s => s.buildRotation)
                    .OrderByDescending(s => s.Count())
                    .First().Key;
                resultingDictionary["buildRotation"] = maxRepeated;
            } 
            

            resultingDictionary["buildDamage"] = (int)bData.damageData;

            //print(resultingDictionary);

            AnalyticsResult analyticsResult = Analytics.CustomEvent(
                bName,
                resultingDictionary
                );
            
            Debug.Log("Building Build Data Analytics " + analyticsResult);
            
            Instantiate(statsPrefab, statsParent).GetComponent<MiniGUI_StatDisplay>().SetUp(bName + " Build Count", (constStats.Count).ToString());
            Instantiate(statsPrefab, statsParent).GetComponent<MiniGUI_StatDisplay>().SetUp(bName + " Damage", ((int)bData.damageData).ToString());
        }
    }

    int DistanceToStats(float distance) {
        return Mathf.RoundToInt(distance / 10) * 10;
    }

    int RatioToStatsPercent(float ratio) {
        // only in sets of 5 > 0% 5% 10% etc.
        return Mathf.RoundToInt(ratio * 20) * 5;
    }

    public Dictionary<string, BuildingData> currentLevelStats = new Dictionary<string, BuildingData>();


    public class BuildingData {
        public string uniqueName;
        public float damageData = 0;
        public List<ConstructionData> constructionData = new List<ConstructionData>();
    }
    
    
    public struct ConstructionData {
        public float buildTrainPercent;
        public bool isMultiBuild;
        public float buildLevelDistance;
        public TrainBuilding.Rots buildRotation;
    }

    void CancelBuilding(InputAction.CallbackContext context) {
        if (isBuilding) {
            finishBuildingCallback?.Invoke(false);
            finishBuildingCallback = null;
            returnFinishedBuilding = null;
            StopBuilding();
            
            PlayerModuleSelector.s.playerBuildingDisableOverride = false;
            PlayerModuleSelector.s.playerBuildingSkipOneTimeOverride = false;
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


    public void StartBuilding(TrainBuilding building, 
        BuildingDoneCallback callback = null, GetTheFinishedBuilding buildingReturn = null, bool isFree = false, bool sayVoiceline = true
        ) {
        inputActionMap.FindActionMap(buildingActionMap).Enable();
        inputActionMap.FindActionMap(nonbuildingActionMap).Disable();
        
        if (tempBuilding != null) {
            Destroy(tempBuilding.gameObject);
        }
        tempBuilding = Instantiate(building.gameObject, Vector3.zero, Quaternion.identity).GetComponent<TrainBuilding>();
        tempBuilding.SetBuildingMode(true);
        tempBuilding.CycleRotation();
        tempBuilding.SetGfxBasedOnRotation();
        isBuilding = true;
        
        MenuToggle.HideAllToggleMenus();
        finishBuildingCallback = callback;
        returnFinishedBuilding = buildingReturn;
        nextBuildIsFree = isFree;
        nextBuildSayVoiceline = sayVoiceline;

        Debug.Log($"Starting building: {building.displayName}");
    }


    public void StopBuilding() {
        inputActionMap.FindActionMap(buildingActionMap).Disable();
        inputActionMap.FindActionMap(nonbuildingActionMap).Enable();

        if (tempBuilding) {
            Destroy(tempBuilding.gameObject);
            activeSlot = null;
            activeIndex = -2;
        }
        isBuilding = false;
    }
    public delegate bool BuildingDoneCallback(bool isSuccess);

    public delegate void GetTheFinishedBuilding(TrainBuilding building);
}


