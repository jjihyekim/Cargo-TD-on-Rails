using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu()]
public class LevelDataScriptable : ScriptableObject {
	[SerializeField]
	private LevelData myData;

	public LevelData GetData() {
		myData.levelName = name;
		return myData;
	}
}