using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MiniGUI_ActionButton : MonoBehaviour {
    private Slider _slider;
    public Image bgImage;
    void Start() {
        _slider = GetComponentInChildren<Slider>();
    }

    [NonSerialized] 
    public UnityEvent OnButtonPressed = new UnityEvent();

    [NonSerialized] 
    public UnityEvent<bool> OnToggleValueChanged = new UnityEvent<bool>();
    
    public void ToggleChanged() {
        OnToggleValueChanged?.Invoke(_slider.value > 0.5f);
    }

    public void ButtonPressed() {
        OnButtonPressed?.Invoke();
    }

    public void OverrideSliderValue(bool value) {
        _slider = GetComponentInChildren<Slider>();
        _slider.value = value ? 1 : 0;
    }

    public void SetBgColor(Color color) {
        bgImage.color = color;
    }
}

