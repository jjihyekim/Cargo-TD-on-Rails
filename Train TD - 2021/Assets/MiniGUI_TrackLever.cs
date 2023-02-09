using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGUI_TrackLever : MonoBehaviour {
    public GameObject lever;
    public GameObject upTrack;
    public GameObject bottomTrack;

    public int leverId;

    public bool currentState;
    public void SetTrackState(bool state) {
        currentState = state;
        if (currentState) {
            upTrack.SetActive(true);
            bottomTrack.SetActive(false);
            var scale = Vector3.one;
            scale.x = -1;
            lever.transform.localScale = scale;
        } else {
            upTrack.SetActive(false);
            bottomTrack.SetActive(true);
            var scale = Vector3.one;
            //scale.x = -1;
            lever.transform.localScale = scale;
        }
    }


    public void LeverClicked() {
        PathSelectorController.s.ActivateLever(leverId);
    }
}
