using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TooltipScriptable : ScriptableObject {
	public Tooltip myTooltip;
}

[Serializable]
public class Tooltip {
    
	[Multiline(10)]
	public string text;
}
