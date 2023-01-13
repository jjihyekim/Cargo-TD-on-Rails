using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableCargoRewardInfo : MonoBehaviour, IClickableInfo {
    private CargoModule myModule;
    
    private void Start() {
        myModule = GetComponent<CargoModule>();
    }

    public string GetInfo() {
        return $"reward: +{myModule.GetReward()}";
    }

    public Tooltip GetTooltip() {
        return new Tooltip() { text = $"Bring this cargo safely to the next city to earn {myModule.GetReward()} money" };;
    }

}
