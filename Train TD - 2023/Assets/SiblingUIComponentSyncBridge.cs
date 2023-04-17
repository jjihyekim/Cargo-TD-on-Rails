using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiblingUIComponentSyncBridge : MonoBehaviour {
    public GameObject sibling;
    
    private void OnEnable() {
        sibling.SetActive(true);
    }

    private void OnDisable() {
        if(sibling != null)
            sibling.SetActive(false);
    }
}
