using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_SliderValueToText : MonoBehaviour {
    public TMP_Text text;

    [HideIf("basicPercentage")]
    public string[] values;

    private Slider _slider;

    public bool basicPercentage = false;
    private void Start() {
        _slider = GetComponent<Slider>();
    }

    private void Update() {
        if (basicPercentage) {
            text.text = $"{_slider.value:F2}";
        } else {
            text.text = values[(int)_slider.value];
        }
    }
}
