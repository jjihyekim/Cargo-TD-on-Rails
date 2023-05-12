using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class MiniGUI_TweakableChanger : MonoBehaviour
{
    //[ValueDropdown("GetAllVariableNames")]
    public string targetField = "unset";
	
    /*private static IEnumerable GetAllVariableNames() {
        var fields = typeof(Tweakables).GetFields();
        var fieldNames = new List<string>();
        for (int i = 0; i < fields.Length; i++) {
            fieldNames.Add(fields[i].Name);
        }
        return fieldNames;
    }*/


    public TMP_Text varNameText;
    public TMP_InputField inputField;


    public void Start() {
        varNameText.text = targetField;
        ResetInputFieldValue();
    }


    public void InputFieldSet(string value) {
        var fieldInfo = typeof(Tweakables).GetField(targetField);
        try {
            var convertedValue = Convert.ChangeType(value, fieldInfo.FieldType);
            if (convertedValue != null) {
                fieldInfo.SetValue(TweakablesMaster.s.myTweakables, convertedValue);
                TweakablesMaster.s.ApplyTweakableChange();
                inputField.text = "Value Set!";
                StartCoroutine(ResetCoroutine(0.5f));
            } else  {
                inputField.text = "Value is null!";
                StartCoroutine(ResetCoroutine(0.5f));
            }
        } catch (Exception e){
            inputField.text = e.Message;
            StartCoroutine(ResetCoroutine(0.5f));
        }
        
        
    }

    void ResetInputFieldValue() {
        inputField.text = typeof(Tweakables).GetField(targetField).GetValue(TweakablesMaster.s.myTweakables).ToString();
    }

    IEnumerator ResetCoroutine(float delay) {
        yield return new WaitForSecondsRealtime(delay);
        ResetInputFieldValue();
    }
}
