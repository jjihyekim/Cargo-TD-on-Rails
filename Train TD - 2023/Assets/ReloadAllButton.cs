using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ReloadAllButton : MonoBehaviour
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
            if (_reloadActions[i]!=null && _reloadActions[i].gameObject != null && _reloadActions[i].myType == ResourceTypes.ammo) {
                _reloadActions[i].Start();
                _reloadActions[i].Update();
                _reloadActions[i].Update();
            }
            // we want these to update their cost
        }
    }

    private bool isFree = true;

    void Update() {
        var cost = 0;

        for (int i = 0; i < _reloadActions.Length; i++) {
            if (_reloadActions[i].myType == ResourceTypes.ammo)
                //cost += (int)_reloadActions[i].costWithoutAffordability;
                cost += _reloadActions[i].cost;
        }


        var canAfford = MoneyController.s.HasResource(ResourceTypes.ammo, cost) || isFree;
        _button.interactable = cost > 0 && canAfford;
        costText.color = canAfford ? regularColor : cantAffordColor;

        if (canAfford) {
            costText.text = isFree ? "Free" : cost.ToString();
        } else {
            costText.text = isFree ? "Full" : cost.ToString();
        }
    }


    public void DoRefuel() {
        for (int i = 0; i < _reloadActions.Length; i++) {
            if(_reloadActions[i].myType == ResourceTypes.ammo)
                _reloadActions[i].EngageAction();
        }
    }
}

