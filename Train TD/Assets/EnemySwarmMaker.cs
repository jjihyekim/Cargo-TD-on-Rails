using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnemySwarmMaker : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Sprite enemyIcon;
    
    // 3 speed ~= regular speed
    // 0.25 speed ~= min train sped
    // 10 speed ~= max speed
    public float speed = 5;
    
    public SpreadAndCount[] spreadAndCounts = new[] {
        new SpreadAndCount() { count = 3, spread = 0.5f },
        new SpreadAndCount() { count = 6, spread = 1f },
        new SpreadAndCount() { count = 9, spread = 1.5f },
    };

    public int activeEnemies;
    
    public AudioClip[] enemyEnterSounds;
    public AudioClip[] enemyDieSounds;

    public void SetData(float data) {
        var totalCount = Mathf.RoundToInt(data);

        if (totalCount == 1) {
            var buggy = Instantiate(enemyPrefab, transform);
            buggy.transform.localPosition = Vector3.zero;
            activeEnemies += 1;

        } else {
            int n = 0;
            while (totalCount > 0) {
                var count = spreadAndCounts[n].count;
                count = Mathf.Min(count, totalCount);
                totalCount -= count;

                var spread = spreadAndCounts[n].spread;

                var radians = Mathf.Deg2Rad * 360 / count;
                for (int i = 0; i < count; i++) {
                    var pos = new Vector3(Mathf.Sin(radians * i), 0, Mathf.Cos(radians * i));

                    var buggy = Instantiate(enemyPrefab, transform);
                    buggy.transform.localPosition = pos * spread;
                    activeEnemies += 1;
                }

                n++;
            }
        }
    }

    public void EnemyDeath() {
        activeEnemies -= 1;
        
        if(!EnemyHealth.winSelfDestruct)
            SoundscapeController.s.PlayEnemyDie(enemyDieSounds[Random.Range(0,enemyDieSounds.Length)]);

        if (activeEnemies == 0) {
            Destroy(GetComponentInParent<EnemyWave>().gameObject);
            Destroy(gameObject);
        }
    }
    
    public void PlayEnemyEnterSound() {
        SoundscapeController.s.PlayEnemyEnter(enemyEnterSounds[Random.Range(0,enemyEnterSounds.Length)]);
    }
}

[Serializable]
public class SpreadAndCount {
    public int count = 3;
    public float spread = 0.5f;
}