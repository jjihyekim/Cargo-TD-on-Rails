using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_SliderValueToText : MonoBehaviour {
    public TMP_Text text;

    public string[] values;

    private Slider _slider;
    private void Start() {
        _slider = GetComponent<Slider>();
    }

    private void Update() {
        text.text = values[(int)_slider.value];
    }
}
