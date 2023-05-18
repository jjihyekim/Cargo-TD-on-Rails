using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_SliderSnapToValue : MonoBehaviour {

    public float snapInterval = 0.25f;
    public float snapDistance = 0.05f;
    private void Start() {
        var slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(float arg0) {
        var slider = GetComponent<Slider>();
        float value = slider.value;
        float interval = snapInterval; //any interval you want to round to
        float stickyDistance = snapDistance;
        value = Mathf.Round(value / interval) * interval;
        
        if(Mathf.Abs(value - slider.value) < stickyDistance)
            slider.value = value;
    }
}
