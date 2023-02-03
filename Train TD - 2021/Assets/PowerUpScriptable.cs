using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PowerUpScriptable : ScriptableObject {
	public Sprite icon;
	[Multiline(10)]
	public string description;
}
