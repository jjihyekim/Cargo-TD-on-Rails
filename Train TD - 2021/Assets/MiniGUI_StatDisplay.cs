using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_StatDisplay : MonoBehaviour
{
    public TMP_Text statName;
    public TMP_Text statValue;
    
    public void SetUp (string dataName, string dataValue) {
        statName.text = dataName;
        statValue.text = dataValue;
    }
}
