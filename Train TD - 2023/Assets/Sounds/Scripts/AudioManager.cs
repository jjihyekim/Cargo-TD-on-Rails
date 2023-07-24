using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Sirenix.OdinInspector;
using UnityEngine.Audio;


public class AudioManager : MonoBehaviour
{
    // singleton class
    public static AudioManager instance { get; private set; }

    #region Unity Mixer
    [FoldoutGroup("Unity Mixer")]
    public AudioMixer unityMixer;

    private void UpdateUnityMixer()
    {
        unityMixer.GetFloat("EnemyEngineVol", out float enemyEngineVol);
        unityMixer.SetFloat("EnemyEngineVol", Mathf.Lerp(enemyEngineVol, TimeController.s.isPaused ? -80 : 0, Time.unscaledDeltaTime * 20f));
    }
    #endregion

    #region FMOD Mixer
    [FoldoutGroup("FMOD Mixer")]
    public Bus masterBus, musicBus;

    [FoldoutGroup("FMOD Mixer")]
    [PropertyRange(-80f, 10f)] public float masterBusVolume, musicBusVolume;
    private void UpdateBus()
    {
        masterBus.setVolume(masterBusVolume);
        musicBus.setVolume(musicBusVolume);
    }
    #endregion


    #region FMOD functions
    [FoldoutGroup("FMOD Functions")]
    [Required]
    [ShowInInspector]
    public static SfxDictionary sfxDict
    {
        get
        {
            if (_sfxDict == null)
                _sfxDict = Resources.Load("FMOD/SfxDictionary") as SfxDictionary;
            return _sfxDict;
        }
    }
    private static SfxDictionary _sfxDict;
    public static void PlayOneShot(SfxTypes sfxType)
    {
        if (sfxDict != null)
            PlayOneShot(sfxDict.sfxDict[sfxType]);
    }
    /// <summary>
    /// Play one shot sound, mostly for sound effects that are played instantly and once. E.g., gun fire sound.
    /// </summary>
    /// <param name="sound">The audio, a type of FMod's EventReference</param>
    /// <param name="worldPos">The world position the sound is played from</param>
    public static void PlayOneShot(EventReference sound, Vector3 worldPos=default)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public static EventInstance CreateFmodEventInstance(EventReference eventRef)
    {
        EventInstance eventInst = RuntimeManager.CreateInstance(eventRef);
        // Debug.Log(eventRef.ToString() + " is instantitated");
        return eventInst;
    }
    #endregion

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
        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
    }


    private void Update()
    {
        UpdateBus();

        UpdateUnityMixer();
    }

    
    private float DecibleToLinear(float db)
    {
        return Mathf.Pow(10f, db / 20f);
    }

}
