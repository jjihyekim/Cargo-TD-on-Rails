using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RefuelAllButton : MonoBehaviour
{
    public TMP_Text costText;
    private Button _button;


    private ReloadAction[] _reloadActions;

    public Color regularColor = Color.black;
    public Color cantAffordColor = Color.red;
    void Start() {
        _button = GetComponent<Button>();
        GetReloadModules();
        Update();
        Train.s.trainUpdated.AddListener(GetReloadModules);
    }
    
    private void OnDestroy() {
        Train.s.trainUpdated.RemoveListener(GetReloadModules);
    }

    public void GetReloadModules() {
        _reloadActions = Train.s.GetComponentsInChildren<ReloadAction>();
        for (int i = 0; i < _reloadActions.Length; i++) {
            if (_reloadActions[i]!=null && _reloadActions[i].gameObject != null && _reloadActions[i].myType == ResourceTypes.fuel) {
                _reloadActions[i].Start();
                _reloadActions[i].Update();
                _reloadActions[i].Update();
            }
            // we want these to update their cost
        }
    }

    void Update() {
        var cost = 0;

        for (int i = 0; i < _reloadActions.Length; i++) {
            if(_reloadActions[i].myType == ResourceTypes.fuel)
                cost += _reloadActions[i].cost;
        }

        costText.text = cost.ToString();

        var canAfford = MoneyController.s.HasResource(ResourceTypes.fuel, cost);
        _button.interactable = cost > 0 && canAfford;
        costText.color = canAfford ? regularColor : cantAffordColor;
    }


    public void DoRefuel() {
        for (int i = 0; i < _reloadActions.Length; i++) {
            if(_reloadActions[i].myType == ResourceTypes.fuel)
                _reloadActions[i].EngageAction();
        }
    }
}
