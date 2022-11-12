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

    public float currentTime = 0;
    public float currentDistance = 0;

    public float missionDistance = 300; //100 engine power goes 1 distance per second

    public float enginePower = 0;
    public float enginePowerTarget = 0;
    public float enginePowerBoost = 1f;
    public float enginePowerPlayerControl = 1f;
    public float targetSpeed;
    public float speedMultiplier = 1.5f;

    public TMP_Text enginePowerText;
    public TMP_Text trainWeightText;
    public TMP_Text trainSpeedText;
    
    public TMP_Text timeText;
    public TMP_Text distanceText;

    public TrainStation endTrainStation;

    public float fuel;
    public float maxFuel;

    //public TMP_Text fuelText;
    public TMP_Text fuelUseText;

    public Slider enginePowerSlider;
    public Image engineFill;
    public Color[] engineFillColors;
    public Color engineDisabledColor;
    public GameObject engineDisabledWarning;

    public ScrapBoxScript myFuelShower;
    public ScrapBoxScript mySteamShower;

    public float steam;
    public float maxSteam;

    public List<EngineModule> engines = new List<EngineModule>();

    public void UpdateBasedOnLevelData() {
        var myLevel = SceneLoader.s.currentLevel;
        missionDistance = myLevel.missionDistance;
        endTrainStation.startPos = Vector3.forward * missionDistance;
        enginePowerBoost = 1;
        var myResources = DataSaver.s.GetCurrentSave().currentRun.myResources;
        fuel = myResources.fuel;
        maxFuel = myResources.maxFuel;
        myFuelShower.SetMaxScrap(maxFuel);
        maxSteam = engines.Count*100;
        steam = maxSteam/2f;
        mySteamShower.SetMaxScrap(maxSteam);
    }

    private void Start() {
        DistanceAndEnemyRadarController.s.RegisterUnit(this);
    }

    public void AddEngine(EngineModule engineModule) {
        engines.Add(engineModule);
        maxSteam = engines.Count*100;
        mySteamShower.SetMaxScrap(maxSteam);

        if (!SceneLoader.s.isLevelInProgress) {
            steam = maxSteam/2f;
        }
    }

    public void RemoveEngine(EngineModule engineModule) {
        engines.Remove(engineModule);
        maxSteam = engines.Count*100;
        mySteamShower.SetMaxScrap(maxSteam);
        if (!SceneLoader.s.isLevelInProgress) {
            steam = maxSteam/2f;
        }
    }
    

    public float enginePowerChangeDelta = 100f;

    public float fuelUseMultiplier = 1f;
    public SpeedometerScript mySpeedometer;

    public float steamPer100EnginePower = 1;
    public float steamUsePercentage = 0.15f;
    public float steamUseToEnginePowerConversion = 100;

    public float debugSteamGeneration;
    public float debugSteamUse;

    public float excessPowerAccelerationBonus = 1f;

    public float trainWeightMultiplier = 0.8f;
    private void Update() {
        if (SceneLoader.s.isLevelInProgress) {
            enginePowerTarget = 0;
            for (int i = 0; i < engines.Count; i++) {
                enginePowerTarget += engines[i].enginePower;
            }
            
            var trainWeight = Train.s.GetTrainWeight();
            trainWeightText.text = trainWeight.ToString();
            trainWeight = (int)(trainWeight * trainWeightMultiplier);

            DoFuelAndPlayerEngineControls();
            
            
            var engineTarget = enginePowerTarget * enginePowerBoost * enginePowerPlayerControl;
            enginePower = Mathf.MoveTowards(enginePower, engineTarget, enginePowerChangeDelta * Time.deltaTime);

            var steamGenerationPerSecond = steamPer100EnginePower * (enginePower/100f);
            debugSteamGeneration = steamGenerationPerSecond;
            steam += steamGenerationPerSecond * Time.deltaTime;
            
            mySteamShower.SetScrap(steam);

            var steamUsePerSecond = steam * steamUsePercentage;
            debugSteamUse = steamUsePerSecond;
            steam -= steamUsePerSecond*Time.deltaTime;
            
            mySteamShower.SetScrap(steam);

            steam = Mathf.Clamp(steam, 0, maxSteam);

            var pressurePower = steamUsePerSecond * steamUseToEnginePowerConversion;
            

            var minSpeed = 0.5f / Train.s.carts.Count;
            targetSpeed = 2 * (pressurePower / (Mathf.Sqrt(trainWeight)*17)) + minSpeed;
            var acceleration = 0.2f - (trainWeight/2000f);
            acceleration = Mathf.Clamp(acceleration, 0.01f, 0.35f);
            if (targetSpeed > LevelReferences.s.speed) {
                var excessEnginePower = (pressurePower / trainWeight);
                acceleration += excessEnginePower * excessPowerAccelerationBonus;
            }


            
            LevelReferences.s.speed = Mathf.MoveTowards(LevelReferences.s.speed, targetSpeed*speedMultiplier, acceleration * Time.deltaTime);

            trainSpeedText.text = $"{LevelReferences.s.speed:F1}";
            mySpeedometer.SetSpeed(LevelReferences.s.speed);
            enginePowerText.text = enginePower.ToString("F0");
            
            myFuelShower.SetScrap(fuel);

            currentTime += Time.deltaTime;
            timeText.text = GetNiceTime(currentTime);

            currentDistance += LevelReferences.s.speed * Time.deltaTime;

            distanceText.text = ((int)currentDistance).ToString();


            if (currentDistance > missionDistance) {
                MissionWinFinisher.s.MissionWon();
                CalculateStopAcceleration();
            }
        } else if (SceneLoader.s.isLevelFinished()) {
            LevelReferences.s.speed = Mathf.MoveTowards(LevelReferences.s.speed, 0, stopAcceleration * Time.deltaTime);
            if (stopAcceleration <= 0) {
                CalculateStopAcceleration();
            }
            
            currentDistance += LevelReferences.s.speed * Time.deltaTime;
        } else {
            LevelReferences.s.speed = 0;
            enginePower = 0;
            enginePowerText.text = enginePowerTarget.ToString("F0");
        }
    }

    private bool playedNoFuelSound = false;
    void DoFuelAndPlayerEngineControls() {
        var engineCountMultiplier = 1f;
        if(engines.Count > 0)
            engineCountMultiplier = ((engines.Count-1)/2f + 1)/engines.Count;

        var fuelUse = (enginePowerTarget*enginePowerPlayerControl)/100 * fuelUseMultiplier;
        if (enginePowerPlayerControl > 1.25f) {
            fuelUse *= 1.2f;
        }else if(enginePowerPlayerControl > 1f) {
            fuelUse *= 1.1f;
        }else if (enginePowerPlayerControl <= 0.5f) {
            fuelUse *= 0.8f;
        }

        fuelUse *= engineCountMultiplier;

        fuelUseText.text = $"-{fuelUse:F2}/s";
        fuel -= fuelUse * Time.deltaTime;
        fuel = Mathf.Clamp(fuel, 0, maxFuel);

        if (fuel <= 0) {
            enginePowerPlayerControl = 0;
            
            engineFill.color = engineDisabledColor;
            engineDisabledWarning.SetActive( true);
            
            if (!playedNoFuelSound) {
                SoundscapeController.s.PlayNoMoreResource(ResourceTypes.fuel);
                playedNoFuelSound = true;
            }
        } else {
            enginePowerPlayerControl = enginePowerSlider.value/4f;
            
            engineFill.color = engineFillColors[(int)enginePowerSlider.value];
            engineDisabledWarning.SetActive( false);
            
            if (playedNoFuelSound) {
                playedNoFuelSound = false;
            }
        }
    }


    public void ApplyStorageAmounts(int _maxFuel) {
        maxFuel = _maxFuel;
        myFuelShower.SetMaxScrap(maxFuel);
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


    public void ModifyFuel(float amount) {
        if (fuel + amount > maxFuel || fuel + amount < 0) {
            return;
        }
        if (SceneLoader.s.isLevelInProgress) {
            fuel += amount;
        } else {
            var currentRunMyResources = DataSaver.s.GetCurrentSave().currentRun.myResources;
            currentRunMyResources.fuel += (int)amount;
            currentRunMyResources.fuel = Mathf.Clamp(currentRunMyResources.fuel, 0, currentRunMyResources.maxFuel);
            fuel = currentRunMyResources.fuel;
        }
        
        if (fuel <= 0) {
            SoundscapeController.s.PlayNoMoreResource(ResourceTypes.fuel);
        }
    }
    

    public void UseSteam(float amount) {
        steam -= amount;
    }

    public float GetDistance() {
        return currentDistance;
    }

    public Sprite trainRadarImg;
    
    public Sprite GetIcon() {
        return trainRadarImg;
    }
}
