using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
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
    public float engineSpeedBoost = 1f;

    public TMP_Text enginePowerText;
    
    public TMP_Text timeText;
    public TMP_Text distanceText;

    public TrainStation endTrainStation;

    public float fuel;

    public void UpdateBasedOnLevelData() {
        var myLevel = SceneLoader.s.currentLevel;
        missionDistance = myLevel.missionDistance;
        endTrainStation.startPos = Vector3.forward * missionDistance;
        engineSpeedBoost = 1;
    }

    private void Start() {
        DistanceAndEnemyRadarController.s.RegisterUnit(this);
    }

    public void UpdateEnginePower(int change) {
        enginePowerTarget += change;
        engineCount += change > 0 ? 1 : -1;
    }

    public int engineCount = 0;
    public float enginePowerChangeDelta = 100f;
    private void Update() {
        if (SceneLoader.s.isLevelInProgress) {
            enginePower = Mathf.MoveTowards(enginePower, enginePowerTarget*engineSpeedBoost, enginePowerChangeDelta * engineCount * Time.deltaTime);
            LevelReferences.s.speed = EnginePowerToDistance(enginePower, 1f);
            enginePowerText.text = enginePower.ToString("F0");
            
            currentTime += Time.deltaTime;
            timeText.text = GetNiceTime(currentTime);
        
            currentDistance += EnginePowerToDistance(enginePower, Time.deltaTime);
        
            distanceText.text = ((int)currentDistance).ToString();
            

            if (currentDistance > missionDistance) {
                MissionWinFinisher.s.MissionWon();
                CalculateStopAcceleration();
            }
        } else if (SceneLoader.s.isLevelFinished()) {
            LevelReferences.s.speed  = Mathf.MoveTowards(LevelReferences.s.speed , 0, stopAcceleration * Time.deltaTime);;
            currentDistance += LevelReferences.s.speed * Time.deltaTime;
        }else {
            LevelReferences.s.speed = 0;
            enginePower = 0;
            enginePowerText.text = enginePowerTarget.ToString("F0");
        }
    }

    // v2 = u2 + 2as
    // 0 = u2 + 2as
    // u2 = 2as
    // u2/2s = a
    public readonly float stopDistance = 20.101f;
    private float stopAcceleration = 0;
    void CalculateStopAcceleration() {
        var speed =  EnginePowerToDistance(enginePower, 1f);
        var realStopDistance = stopDistance - (Mathf.Floor(Train.s.carts.Count / 2f) * DataHolder.s.cartLength);
        stopAcceleration = (speed * speed) / (2 * realStopDistance);
    }

    public static float EnginePowerToDistance(float power, float deltaTime) {
        return ((power+50) / 50f) * deltaTime;
    }
    
    public static float EnginePowerAndDistanceToRemainingTime(float power, float remainingDistance) {
        return remainingDistance / EnginePowerToDistance(power, 1);
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
    }

    public float GetDistance() {
        return currentDistance;
    }

    public Sprite trainRadarImg;
    
    public Sprite GetIcon() {
        return trainRadarImg;
    }
}
