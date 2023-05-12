using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MiniGUI_SmartToggleSlider : MonoBehaviour {
    private Slider Slider;
    void Start() {
        Slider = GetComponent<Slider>();
    }

    public void OnClick() {
        Slider.value = Slider.value > 0 ? 0 : 1;
    }
}
