using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ConstructionSounds : MonoBehaviour {
    public AudioClip[] possibleSounds;

    public bool playAtStart = false;


    private void Start() {
        if(playAtStart)
            PlayConstructionSound();
    }

    public void PlayConstructionSound() {
        var aud = GetComponent<AudioSource>();
        aud.clip = possibleSounds[Random.Range(0, possibleSounds.Length)];
        aud.pitch *= Random.Range(0.8f, 1f);
        aud.Play();
    }
}
