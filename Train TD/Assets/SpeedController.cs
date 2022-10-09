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
    public float enginePowerPlayerControl = 1f;
    public float throttlePlayerControl = 1f;
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
    public ScrapBoxScript fuelAmount;
    public TMP_Text fuelUseText;
    public Slider enginePowerSlider;
    public Image engineFill;
    public Color[] engineFillColors;
    public Color engineDisabledColor;
    public GameObject engineDisabledWarning;
    public float fuelUseMultiplier = 0.25f;
    public TMP_Text steamGenerationText;
    public float steamGenerationMultiplier = 0.25f;
    
    
    public float steam;
    public float maxSteam;
    public ScrapBoxScript steamAmount;
    public TMP_Text steamUseText;
    public Slider throttleSlider;
    public Image throttleFill;
    public Color[] throttleFillColors;
    public Color throttleDisabledColor;
    public GameObject throttleDisabledWarning;
    public float steamUseMultiplier = 0.25f;

    
    

    public List<EngineModule> engines = new List<EngineModule>();

    public void UpdateBasedOnLevelData() {
        var myLevel = SceneLoader.s.currentLevel;
        missionDistance = myLevel.missionDistance;
        endTrainStation.startPos = Vector3.forward * missionDistance;
        var currentResources = DataSaver.s.GetCurrentSave().currentRun.myResources;
        fuel = currentResources.fuel;
        maxFuel = currentResources.maxFuel;
        steam = currentResources.steam;
        maxSteam = currentResources.maxSteam;
        
        fuelAmount.SetScrap(fuel);
        fuelAmount.SetMaxScrap(maxFuel);
        steamAmount.SetScrap(steam);
        steamAmount.SetMaxScrap(maxSteam);
    }

    private void Start() {
        DistanceAndEnemyRadarController.s.RegisterUnit(this);
    }

    public void AddEngine(EngineModule engineModule) {
        engines.Add(engineModule);
    }

    public void RemoveEngine(EngineModule engineModule) {
        engines.Remove(engineModule);
    }
    

    public float enginePowerChangeDelta = 100f;

    private void Update() {
        if (SceneLoader.s.isLevelInProgress) {
            enginePowerTarget = 0;
            for (int i = 0; i < engines.Count; i++) {
                enginePowerTarget += engines[i].enginePower;
            }
            
            
            var trainWeight = Train.s.GetTrainWeight();
            trainWeightText.text = trainWeight.ToString();

            DoFuelAndPlayerThrottleControls();
            DoSteamAndPlayerEngineControls();

            var minSpeed = 0.5f / Train.s.carts.Count;
            targetSpeed = 2 * (enginePower / trainWeight) + minSpeed;
            var acceleration = 0.35f - (trainWeight/5000f);
            acceleration = Mathf.Clamp(acceleration, 0.05f, 0.35f);
            if (targetSpeed > LevelReferences.s.speed) {
                var excessEnginePower = (enginePower / trainWeight);
                acceleration += (excessEnginePower / 4f);
            }
            
            var steamGen = (enginePower/100) * throttlePlayerControl * steamGenerationMultiplier;
            steam += steamGen * Time.deltaTime;
            steamGenerationText.text = $"+{steamGen:F2}/s";
            steam = Mathf.Clamp(steam, 0, maxSteam);


            enginePower = Mathf.MoveTowards(enginePower, enginePowerTarget * enginePowerPlayerControl, enginePowerChangeDelta * Time.deltaTime);
            LevelReferences.s.speed = Mathf.MoveTowards(LevelReferences.s.speed, targetSpeed*speedMultiplier, acceleration * Time.deltaTime);

            trainSpeedText.text = $"{LevelReferences.s.speed:F1}";
            enginePowerText.text = enginePower.ToString("F0");

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

    void DoSteamAndPlayerEngineControls() {
        var steamUse = (enginePowerTarget*enginePowerPlayerControl)/100 * steamUseMultiplier;
        if (enginePowerPlayerControl > 1f) {
            steamUse *= enginePowerPlayerControl;
        }else if (enginePowerPlayerControl <= 0.5f) {
            steamUse *= 0.8f;
        }

        steamUseText.text = $"-{steamUse:F2}/s";

        if (steamUse > 0) {
            steam -= steamUse * Time.deltaTime;
            steam = Mathf.Clamp(steam, 0, maxSteam);
            steamAmount.SetScrap(steam);
        }

        if (steam <= 0) {
            enginePowerPlayerControl = 0;
            
            engineFill.color = engineDisabledColor;
            engineDisabledWarning.SetActive( true);
        } else {
            enginePowerPlayerControl = enginePowerSlider.value/4f;
            
            engineFill.color = engineFillColors[(int)enginePowerSlider.value];
            engineDisabledWarning.SetActive( false);
        }
    }
    
    void DoFuelAndPlayerThrottleControls() {
        var fuelUse = (enginePowerTarget*throttlePlayerControl)/100 * fuelUseMultiplier;
        if (throttlePlayerControl > 1f) {
            fuelUse *= throttlePlayerControl;
        }else if (throttlePlayerControl <= 0.5f) {
            fuelUse *= 0.8f;
        }

        fuelUseText.text = $"-{fuelUse:F2}/s";

        if (fuelUse > 0) {
            fuel -= fuelUse * Time.deltaTime;
            fuel = Mathf.Clamp(fuel, 0, maxFuel);
            fuelAmount.SetScrap(fuel);
        }

        if (fuel <= 0) {
            throttlePlayerControl = 0;
            
            throttleFill.color = throttleDisabledColor;
            throttleDisabledWarning.SetActive( true);
        } else {
            throttlePlayerControl = throttleSlider.value/4f;
            
            throttleFill.color = throttleFillColors[(int)throttleSlider.value];
            throttleDisabledWarning.SetActive( false);
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
    
    public void AddFuel(int amount) {
        fuel += amount;
        fuel = Mathf.Clamp(fuel, 0, maxFuel);
    }

    public float GetDistance() {
        return currentDistance;
    }

    public Sprite trainRadarImg;
    
    public Sprite GetIcon() {
        return trainRadarImg;
    }
}
