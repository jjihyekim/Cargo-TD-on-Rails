using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Random = UnityEngine.Random;


[CreateAssetMenu()]
public class LevelArchetypeScriptable : ScriptableObject {

    public LevelSegmentScriptable[] possibleLevels;
    public EncounterTitle[] possibleEncounters;
    public EnemyDynamicSpawnData[] possibleDynamicSpawns;

    // encounter chances
    private NumberWithWeights[] encounterChances = new[] { 
        new NumberWithWeights() { number = 0, weight = 0.2f },
        new NumberWithWeights() { number = 1, weight = 0.8f },
        new NumberWithWeights() { number = 2, weight = 0.1f }
    };

    private float allOneSideChance = 0.5f;
    private int segmentCount = 4;

    public ConstructedLevel GenerateLevel() {
        var level = new ConstructedLevel();

        for (int i = 0; i < segmentCount; i++) {
            level.mySegmentsA[i] = GenerateSegment();
            level.mySegmentsB[i] = GenerateSegment();
        }

        level.dynamicSpawnData = possibleDynamicSpawns[Random.Range(0, possibleDynamicSpawns.Length)];
        
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

        level.levelName = name + " level " + Random.Range(100,1000);
        return level.Copy();
    }


    LevelSegment GenerateSegment() { 
        var segment =  possibleLevels[Random.Range(0, possibleLevels.Length)].GetData().Copy();

        var isDirectionOneSide = Random.value > allOneSideChance;

        if (isDirectionOneSide) {
            var direction = Random.value > 0.5f;

            for (int i = 0; i < segment.enemiesOnPath.Length; i++) {
                segment.enemiesOnPath[i].isLeft = direction;
            }
        } else {
            for (int i = 0; i < segment.enemiesOnPath.Length; i++) {
                segment.enemiesOnPath[i].isLeft = Random.value > 0.5f;
            }
        }

        return segment;
    }
}


[Serializable]
public class ConstructedLevel {
    [HideInInspector]
    public string levelName = "unset";
    
    // for the first segment Segment A will always be chosen, but for the remaining bits the player can choose which one to go to.
    public LevelSegment[] mySegmentsA; 
    public LevelSegment[] mySegmentsB;

    public EnemyDynamicSpawnData dynamicSpawnData;

    public bool isRealLevel() {
        return levelName != "unset";
    }

    public ConstructedLevel Copy() {
        var serialized = SerializationUtility.SerializeValue(this, DataFormat.Binary);
        return SerializationUtility.DeserializeValue<ConstructedLevel>(serialized, DataFormat.Binary);
    }
}


