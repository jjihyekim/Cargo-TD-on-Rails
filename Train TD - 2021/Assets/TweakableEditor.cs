using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;

public class TweakableEditor : MonoBehaviour {
	
	public TMP_InputField inputField;

	public TMP_Text status;
	private void Start() {
		var data = DataHolder.s.GetTweaks();
		var serialized = Encoding.ASCII.GetString(SerializationUtility.SerializeValue(data, DataFormat.JSON));
		inputField.text = serialized;
		status.text = "data initialized";
	}

	public void ResetData() {
		status.text = "data reset";
		Invoke(nameof(ClearStatus),1f);
	}
	
	public void Apply() {
		try {
			var bytes = Encoding.ASCII.GetBytes(inputField.text);
			TweakablesParent data = SerializationUtility.DeserializeValue<TweakablesParent>( bytes, DataFormat.JSON);
			DataHolder.s.OverrideData(data);
			
			status.text = "data applied";
		} catch (Exception e) {
			
			status.text = $"Error: {e} {e.StackTrace}";
		}
		
		Invoke(nameof(ClearStatus),1f);
	}

	void ClearStatus() {
		status.text = "---";
	}

	void Reload() {
		SceneLoader.s.BackToStarterMenu();
	}
}
