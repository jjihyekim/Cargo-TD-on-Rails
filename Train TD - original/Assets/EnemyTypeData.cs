using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTypeData : MonoBehaviour
{
    public enum EnemyType {
        Deadly, Safe
    }

    public EnemyType myType;
}
