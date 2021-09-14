using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuggySwarm : MonoBehaviour, IData
{
    public GameObject buggyPrefab;
    
    public SpreadAndCount[] spreadAndCounts = new[] {
        new SpreadAndCount() { count = 3, spread = 0.5f },
        new SpreadAndCount() { count = 6, spread = 1f },
        new SpreadAndCount() { count = 9, spread = 1.5f },
    };

    public void SetData(float data) {
        var totalCount = Mathf.RoundToInt(data);

        if (totalCount == 1) {
            var buggy = Instantiate(buggyPrefab, transform);
            buggy.transform.localPosition = Vector3.zero;
            
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

                    var buggy = Instantiate(buggyPrefab, transform);
                    buggy.transform.localPosition = pos * spread;
                }

                n++;
            }

        }
    }
}

[Serializable]
public class SpreadAndCount {
    public int count = 3;
    public float spread = 0.5f;
}