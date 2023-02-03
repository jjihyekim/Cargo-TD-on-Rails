using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReturnAllCargoButton : MonoBehaviour
{
    
    
    private Button _button;
    private CargoModule[] _cargoModules;
    void Start() {
        _button = GetComponent<Button>();
        GetModules();
        Update();
        Train.s.trainUpdated.AddListener(GetModules);
    }

    private void OnDestroy() {
        Train.s.trainUpdated.RemoveListener(GetModules);
    }

    public void GetModules() {
        _cargoModules = Train.s.GetComponentsInChildren<CargoModule>();
    }

    void Update() {
        _button.interactable = _cargoModules.Length > 0;
    }


    public void DoAction() {
        for (int i = 0; i < _cargoModules.Length; i++) {
            _cargoModules[i].CargoReturned();
        }
    }
}
