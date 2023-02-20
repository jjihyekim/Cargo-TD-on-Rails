using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Random = UnityEngine.Random;


[CreateAssetMenu()]
public class LevelArchetypeScriptable : ScriptableObject {

    public bool isBossLevel;

    public LevelSegmentScriptable[] possibleLevels;
    public EncounterTitle[] possibleEncounters;
    public EnemyDynamicSpawnData[] possibleDynamicSpawns;

    // encounter chances
    private NumberWithWeights[] encounterChances = new[] { 
        new NumberWithWeights() { number = 0, weight = 0.2f },
        new NumberWithWeights() { number = 1, weight = 0.8f },
        new NumberWithWeights() { number = 2, weight = 0.1f }
    };
    
    private NumberWithWeights[] enemyDirectionsChances = new[] { 
        new NumberWithWeights() { number = 0, weight = 1 }, // all same direction
        new NumberWithWeights() { number = 1, weight = 5 }, // different directions
        new NumberWithWeights() { number = 2, weight = 3 }, // all random
        new NumberWithWeights() { number = 3, weight = 8 }, // one direction one random
    };

    private float allOneSideChance = 0.5f;
    private int segmentCount = 4;

    public ConstructedLevel GenerateLevel() {
        var level = new ConstructedLevel();

        level.mySegmentsA = new LevelSegment[segmentCount];
        level.mySegmentsB = new LevelSegment[segmentCount];

        for (int i = 0; i < segmentCount; i++) {
            var segmentType = NumberWithWeights.WeightedRandomRoll(enemyDirectionsChances);
            
            switch (segmentType) {
                case 0: {
                    int direction = Random.value > 0.5f ? 1 : 2;
                    level.mySegmentsA[i] = GenerateSegment(direction);
                    level.mySegmentsB[i] = GenerateSegment(direction);
                    break;
                }
                case 1: {
                    bool isLeft = Random.value > 0.5f;
                    level.mySegmentsA[i] = GenerateSegment(isLeft ? 2 : 3);
                    level.mySegmentsB[i] = GenerateSegment(!isLeft ? 2 : 3);
                    break;
                }
                case 2: {
                    level.mySegmentsA[i] = GenerateSegment(0);
                    level.mySegmentsB[i] = GenerateSegment(0);
                    break;
                }
                case 3: {
                    bool isTop = Random.value > 0.5f;
                    bool isLeft = Random.value > 0.5f;
                    int top = isLeft ? 2 : 3;
                    int bottom = isLeft ? 2 : 3;
                    if (isTop) top = 0;
                    
                    level.mySegmentsA[i] = GenerateSegment(top);
                    level.mySegmentsB[i] = GenerateSegment(bottom);
                    break;
                }
            }
            
        }

        level.dynamicSpawnData = possibleDynamicSpawns[Random.Range(0, possibleDynamicSpawns.Length)];

        if (possibleEncounters.Length > 0) {
            var encounterCount = NumberWithWeights.WeightedRandomRoll(encounterChances);

            for (int i = 0; i < encounterCount; i++) {
                var mySegment = Random.Range(1, segmentCount); // first one is never an encounter

                var encounter = "e_" + possibleEncounters[Random.Range(0, possibleEncounters.Length)].gameObject.name;
                if (Random.value > 0.5f) {
                    level.mySegmentsA[mySegment] = new LevelSegment() { isEncounter = true, levelName = encounter };
                } else {
                    level.mySegmentsB[mySegment] = new LevelSegment() { isEncounter = true, levelName = encounter };
                }
            }
        }

        level.levelName = name + " level " + Random.Range(100,1000);
        level.levelNiceName = name;
        return level.Copy();
    }


    LevelSegment GenerateSegment(int type) { // 0 -> random 1 -> all left 2 -> all right 
        var segment =  possibleLevels[Random.Range(0, possibleLevels.Length)].GetData().Copy();
        
        var enemyOffset = segment.enemiesOnPath[0].distanceOnPath;

        if (enemyOffset < 50) {
            enemyOffset = 50 - enemyOffset;
        }

        for (int i = 0; i < segment.enemiesOnPath.Length; i++) {
            segment.enemiesOnPath[i].distanceOnPath += enemyOffset;
        }

        if (type == 0) {
            for (int i = 0; i < segment.enemiesOnPath.Length; i++) {
                segment.enemiesOnPath[i].isLeft = Random.value > 0.5f;
            }
            
        } else {
            var isLeft = type == 1;

            for (int i = 0; i < segment.enemiesOnPath.Length; i++) {
                segment.enemiesOnPath[i].isLeft = isLeft;
            }
        }
        
        var furthestEnemyDistance = 0;
        for (int i = 0; i < segment.enemiesOnPath.Length; i++) {
            furthestEnemyDistance = Mathf.Max(furthestEnemyDistance, segment.enemiesOnPath[i].distanceOnPath);
        }

        

        if (segment.rewardPowerUpAtTheEnd) {
            if (furthestEnemyDistance + 100 > segment.segmentLength) {
                segment.segmentLength = furthestEnemyDistance + 100;
            }
            
            var enemiesOnPath = new List<EnemyOnPathData>(segment.enemiesOnPath);
            enemiesOnPath.Add(new EnemyOnPathData() {
                distanceOnPath = segment.segmentLength-50,
                enemyIdentifier =  LevelReferences.s.powerUpSpawnerEnemy,
                isLeft = segment.enemiesOnPath[segment.enemiesOnPath.Length-1].isLeft
            });

            segment.enemiesOnPath = enemiesOnPath.ToArray();

            segment.powerUpRewardUniqueName = DataHolder.s.powerUps[Random.Range(0, DataHolder.s.powerUps.Length)].name;

        } else {
            if (furthestEnemyDistance + 50 > segment.segmentLength) {
                segment.segmentLength = furthestEnemyDistance + 50;
            }
        }
        
        segment.segmentLength += segment.segmentLength % HexGrid.s.gridSize.x;
        //segment.segmentLength -= HexGrid.s.gridSize.x / 2;
        
        return segment;
    }
}


[Serializable]
public class ConstructedLevel {
    [HideInInspector]
    public string levelName = "unset";

    [HideInInspector] public string levelNiceName;
    
    // for the first segment Segment A will always be chosen, but for the remaining bits the player can choose which one to go to.
    public LevelSegment[] mySegmentsA; 
    public LevelSegment[] mySegmentsB;

    public EnemyDynamicSpawnData dynamicSpawnData;

    public bool isBossLevel;

    public bool isRealLevel() {
        return levelName != "unset";
    }

    public ConstructedLevel Copy() {
        var serialized = SerializationUtility.SerializeValue(this, DataFormat.Binary);
        return SerializationUtility.DeserializeValue<ConstructedLevel>(serialized, DataFormat.Binary);
    }
}


