using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


public class HexHeightAffector : MonoBehaviour
{
    [PropertyRange(0,1)]
    public float pinWeight = 1;
    [PropertyRange(0,20)]
    public float pinDistance = 2;
    public AnimationCurve pinDropOff = AnimationCurve.Linear(0,1,1,0);

    public float randomness = 0.2f;

    private void OnDrawGizmos() {
        Gizmos.color = Color.HSVToRGB(0, pinWeight, 1);
        Gizmos.DrawWireSphere(transform.position, pinDistance);
    }
}
