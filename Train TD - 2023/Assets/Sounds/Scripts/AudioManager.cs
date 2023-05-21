using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    // singleton class
    public static AudioManager instance { get; private set; }

    [Header("Music Mixer")]
    public Bus musicBus;
    [Range(-80f, 10f)] public float musicBusVolume;

    private void Awake()
    {
        // setup singleton object
        if (instance != null)
        {
            Debug.LogError("Audio Manager should be a singleton class, but multiple instances are found!");
        }
        instance = this;
    }

    private void Start()
    {
        musicBus = RuntimeManager.GetBus("bus:/Music");
    }

    /// <summary>
    /// Play one shot sound, mostly for sound effects that are played instantly and once. E.g., gun fire sound.
    /// </summary>
    /// <param name="sound">The audio, a type of FMod's EventReference</param>
    /// <param name="worldPos">The world position the sound is played from</param>
    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    private void Update()
    {
        UpdateBus();
    }

    private void UpdateBus()
    {
        musicBus.setVolume(musicBusVolume);
    }
    
    private float DecibleToLinear(float db)
    {
        return Mathf.Pow(10f, db / 20f);
    }

    public static EventInstance CreateFmodEventInstance(EventReference eventRef)
    {
        EventInstance eventInst = RuntimeManager.CreateInstance(eventRef);
        // Debug.Log(eventRef.ToString() + " is instantitated");
        return eventInst;
    }
}
