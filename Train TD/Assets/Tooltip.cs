using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Tooltip : ScriptableObject {
    [TextArea]
    public string text;
}
