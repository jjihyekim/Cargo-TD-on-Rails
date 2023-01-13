using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveTogglePreference : MonoBehaviour {
    private Toggle _toggle;
    // Start is called before the first frame update
    void Start() {
        _toggle = GetComponent<Toggle>();
        var curValue = _toggle.isOn ? 1 : 0;
        
        var toggleStatus = PlayerPrefs.GetInt(gameObject.name, curValue);
        _toggle.isOn = toggleStatus == 1;
    }

    private void OnDisable() {
        PlayerPrefs.SetInt(gameObject.name, _toggle.isOn ? 1 : 0);
    }
}
