using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_MusicPlayer : MonoBehaviour {
    public TMP_Text musicText;

    private void Start() {
        if (MusicPlayer.s == null)
            this.enabled = false;
    }

    // Update is called once per frame
    void Update() {
        musicText.text = MusicPlayer.s.trackNameAndTime;
    }


    public void NextTrack() {
        MusicPlayer.s.PlayNextTrack();
    }

    public void PrevTrack() {
        MusicPlayer.s.PlayPrevTrack();
    }
}
