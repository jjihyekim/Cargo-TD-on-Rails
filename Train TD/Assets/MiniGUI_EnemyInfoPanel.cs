using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_EnemyInfoPanel : MonoBehaviour {

    public TMP_Text enemyType;
    public TMP_Text enemyDistance;
    public TMP_Text enemySide;

    public void SetUp (EnemyWaveData data) {
        enemyType.text = data.enemyUniqueName + ((int)data.enemyData);
        enemyDistance.text = data.startDistance.ToString();
        enemySide.text = data.isLeft ? "Left" : "Right";
    }
}
