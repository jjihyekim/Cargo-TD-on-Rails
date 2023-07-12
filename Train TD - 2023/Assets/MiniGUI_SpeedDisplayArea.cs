using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_SpeedDisplayArea : MonoBehaviour
{

	public SpeedometerScript mySpeedometer;
	public SpeedometerScript myEngineSpeedometer;
	public TMP_Text trainSpeedText;

	public void UpdateValues(float targetSpeed, float actualSpeed) {
		trainSpeedText.text = $"{(targetSpeed*10):F0}";
		mySpeedometer.SetSpeed(actualSpeed);
		myEngineSpeedometer.SetSpeed(targetSpeed); // used for the engine power speedometer
	}
}
