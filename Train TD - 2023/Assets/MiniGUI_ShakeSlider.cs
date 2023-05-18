using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_ShakeSlider : MonoBehaviour, IInitRequired
{
    const string exposedName = "screenshake";
    public Slider mySlider;
    
    public void Initialize() {
        var audVal = PlayerPrefs.GetFloat(exposedName, 1.0f);
        mySlider.value = audVal;
        SetVol(audVal);
    }

    public void OnSliderUpdated() {
        PlayerPrefs.SetFloat(exposedName, mySlider.value);
        SetVol(mySlider.value);
    }

    void SetVol(float sliderVal) {
        CameraShakeController.s.overallShakeAmount = sliderVal;
    }
}
