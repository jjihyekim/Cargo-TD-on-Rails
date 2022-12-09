using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugInfiniteMoneySpawner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(SpawnMoney),0.5f);
    }

    void SpawnMoney() {
        LevelReferences.s.SpawnResourceAtLocation(ResourceTypes.scraps, 5, transform.position);
        Invoke(nameof(SpawnMoney),0.5f);
    }
}
