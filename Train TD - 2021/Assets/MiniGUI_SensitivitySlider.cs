using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_SensitivitySlider : MonoBehaviour, IInitRequired
{
    const string exposedName = "sensitivity";
    public Slider mySlider;
    
    public void Initialize() {
        var audVal = PlayerPrefs.GetFloat(exposedName, 2.5f);
        mySlider.value = audVal;
        SetVol(audVal);
    }

    public void OnSliderUpdated() {
        PlayerPrefs.SetFloat(exposedName, mySlider.value);
        SetVol(mySlider.value);
    }

    void SetVol(float sliderVal) {
        CameraController.s.overallSensitivity = sliderVal;
    }
}
