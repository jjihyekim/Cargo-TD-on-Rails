using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RepairAllButton : MonoBehaviour {

    public TMP_Text costText;
    private Button _button;


    private RepairAction[] _repairActions;
    void Start() {
        _button = GetComponent<Button>();
        GetRepairModules();
        Update();
    }

    private void OnEnable() {
        if(Train.s != null)
            Train.s.trainUpdated.AddListener(GetRepairModules);
        else
            Invoke(nameof(DelayedEnable), 0.05f);
    }

    void DelayedEnable() {
        Train.s.trainUpdated.AddListener(GetRepairModules);
    }

    private void OnDisable() {
        Train.s.trainUpdated.RemoveListener(GetRepairModules);
    }


    public void GetRepairModules() {
        _repairActions = Train.s.GetComponentsInChildren<RepairAction>();
        for (int i = 0; i < _repairActions.Length; i++) {
            if (_repairActions[i]!=null && _repairActions[i].gameObject != null) {
                _repairActions[i].Start();
                _repairActions[i].Update();
                _repairActions[i].Update();
            }
            // we want these to update their cost
        }
    }

    public Color regularColor = Color.black;
    public Color cantAffordColor = Color.red;
    void Update() {
        var cost = 0;

        for (int i = 0; i < _repairActions.Length; i++) {
            cost += _repairActions[i].cost;
        }

        costText.text = cost.ToString();

        var canAfford = MoneyController.s.HasResource(ResourceTypes.scraps, cost);
        _button.interactable = cost > 0 && canAfford;
        costText.color = canAfford ? regularColor : cantAffordColor;
    }


    public void DoRepair() {
        for (int i = 0; i < _repairActions.Length; i++) {
            _repairActions[i].EngageAction();
        }
    }
}
