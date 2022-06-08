using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPitchAtStart : MonoBehaviour {

    public float pitchRange = 0.2f;

    void Start() {
        GetComponent<AudioSource>().pitch = GetComponent<AudioSource>().pitch + Random.Range(-pitchRange, pitchRange);
    }

}
