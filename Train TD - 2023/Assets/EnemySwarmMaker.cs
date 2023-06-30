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
    
    
    public bool isTeleporting = false;
    public Vector2 teleportTiming = new Vector2(10, 30);

    public bool isStealing = false;

    public Sprite GetGunSprite() {
        var gunModule = enemyPrefab.GetComponentInChildren<GunModule>();

        if (gunModule != null) {
            return gunModule.gunSprite;
        } else {
            return null;
        }
    }

    public float currentXSpread = 0;
    public float xSpreadAdd = 0;
    public float SetData(float data, Artifact artifact) {
        var totalCount = Mathf.RoundToInt(data);
        currentXSpread = 0;
        var artifactEnemy = Random.Range(0, totalCount);

        if (totalCount == 1) {
            var buggy = Instantiate(enemyPrefab, transform);
            buggy.transform.localPosition = Vector3.zero;
            activeEnemies += 1;
            
            AddArtifactToEnemy(artifact, buggy);
            ArtifactsController.s.ModifyEnemy(buggy.GetComponent<EnemyHealth>());

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
                    var posWithSpread = pos * spread;
                    buggy.transform.localPosition = posWithSpread;
                    currentXSpread = Mathf.Max(currentXSpread, posWithSpread.x);
                    activeEnemies += 1;
                    
                    
                    if (i == artifactEnemy && artifact != null)
                        AddArtifactToEnemy(artifact, buggy);
                    ArtifactsController.s.ModifyEnemy(buggy.GetComponent<EnemyHealth>());
                }

                n++;
            }
        }

        if (artifact != null) {
            
        }

        currentXSpread += xSpreadAdd;

        return currentXSpread;
    }

    void AddArtifactToEnemy(Artifact artifact, GameObject enemy) {
            var artifactCarrier = enemy.GetComponent<EnemyHealth>();
            var hasArtifactDisplayParent = artifactCarrier.GetUITransform();
            var uiStar = Instantiate(LevelReferences.s.enemyHasArtifactStar, LevelReferences.s.uiDisplayParent).GetComponent<UIElementFollowWorldTarget>().SetUp(hasArtifactDisplayParent);
            artifactCarrier.rewardArtifactOnDeath = true;
            artifactCarrier.artifactRewardUniqueName = artifact.uniqueName;
            artifactCarrier.bonusArtifactUIStar = uiStar;
    }

    public void EnemyDeath(bool playDeathSounds = true) {
        activeEnemies -= 1;

        if (playDeathSounds) {
            if(enemyDieSounds.Length > 0)
                SoundscapeController.s.PlayEnemyDie(enemyDieSounds[Random.Range(0,enemyDieSounds.Length)]);
        }

        if (activeEnemies == 0) {
            Destroy(GetComponentInParent<EnemyWave>().gameObject);
            Destroy(gameObject);
        }
    }

    public void PlayEnemyEnterSound() {
        if (enemyEnterSounds.Length > 0) {
            SoundscapeController.s.PlayEnemyEnter(enemyEnterSounds[Random.Range(0, enemyEnterSounds.Length)]);
        }
    }
}

[Serializable]
public class SpreadAndCount {
    public int count = 3;
    public float spread = 0.5f;
}