using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PowerUpScriptable : ScriptableObject {
	public Sprite icon;
	[Multiline(10)]
	public string description;
	public Color color = Color.magenta;

	public enum PowerUpType {
		boost, massHeal, massReload, buildGun
	}

	public PowerUpType myType;
}
