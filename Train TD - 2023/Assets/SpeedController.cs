using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SpeedController : MonoBehaviour, IShowOnDistanceRadar {
    public static SpeedController s;

    private void Awake() {
        s = this;
    }

    public float debugSpeedOverride = -1f;

    public float currentTime = 0;
    public float currentDistance = 0;

    public float missionDistance = 300; //100 engine power goes 1 distance per second

    public float currentEnginePower = 0;
    public float enginePower = 0;
    public float enginePowerBoost = 1f;
    public float targetSpeed;
    public float speedMultiplier = 1.5f;

    public TMP_Text timeText;
    public TMP_Text distanceText;

    public TrainStation endTrainStation;

    //public TMP_Text fuelText;
    //public TMP_Text fuelUseText;

    public List<EngineModule> engines = new List<EngineModule>();

    private void Start() {
        ResetDistance();
    }

    public void ResetDistance() {
        missionDistance = 500;
        endTrainStation.startPos = Vector3.forward * missionDistance;
        enginePowerBoost = 1;
        currentDistance = 0;
        LevelReferences.s.speed = 0;
        internalRealSpeed = 0;
        targetSpeed = 0;
        
        currentBreakPower = 0;
    }

    public void SetUpOnMissionStart() {
        ResetDistance();
        DisableLowPower();
        PlayEngineStartEffects();
    }

    public void RegisterRadar() {
        DistanceAndEnemyRadarController.s.RegisterUnit(this);
    }

    public void IncreaseMissionEndDistance(float amount) {
        SetMissionEndDistance(missionDistance + amount);
    }

    public void TravelToMissionEndDistance() {
        CalculateStopAcceleration();
        currentDistance = missionDistance;
    }
    
    public void SetMissionEndDistance(float distance) {
        missionDistance = distance;
        endTrainStation.startPos = Vector3.forward * missionDistance;
        HexGrid.s.ResetDistance();
    }

    public void AddEngine(EngineModule engineModule) {
        engines.Add(engineModule);
    }

    public void RemoveEngine(EngineModule engineModule) {
        engines.Remove(engineModule);
    }
    

    public float enginePowerChangeDelta = 100f;

    public float enginePowerToSpeedMultiplier = 1;

    public MiniGUI_SpeedDisplayArea speedDisplayArea;
    public MiniGUI_SpeedDisplayArea speedDisplayAreaShop;

    public float breakPower = 1f;

    public float trainWeightMultiplier = 0.8f;

    public float internalRealSpeed;
    public int activeEngines = 0;

    public float enginePowerPlayerControl = 1f;
    public float currentBreakPower = 0;

    public bool encounterOverride = false;

    public void PlayEngineStartEffects() {
        for (int i = 0; i < engines.Count; i++) {
            engines[i].OnEngineStart?.Invoke();
        }
    }
    
    public void SetEngineBoostEffects(bool isBoosting, bool isLowPower) {
        for (int i = 0; i < engines.Count; i++) {
            engines[i].OnEngineBoost?.Invoke(isBoosting);
        }
        for (int i = 0; i < engines.Count; i++) {
            engines[i].OnEngineLowPower?.Invoke(isLowPower);
        }
    }

    public float maxSpeed = 8;
    
    private void Update() {
        enginePower = 0;
        for (int i = 0; i < engines.Count; i++) {
            if (engines[i]) {
                enginePower += engines[i].enginePower;
                activeEngines += 1;
            } 
        }

        var trainWeight = Train.s.GetTrainWeight();
        trainWeight = (int)(trainWeight * trainWeightMultiplier);

        if (isBoosting || isSlow) {
            enginePower *= currentBoostMultiplier;
            if (isBoosting) {
                boostTimer -= Time.deltaTime;
            } else {
                boostTimer += Time.deltaTime;
            }
        }

        var engineTarget = enginePower * enginePowerBoost;

        var enginePowerReal = currentEnginePower * enginePowerToSpeedMultiplier;

        var minSpeed = 0.5f / Train.s.carts.Count;
        targetSpeed = speedMultiplier* (2 * (enginePowerReal / (Mathf.Sqrt(trainWeight)*17)) + minSpeed);
        var stabilizedSpeed = speedMultiplier* (2 * (engineTarget / (Mathf.Sqrt(trainWeight)*17)) + minSpeed); // used for the engine power speedometer

        targetSpeed = Mathf.Clamp(targetSpeed, minSpeed, maxSpeed);
        stabilizedSpeed = Mathf.Clamp(stabilizedSpeed, minSpeed, maxSpeed);

        speedDisplayArea.UpdateValues(Train.s.GetTrainWeight(), (int)enginePower, stabilizedSpeed, LevelReferences.s.speed);
        speedDisplayAreaShop.UpdateValues(Train.s.GetTrainWeight(), (int)enginePower, stabilizedSpeed, LevelReferences.s.speed);


        if (!encounterOverride) {
            if (PlayStateMaster.s.isCombatInProgress()) {

                currentEnginePower = Mathf.MoveTowards(currentEnginePower, engineTarget, enginePowerChangeDelta * Time.deltaTime);
                var acceleration = 0.4f - ((float)trainWeight).Remap(0, 2000, 0, 0.4f);
                acceleration = Mathf.Clamp(acceleration, 0.1f, 0.4f);
                if (targetSpeed > LevelReferences.s.speed) {
                    var excessEnginePower = (enginePowerReal / trainWeight);
                    acceleration += excessEnginePower.Remap(0, 0.5f, 0, 0.2f);
                }


                internalRealSpeed = Mathf.MoveTowards(internalRealSpeed, targetSpeed, acceleration * Time.deltaTime);
                LevelReferences.s.speed = Mathf.Max(internalRealSpeed - slowAmount, 0f);

                if (debugSpeedOverride > 0) {
                    LevelReferences.s.speed = debugSpeedOverride;
                }

                slowAmount = Mathf.Lerp(slowAmount, 0, slowDecay * Time.deltaTime);
                slowAmount = Mathf.Clamp(slowAmount, 0, 5);
                if (slowAmount <= 0.2f) {
                    ToggleSlowedEffect(false);
                }

                currentTime += Time.deltaTime;
                timeText.text = GetNiceTime(currentTime);

                currentDistance += LevelReferences.s.speed * Time.deltaTime;

                distanceText.text = ((int)currentDistance).ToString();

                if (currentDistance > missionDistance) {
                    MissionWinFinisher.s.MissionWon();
                    CalculateStopAcceleration();
                }
            } else if (PlayStateMaster.s.isCombatFinished() && !MissionLoseFinisher.s.isMissionLost) {
                var stopProgress = (currentDistance - beforeStopDistance) / stopLength;
                if (currentDistance >= stopMissionDistanceTarget) {
                    stopProgress = 1;
                }

                if (stopProgress < 1) {
                    LevelReferences.s.speed = Mathf.Lerp(beforeStopSpeed, 0, stopProgress * stopProgress);
                    LevelReferences.s.speed = Mathf.Clamp(LevelReferences.s.speed, 0.2f, float.MaxValue);

                    currentBreakPower = 10;

                    currentDistance += LevelReferences.s.speed * Time.deltaTime;
                } else {
                    LevelReferences.s.speed = 0;
                    currentBreakPower = 0;
                    currentDistance = stopMissionDistanceTarget;
                }
            } else {
                LevelReferences.s.speed = 0;
                currentEnginePower = 0;
            }
        }
    }

    //public readonly float stopDistance = 10f;
    public readonly float stopDistance = 7.5f;
    public float stopMissionDistanceTarget;
    public float beforeStopSpeed;
    public float beforeStopDistance;
    public float stopLength;
    void CalculateStopAcceleration() {
        beforeStopSpeed = LevelReferences.s.speed;
        if (beforeStopSpeed < 2f) {
            LevelReferences.s.speed = 2;
            beforeStopSpeed = LevelReferences.s.speed;
        }

        stopLength = stopDistance/* - (Train.s.GetTrainLength()/2f)*/;
        stopMissionDistanceTarget = missionDistance + stopLength;
        beforeStopDistance = missionDistance;
    }

    public static string GetNiceTime(float time) {
        var minutes = (int) (time / 60);
        var remainingSeconds = (int) (time - minutes * 60);
        return (minutes.ToString("00") + ':' + remainingSeconds.ToString("00"));
    }

    public string GetCurrentTime() {
        return GetNiceTime(currentTime);
    }


    public void UseSteam(float amount) {
        // not used anymore
    }

    [Header("Boost stuff")]
    public bool canBoost = true;
    public bool isBoosting = false;
    public bool isSlow = false;
    public float boostDuration = 30f;
    public float boostMultiplier = 2f;
    public float lowPowerMultiplier = 0.5f;
    public float lowPowerDuration = 60f;
    public float currentBoostMultiplier = 1;
    public float boostTimer;
    public float boostTotalTime;
    public float boostGraphicPercent {
        get {
            return Mathf.Clamp((boostTimer*2) / boostTotalTime,0,2f);
        }
    }

    public void ActivateBoost() {
        if (canBoost && !encounterOverride) {
            for (int i = 0; i < engines.Count; i++) {
                var boostable = engines[i].GetComponentInChildren<EngineBoostable>(true);
                if (boostable) {
                    boostable.gameObject.SetActive(false);
                }
            }

            isBoosting = true;
            currentBoostMultiplier = boostMultiplier;
            
            PlayEngineStartEffects();
            SetEngineBoostEffects(true, false);
            CameraController.s.BoostFOV();
            boostTimer = boostDuration;
            boostTotalTime = boostDuration;
            Invoke(nameof(DisableBoostAndActivateLowPowerMode), boostDuration);
        }
    }


    void DisableBoostAndActivateLowPowerMode() {
        currentBoostMultiplier = lowPowerMultiplier;
        CameraController.s.SlowFOV();

        isSlow = true;
        isBoosting = false;
    
        boostTimer = 0;
        boostTotalTime = lowPowerDuration;
        SetEngineBoostEffects(false, true);
        Invoke(nameof(DisableLowPower), lowPowerDuration);
    }

    public void DisableLowPower() {
        CancelInvoke(nameof(DisableBoostAndActivateLowPowerMode));
        CancelInvoke(nameof(DisableLowPower));
        
        SetEngineBoostEffects(false, false);
        PlayEngineStartEffects();
        CameraController.s.ReturnToRegularFOV();
        
        canBoost = true;
        currentBoostMultiplier = 1;
        isBoosting = false;
        isSlow = false;
        
        
        boostTimer = 1;
        boostTotalTime = 2;
        
        for (int i = 0; i < engines.Count; i++) {
            var boostable = engines[i].GetComponentInChildren<EngineBoostable>(true);
            if (boostable) {
                boostable.gameObject.SetActive(true);
            }
        }
    }

    public void OnCombatFinished() {
        DisableLowPower();
    }

    public float GetDistance() {
        return currentDistance;
    }

    public Sprite trainRadarImg;

    public bool IsTrain() {
        return true;
    }
    
    public Sprite GetIcon() {
        return trainRadarImg;
    }

    public bool isLeftUnit() {
        return false;
    }

    public float slowMultiplier = 0.5f;
    public float slowAmount;
    public float slowDecay = 0.1f;
    public void AddSlow(float amount) {
        amount *= slowMultiplier;
        if (slowAmount > 1)
            amount /= slowAmount;
        slowAmount += amount;
        ToggleSlowedEffect(true);
    }


    public List<GameObject> activeSlowedEffects = new List<GameObject>();
    private bool isSlowedOn = false;
    void ToggleSlowedEffect(bool isOn) {
        if (isOn && !isSlowedOn) {
            for (int i = 0; i < engines.Count; i++) {
                var effect = Instantiate(LevelReferences.s.currentlySlowedEffect, engines[i].transform.position, Quaternion.identity);
                effect.transform.SetParent(engines[i].transform);
                activeSlowedEffects.Add(effect);
            }

            isSlowedOn = true;
        }

        if (!isOn && isSlowedOn) {
            for (int i = 0; i < activeSlowedEffects.Count; i++) {
                SmartDestroy(activeSlowedEffects[i].gameObject);
            }
            
            activeSlowedEffects.Clear();
            isSlowedOn = false;
        }
    }
    
    void SmartDestroy(GameObject target) {
        var particles = GetComponentsInChildren<ParticleSystem>();

        foreach (var particle in particles) {
            particle.transform.SetParent(null);
            particle.Stop();
            Destroy(particle.gameObject, 1f);
        }
            
        Destroy(target);
    }
}
