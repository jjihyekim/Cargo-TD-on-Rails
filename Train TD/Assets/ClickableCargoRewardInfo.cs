using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableCargoRewardInfo : MonoBehaviour, IClickableInfo {
    private CargoModule myModule;

    private void Start() {
        myModule = GetComponent<CargoModule>();
    }

    public string GetInfo() {
        return $"reward: +{myModule.moneyReward}";
    }

    public Tooltip GetTooltip() {
        return null;
    }

}
