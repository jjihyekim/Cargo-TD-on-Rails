using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomPrefabAtStart : MonoBehaviour {
    public RandomPrefab[] myPrefabs;


    public Vector2Int spawnCount = new Vector2Int(0, 3);
    public Vector2 spawnScale = new Vector2(0.05f, 0.15f);

    public float spawnSpread;

    private const float spawnXLimit = 3.5f;

    private void Start() {
        //if (Math.Abs(transform.position.x) > spawnXLimit) {
            var count = Random.Range(spawnCount.x, spawnCount.y + 1);
            for (int i = 0; i < count; i++) {
                SpawnPrefab();
            }
        //}
    }




    void SpawnPrefab() {
        var index = GetRandomWeightedIndex(myPrefabs);

        var spawn = Instantiate(myPrefabs[index], transform);

        var loc = Random.insideUnitCircle * spawnSpread;
        spawn.transform.localPosition = new Vector3(loc.x, 0, loc.y);
        spawn.transform.localScale = Vector3.one * Random.Range(spawnScale.x, spawnScale.y);
        spawn.transform.Rotate(new Vector3(0,1,0) * Random.Range(0, 360));
    }
    
    
    int GetRandomWeightedIndex(RandomPrefab[] prefabs)
    {
        // Get the total sum of all the weights.
        float weightSum = 0f;
        for (int i = 0; i < prefabs.Length; ++i)
        {
            weightSum += GetEnemySpawnChanceWeight(prefabs[i]);
        }
 
        // Step through all the possibilities, one by one, checking to see if each one is selected.
        int index = 0;
        int lastIndex = prefabs.Length - 1;
        var chance = Random.Range(0, weightSum);
        while (index < lastIndex)
        {
            // Do a probability check with a likelihood of weights[index] / weightSum.
            if (chance < GetEnemySpawnChanceWeight(prefabs[index]))
            {
                return index;
            }
 
            // Remove the last item from the sum of total untested weights and try again.
            chance -= GetEnemySpawnChanceWeight(prefabs[index]);
            index++;
        }
 
        // No other item was selected, so return very last index.
        return index;
    }
    
    
    float GetEnemySpawnChanceWeight(RandomPrefab prefab) {
        return  prefab.spawnChance;
    }
}
