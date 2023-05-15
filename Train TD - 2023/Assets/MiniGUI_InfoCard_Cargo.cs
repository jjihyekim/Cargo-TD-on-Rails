using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class MiniGUI_InfoCard_Cargo : MonoBehaviour, IBuildingInfoCard {

    public TMP_Text cargoRewardName;
    public Image cargoRewardIcon;
    

    [ReadOnly] public CargoModule cargoModule;
    
    public void SetUp(Cart building) {
        cargoModule = building.GetComponent<CargoModule>();
        
        if (cargoModule == null) {
            gameObject.SetActive(false);
            return;
        } else {
            gameObject.SetActive(true);
        }

        cargoRewardName.text = cargoModule.GetReward();
        cargoRewardIcon.sprite = cargoModule.GetRewardIcon();
    }
}
