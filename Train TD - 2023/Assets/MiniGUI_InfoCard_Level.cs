using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGUI_InfoCard_Level : MonoBehaviour, IBuildingInfoCard {
    public GameObject[] levels;
    
    public void SetUp(Cart building) {

        for (int i = 0; i < levels.Length; i++) {
            levels[i].SetActive(building.level == i);
        }
    }

    public void SetUp(EnemyHealth enemy) {
        gameObject.SetActive(false);
    }
}
