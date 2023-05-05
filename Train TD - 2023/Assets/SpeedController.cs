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

    public float enginePower = 0;
    public float fuelPower = 0;
    public float nuclearPower = 0;
    public float enginePowerBoost = 1f;
    public int enginePowerPlayerControl = 1;
    public float targetSpeed;
    public float speedMultiplier = 1.5f;

    public TMP_Text enginePowerText;
    public TMP_Text trainWeightText;
    public TMP_Text trainSpeedText;
    
    public TMP_Text timeText;
    public TMP_Text distanceText;

    public TrainStation endTrainStation;

    //public TMP_Text fuelText;
    //public TMP_Text fuelUseText;

    public Slider enginePowerSlider;
    public Image engineFill;
    public Color[] engineFillColors;
    public Color engineDisabledColor;
    public GameObject engineDisabledWarning;

    public float steam;
    public float maxSteam;

    public List<EngineModule> engines = new List<EngineModule>();

    public void SetUpOnMissionStart() {
        var myLevel = PlayStateMaster.s.currentLevel;
        missionDistance = 500;
        
        
        endTrainStation.startPos = Vector3.forward * missionDistance;
        enginePowerBoost = 1;

        maxSteam = engines.Count*100;
        steam = maxSteam/2f;


        currentDistance = 0;
        LevelReferences.s.speed = 0;
        internalRealSpeed = 0;
        targetSpeed = 0;
        
        
        DistanceAndEnemyRadarController.s.RegisterUnit(this);
    }

    public void IncreaseMissionEndDistance(float amount) {
        SetMissionEndDistance(missionDistance + amount);
    }
    
    public void SetMissionEndDistance(float distance) {
        missionDistance = distance;
        endTrainStation.startPos = Vector3.forward * missionDistance;
        HexGrid.s.ResetDistance();
    }

    public void AddEngine(EngineModule engineModule) {
        engines.Add(engineModule);
        maxSteam = engines.Count*100;

        if (!PlayStateMaster.s.isCombatInProgress()) {
            steam = maxSteam/2f;
        }
    }

    public void RemoveEngine(EngineModule engineModule) {
        engines.Remove(engineModule);
        maxSteam = engines.Count*100;
        if (!PlayStateMaster.s.isCombatInProgress()) {
            steam = maxSteam/2f;
        }
    }
    

    public float enginePowerChangeDelta = 100f;

    public float fuelUseMultiplier = 1f;
    public SpeedometerScript mySpeedometer;
    public SpeedometerScript mySteamSpeedometer;
    public SpeedometerScript myEngineSpeedometer;

    public float steamPer100EnginePower = 1;
    public float steamUsePercentage = 0.15f;
    public float steamUseToEnginePowerConversion = 100;

    public float debugSteamGeneration;
    public float debugSteamUse;

    public float engineOverloadPowerIncrease = 1.5f;
    public float breakPower = 1f;

    private float currentEngineOverloadPowerMultiplier = 1f;
    public float currentBreakPower = 0;

    public float trainWeightMultiplier = 0.8f;

    public float internalRealSpeed;
    public int activeEngines = 0;
    private void Update() {
        if (PlayStateMaster.s.isCombatInProgress()) {
            fuelPower = 0;
            nuclearPower = 0;
            for (int i = 0; i < engines.Count; i++) {
                if (engines[i].isNuclear) {
                    nuclearPower += engines[i].enginePower;
                    activeEngines += 1;
                } else {
                    if (engines[i].hasFuel) {
                        fuelPower += engines[i].enginePower;
                        activeEngines += 1;
                    }
                }
            }

            var trainWeight = Train.s.GetTrainWeight();
            trainWeightText.text = trainWeight.ToString();
            trainWeight = (int)(trainWeight * trainWeightMultiplier);

            DoFuelAndPlayerEngineControls();

            var engineTarget = (fuelPower+nuclearPower) * enginePowerBoost * currentEngineOverloadPowerMultiplier;
            enginePower = Mathf.MoveTowards(enginePower, engineTarget, enginePowerChangeDelta * Time.deltaTime);

            var steamGenerationPerSecond = steamPer100EnginePower * (enginePower/100f);
            debugSteamGeneration = steamGenerationPerSecond;
            steam += steamGenerationPerSecond * Time.deltaTime;

            var steamUsePerSecond = steam * steamUsePercentage;
            debugSteamUse = steamUsePerSecond;
            if(currentBreakPower <= 0) // dont use steam if breaking
                steam -= steamUsePerSecond*Time.deltaTime;
            

            steam = Mathf.Clamp(steam, 0, maxSteam);

            var pressurePower = steamUsePerSecond * steamUseToEnginePowerConversion;
            var stabilizedPressurePower = steamGenerationPerSecond * steamUseToEnginePowerConversion; // used for the engine power speedometer

            var minSpeed = 0.5f / Train.s.carts.Count;
            targetSpeed = speedMultiplier* (2 * (pressurePower / (Mathf.Sqrt(trainWeight)*17)) + minSpeed);
            var stabilizedSpeed = speedMultiplier* (2 * (stabilizedPressurePower / (Mathf.Sqrt(trainWeight)*17)) + minSpeed); // used for the engine power speedometer
            var acceleration = 0.4f - ((float)trainWeight).Remap(0,2000,0,0.4f);
            acceleration = Mathf.Clamp(acceleration, 0.1f, 0.4f);
            if (targetSpeed > LevelReferences.s.speed) {
                var excessEnginePower = (pressurePower / trainWeight);
                acceleration += excessEnginePower.Remap(0,0.5f,0,0.2f);
            } else {
                if(currentBreakPower > 0)
                    acceleration *= currentBreakPower;
            }

            if (currentBreakPower > 0) {
                targetSpeed = 0;
            }
            

            internalRealSpeed = Mathf.MoveTowards(internalRealSpeed, targetSpeed, acceleration * Time.deltaTime);
            LevelReferences.s.speed = Mathf.Max( internalRealSpeed - slowAmount, 0f);
            
            if (debugSpeedOverride > 0) {
                LevelReferences.s.speed = debugSpeedOverride;
            }
            
            slowAmount = Mathf.Lerp(slowAmount, 0, slowDecay * Time.deltaTime);
            slowAmount = Mathf.Clamp(slowAmount, 0, 5);
            if (slowAmount <= 0.2f) {
                ToggleSlowedEffect(false);
            }

            trainSpeedText.text = $"{LevelReferences.s.speed:F1}";
            mySpeedometer.SetSpeed(LevelReferences.s.speed);
            mySteamSpeedometer.SetSpeed(targetSpeed);
            myEngineSpeedometer.SetSpeed(stabilizedSpeed); // used for the engine power speedometer
            enginePowerText.text = enginePower.ToString("F0");

            currentTime += Time.deltaTime;
            timeText.text = GetNiceTime(currentTime);

            currentDistance += LevelReferences.s.speed * Time.deltaTime;

            distanceText.text = ((int)currentDistance).ToString();


            if (currentDistance > missionDistance) {
                MissionWinFinisher.s.MissionWon();
                CalculateStopAcceleration();
            }
        } else if (PlayStateMaster.s.isCombatFinished()) {
            LevelReferences.s.speed = Mathf.MoveTowards(LevelReferences.s.speed, 0, stopAcceleration * Time.deltaTime);
            if (stopAcceleration <= 0) {
                CalculateStopAcceleration();
            }
            
            currentDistance += LevelReferences.s.speed * Time.deltaTime;
        } else {
            LevelReferences.s.speed = 0;
            enginePower = 0;
            enginePowerText.text = fuelPower.ToString("F0");
        }
    }

    public GameObject selfDamageWarning;
    private bool playedNoFuelSound = false;
    void DoFuelAndPlayerEngineControls() {
        enginePowerPlayerControl = Mathf.RoundToInt(enginePowerSlider.value);

        var selfDamage = enginePowerPlayerControl > 1;

        for (int i = 0; i < engines.Count; i++) {
            engines[i].SetSelfDamageState(selfDamage);
            engines[i].UseFuel(enginePowerPlayerControl);
        }

        switch (enginePowerPlayerControl) {
            case 0:
                currentEngineOverloadPowerMultiplier = 1;
                currentBreakPower = breakPower;
                break;
            case 1:
                currentEngineOverloadPowerMultiplier = 1;
                currentBreakPower = 0;
                break;
            case 2:
                currentEngineOverloadPowerMultiplier = engineOverloadPowerIncrease;
                currentBreakPower = 0;
                break;
        }
        
        selfDamageWarning.SetActive(selfDamage);
        
        
        if (activeEngines <= 0) {
            engineFill.color = engineDisabledColor;
            engineDisabledWarning.SetActive( true);
        } else {
            engineFill.color = engineFillColors[(int)enginePowerSlider.value];
            engineDisabledWarning.SetActive( false);
        }
    }

    // v2 = u2 + 2as
    // 0 = u2 + 2as
    // u2 = 2as
    // u2/2s = a
    public readonly float stopDistance = 20f;
    private float stopAcceleration = 0;

    void CalculateStopAcceleration() {
        var speed = LevelReferences.s.speed;
        var realStopDistance = stopDistance - (Train.s.GetTrainLength()/2f);
        stopAcceleration = (speed * speed) / (2 * realStopDistance);
        stopAcceleration = Mathf.Clamp(stopAcceleration, 0.05f, float.MaxValue);
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
        steam -= amount;
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

    public float slowAmount;
    public float slowDecay = 0.1f;
    public void AddSlow(float amount) {
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
