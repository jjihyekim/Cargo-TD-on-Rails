using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGUI_PursuerTimer : MonoBehaviour
{

    public EnemyDynamicSpawnData myDynamicSpawnData;

    public Image mainImage;
    public Image gunImage;
    public TMP_Text countText;
    public TMP_Text timerText;
    
    public void SetUp(EnemyDynamicSpawnData _dynamicSpawnData) {
        myDynamicSpawnData = _dynamicSpawnData;
        var enemyWave = DataHolder.s.GetEnemy(myDynamicSpawnData.enemyIdentifier.enemyUniqueName).GetComponent<EnemySwarmMaker>();

        mainImage.sprite = enemyWave.enemyIcon;
        var gunSprite = enemyWave.GetGunSprite();
        if (gunSprite != null) {
            gunImage.sprite = gunSprite;
        } else {
            gunImage.enabled = false;
        }
    }

    private void Update() {
        countText.text = (myDynamicSpawnData.enemyIdentifier.enemyCount != -1) ? "x" + myDynamicSpawnData.enemyIdentifier.enemyCount : "x1";
        timerText.text = $"{(myDynamicSpawnData.curTime):F1}s";
    }

}
