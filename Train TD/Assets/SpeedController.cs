using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class SpeedController : MonoBehaviour {
    public static SpeedController s;

    private void Awake() {
        s = this;
    }


    public float bestEngine = 200;
    public float bestDistance = 0;
    public float medEngine = 150;
    public float medDistance = 0;
    public float worstEngine = 100;
    public float worstDistance = 0;

    public float currentTime = 0;
    public float currentDistance = 0;

    public float missionDistance = 300; //100 engine power goes 1 distance per second

    public float enginePower = 0;
    public float enginePowerTarget = 0;
    public float engineSpeedBoost = 1f;

    public TMP_Text enginePowerText;

    public MiniGUI_DistanceShower missionCurrentSlider;
    public MiniGUI_DistanceShower missionBestSlider;
    public MiniGUI_DistanceShower missionMedSlider;
    public MiniGUI_DistanceShower missionWorstSlider;

    public TMP_Text timeText;
    public TMP_Text distanceText;

    public TrainStation endTrainStation;

    public void UpdateBasedOnLevelData() {
        var myLevel = SceneLoader.s.currentLevel;
        bestEngine = myLevel.bestEngineSpeed;
        medEngine = myLevel.mediumEngineSpeed;
        worstEngine = myLevel.worstEngineSpeed;
        missionDistance = myLevel.missionDistance;
        endTrainStation.startPos = Vector3.forward * missionDistance;
        engineSpeedBoost = 1;
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
            missionCurrentSlider.UpdateValue(currentDistance,  enginePower);
        
            bestDistance += EnginePowerToDistance(bestEngine, Time.deltaTime);
            var bestStar = missionBestSlider.UpdateValue(bestDistance,  bestEngine);
        
            medDistance += EnginePowerToDistance(medEngine, Time.deltaTime);
            var medStar = missionMedSlider.UpdateValue(medDistance,  medEngine);
        
            worstDistance += EnginePowerToDistance(worstEngine, Time.deltaTime);
            var worstStar = missionWorstSlider.UpdateValue(worstDistance,  worstEngine);

            StarController.s.UpdateSpeedStars(bestStar + medStar + worstStar);

            
            if (worstDistance > missionDistance) {
                MissionLoseFinisher.s.MissionLost(false);
                //CalculateStopAcceleration();
            }
            

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
        var realStopDistance = stopDistance - (Mathf.Floor(SceneLoader.s.currentLevel.trainLength / 2f) * DataHolder.s.cartLength);
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

    public string GetWorstTime() {
        return GetNiceTime(missionDistance / EnginePowerToDistance(worstEngine, 1));
    }
    
    public string GetMedTime() {
        return GetNiceTime(missionDistance / EnginePowerToDistance(medEngine, 1));
    }
    
    public string GetBestTime() {
        return GetNiceTime(missionDistance / EnginePowerToDistance(bestEngine, 1));
    }

    public string GetCurrentTime() {
        return GetNiceTime(currentTime);
    }
}
