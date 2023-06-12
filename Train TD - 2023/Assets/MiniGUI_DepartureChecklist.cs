using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_DepartureChecklist : MonoBehaviour {
    public Toggle[] myToggles;

    public GameObject statusChangedEffect;

    public void UpdateStatus(bool[] status) {
        for (int i = 0; i < myToggles.Length; i++) {
            if (myToggles[i].isOn != status[i]) {
                myToggles[i].isOn = status[i];
                Instantiate(statusChangedEffect, myToggles[i].transform);
            }
        }

        if (myToggles.Length != status.Length) {
            Debug.LogError("Wrong number of status is fed to the departure checklist");
        }
    }
}
