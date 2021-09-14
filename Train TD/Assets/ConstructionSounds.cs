using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionSounds : MonoBehaviour {
    public AudioClip[] possibleSounds;

    public void PlayConstructionSound() {
        var aud = GetComponent<AudioSource>();
        aud.clip = possibleSounds[Random.Range(0, possibleSounds.Length)];
        aud.pitch = Random.Range(0.8f, 1f);
        aud.Play();
    }
}
