using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_DistanceShower : MonoBehaviour {

    public bool isCurrentDistance = false;

    public Image mainImage;
    public Image leftArrow;
    public Image rightArrow;

    private Slider slider;

    public Color gonnaMakeItColor = Color.green;
    public Color gonnaFailColor = Color.red;
    
    public Color fasterThanPlayerColor = Color.white;
    public Color slowerThanPlayerColor = Color.gray;

    private void Start() {
        slider = GetComponent<Slider>();
    }

    public int UpdateValue(float curDistance,  float enginePower) {
        bool isGonnaMakeIt = false;
        if (slider != null) {

            var totalDistance = SpeedController.s.missionDistance;
            slider.value = curDistance / totalDistance;

            var isFasterThanPlayer = enginePower > SpeedController.s.enginePower;
            isGonnaMakeIt = IsGonnaMakeIt(curDistance, totalDistance, enginePower);

            if (!isCurrentDistance) {
                if (isFasterThanPlayer) {
                    rightArrow.color = fasterThanPlayerColor;
                    leftArrow.color = slowerThanPlayerColor;
                } else {
                    rightArrow.color = slowerThanPlayerColor;
                    leftArrow.color = fasterThanPlayerColor;
                }

                if (isGonnaMakeIt) {
                    mainImage.color = gonnaMakeItColor;
                } else {
                    mainImage.color = gonnaFailColor;
                }
            }
        }

        
        return isGonnaMakeIt ? 1 :0;
    }

    bool IsGonnaMakeIt(float curDistance, float totalDistance, float enginePower) {
        var remainingDistance = curDistance - totalDistance;
        var remainingTime = SpeedController.EnginePowerAndDistanceToRemainingTime(enginePower, remainingDistance);

        var playerRemainingDistance = SpeedController.s.currentDistance - totalDistance;
        var playerRemainingTime = SpeedController.EnginePowerAndDistanceToRemainingTime(SpeedController.s.enginePower, playerRemainingDistance);

        return playerRemainingTime > remainingTime;
    }
}
