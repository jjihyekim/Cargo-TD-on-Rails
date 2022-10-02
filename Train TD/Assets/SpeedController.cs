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

    public TMP_Text fuelText;
    public TMP_Text fuelUseText;

    public Slider enginePowerSlider;
    public Image engineFill;
    public Color[] engineFillColors;
    public Color engineDisabledColor;
    public GameObject engineDisabledWarning;

    public List<EngineModule> engines = new List<EngineModule>();

    public void UpdateBasedOnLevelData() {
        var myLevel = SceneLoader.s.currentLevel;
        missionDistance = myLevel.missionDistance;
        endTrainStation.startPos = Vector3.forward * missionDistance;
        enginePowerBoost = 1;
        fuel = DataSaver.s.GetCurrentSave().currentRun.fuel;
        maxFuel = DataSaver.s.GetCurrentSave().currentRun.maxFuel;
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

    public float fuelUseMultiplier = 1f;

    private void Update() {
        if (SceneLoader.s.isLevelInProgress) {
            enginePowerTarget = 0;
            for (int i = 0; i < engines.Count; i++) {
                enginePowerTarget += engines[i].enginePower;
            }
            
            
            fuelText.text = $"{fuel:F0}/{maxFuel:F0}";
            trainWeightText.text = Train.s.trainWeight.ToString();

            DoFuelAndPlayerEngineControls();

            targetSpeed = 2 * (enginePower / Train.s.trainWeight);
            var acceleration = 0.5f - (Train.s.trainWeight/5000f);
            acceleration = Mathf.Clamp(acceleration, 0.1f, 0.5f);
            if (targetSpeed > LevelReferences.s.speed) {
                var excessEnginePower = (enginePower / Train.s.trainWeight);
                acceleration += (excessEnginePower / 4f);
            }

            enginePower = Mathf.MoveTowards(enginePower, enginePowerTarget * enginePowerBoost * enginePowerPlayerControl, enginePowerChangeDelta * Time.deltaTime);
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

    void DoFuelAndPlayerEngineControls() {
        var fuelUse = (enginePowerTarget*enginePowerPlayerControl)/100 * fuelUseMultiplier;
        if (enginePowerPlayerControl > 1f) {
            fuelUse *= enginePowerPlayerControl;
        } 

        fuelUseText.text = $"-{fuelUse:F1}/s";
        fuel -= fuelUse * Time.deltaTime;
        fuel = Mathf.Clamp(fuel, 0, maxFuel);

        if (fuel <= 0) {
            enginePowerPlayerControl = 0;
            
            engineFill.color = engineDisabledColor;
            engineDisabledWarning.SetActive( true);
        } else {
            enginePowerPlayerControl = enginePowerSlider.value/4f;
            
            engineFill.color = engineFillColors[(int)enginePowerSlider.value];
            engineDisabledWarning.SetActive( false);
        }
    }

    // v2 = u2 + 2as
    // 0 = u2 + 2as
    // u2 = 2as
    // u2/2s = a
    public readonly float stopDistance = 20.101f;
    private float stopAcceleration = 0;

    void CalculateStopAcceleration() {
        var speed = LevelReferences.s.speed;
        var realStopDistance = stopDistance - (Mathf.Floor(Train.s.carts.Count / 2f) * DataHolder.s.cartLength);
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
