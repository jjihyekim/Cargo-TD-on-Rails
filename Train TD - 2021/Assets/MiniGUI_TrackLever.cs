using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_TrackLever : MonoBehaviour {
    public GameObject lever;
    public GameObject upTrack;
    public GameObject bottomTrack;
    public GameObject button;

    public int leverId;

    public bool currentState;

    public bool switchByColor = false;
    public Color disabledColor = Color.white;

    public bool isVisible = true;
    public void SetTrackState(bool state) {
        currentState = state;
        
        if(!isVisible)
            return;
        
        if (currentState) {
            if (switchByColor) {
                upTrack.SetActive(true);
                bottomTrack.SetActive(true);
                
                upTrack.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                bottomTrack.transform.GetChild(0).GetComponent<Image>().color = disabledColor;
            } else {
                upTrack.SetActive(false);
                bottomTrack.SetActive(true);
            }

            var scale = Vector3.one;
            scale.x = -1;
            lever.transform.localScale = scale;
        } else {
            if (switchByColor) {
                upTrack.SetActive(true);
                bottomTrack.SetActive(true);
                
                upTrack.transform.GetChild(0).GetComponent<Image>().color = disabledColor;
                bottomTrack.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            } else {
                upTrack.SetActive(true);
                bottomTrack.SetActive(false);
            }

            var scale = Vector3.one;
            //scale.x = -1;
            lever.transform.localScale = scale;
        }
    }


    public void SetVisibility(bool state) {
        isVisible = state;

        if (isVisible) {
            button.SetActive(true);
            SetTrackState(currentState);
        } else {
            button.SetActive(false);
            upTrack.SetActive(false);
            bottomTrack.SetActive(false);
        }
    }

    public void LeverClicked() {
        PathSelectorController.s.ActivateLever(leverId);
    }

    public void LockTrackState() {
        button.GetComponent<Button>().interactable = false;
    }
}
