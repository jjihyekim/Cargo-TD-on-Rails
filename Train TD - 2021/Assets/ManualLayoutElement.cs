using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ManualLayoutElement : MonoBehaviour {
    public bool isMinWidthMode = true;
    
    [ShowIf("isMinWidthMode")]
    public float minWidth;
    [HideIf("isMinWidthMode")]
    public float preferredWidth;
}
