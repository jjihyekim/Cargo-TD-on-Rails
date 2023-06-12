using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_ChangeImageTint : MonoBehaviour
{
    
    public Color deactiveTint = Color.grey;


    private Color defaultColor;
    private Color deactiveColor;

    public bool isSetUp = false;

    private void SetUp() {
        defaultColor = GetComponent<Image>().color;
        deactiveColor = defaultColor * deactiveTint;
        isSetUp = true;
    }

    [Button]
    public void SetDeactive(bool isActive) {
        if (!isSetUp) {
            SetUp();
        }
        
        if (isActive) {
            GetComponent<Image>().color = defaultColor;
        } else {
            GetComponent<Image>().color = deactiveColor;
        }
    }
}
