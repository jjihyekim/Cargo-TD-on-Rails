using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_SensitivitySlider : MonoBehaviour, IInitRequired
{
    const string mouseName = "sensitivity-mouse";
    const string gamepadName = "sensitivity-gamepad";
    public Slider mySlider;

    public bool isGamepad = false;

    string GetSaveName() {
        if (isGamepad) {
            return gamepadName;
        } else {
            return mouseName;
        }
    }
    
    public void Initialize() {
        var audVal = PlayerPrefs.GetFloat(GetSaveName(), 2.5f);
        mySlider.value = audVal;
        SetVol(audVal);
    }

    public void OnSliderUpdated() {
        PlayerPrefs.SetFloat(GetSaveName(), mySlider.value);
        SetVol(mySlider.value);
    }

    void SetVol(float sliderVal) {
        if (isGamepad) {
            CameraController.s.mouseSensitivity = sliderVal;
        } else {
            CameraController.s.gamepadSensitivity = sliderVal;
        }
    }
}
