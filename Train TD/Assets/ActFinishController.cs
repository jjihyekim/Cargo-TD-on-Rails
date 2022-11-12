using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActFinishController : MonoBehaviour {
    public static ActFinishController s;

    private void Awake() {
        s = this;
    }

    public GameObject actWinUI;

    public void OpenActWinUI() {
        actWinUI.SetActive(true);
    }
}
