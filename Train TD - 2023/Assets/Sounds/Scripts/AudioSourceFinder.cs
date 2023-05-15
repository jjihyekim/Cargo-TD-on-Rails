using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceFinder : MonoBehaviour
{
    // a tool script that help finds Unity's built in audio source and listenres. Help to transoform the project to FMod
    public bool findAllAudioSource, findAllListeners;

    public AudioSource[] sources;
    public AudioListener[] listeners;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (findAllAudioSource)
        {
            sources = FindObjectsOfType<AudioSource>();
            findAllAudioSource = false;
        }

        if (findAllListeners)
        {
            listeners = FindObjectsOfType<AudioListener>();
            findAllListeners = false;
        }
    }
}
