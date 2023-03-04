using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoGenerateBalanceSettings : MonoBehaviour {

    public GameObject prefab;
    // Start is called before the first frame update
    void Start()
    {
        var fields = typeof(Tweakables).GetFields();
        for (int i = 0; i < fields.Length; i++) {
            var fieldObject = Instantiate(prefab, transform);
            fieldObject.GetComponent<MiniGUI_TweakableChanger>().targetField = fields[i].Name;
        }
    }

}
