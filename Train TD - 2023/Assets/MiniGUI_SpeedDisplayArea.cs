using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_SpeedDisplayArea : MonoBehaviour
{

	public SpeedometerScript mySpeedometer;
	public SpeedometerScript myEngineSpeedometer;
	public TMP_Text enginePowerText;
	public TMP_Text trainWeightText;
	public TMP_Text trainSpeedText;

	public void UpdateValues(int weight, int enginePower, float targetSpeed, float actualSpeed) {
		
		trainWeightText.text = weight.ToString();
		trainSpeedText.text = $"{(targetSpeed*10):F0}";
		mySpeedometer.SetSpeed(actualSpeed);
		myEngineSpeedometer.SetSpeed(targetSpeed); // used for the engine power speedometer
		enginePowerText.text = enginePower.ToString("F0");
	}
}
