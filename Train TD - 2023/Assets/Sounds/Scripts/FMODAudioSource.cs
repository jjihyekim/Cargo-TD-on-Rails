using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using FMOD;
using Sirenix.OdinInspector;

public class FMODAudioSource : MonoBehaviour
{
    [FoldoutGroup("Clip")]
    public EventReference clip;

    [FoldoutGroup("Play settings")]
    public bool pauseOnGamePause = true;

    [FoldoutGroup("Play settings")]
    public bool playOnStart = true;

    [FoldoutGroup("Play settings")]
    public bool instantiateInstanceOnStart = true;

    [FoldoutGroup("Play settings")]
    [Range(0f, 3f)]
    public float volume = 1;

    private EventInstance soundInstance;
    private int pausedPosition;

    private void Start()
    {
        if(!clip.Equals(default(EventReference)) && instantiateInstanceOnStart)
            soundInstance = AudioManager.CreateFmodEventInstance(clip);

        if (playOnStart)
            Play();
    }

    private void Update()
    {
        soundInstance.setVolume(volume);
    }

    private void OnEnable()
    {
        if(pauseOnGamePause)
            TimeController.PausedEvent.AddListener(OnPause);
    }

    private void OnDisable()
    {
        Stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        if(pauseOnGamePause)
            TimeController.PausedEvent.RemoveListener(OnPause);
    }


    #region Play/Stop/Pause Functions
    public int TimelinePosotion()
    {
        soundInstance.getTimelinePosition(out int position);
        return position;
    }

    void OnPause(bool isPaused)
    {
        if (isPaused)
        {
            Pause();
        }
        else
        {
            UnPause();
        }
    }
    public void Pause()
    {
        soundInstance.setPaused(true);
        // Invoke("PauseDelay", 0.5f);
    }
    private void PauseDelay()
    {
        soundInstance.setPaused(true);
    }

    public void UnPause()
    {
        soundInstance.setPaused(false);
    }
    public void TogglePause()
    {
        soundInstance.getPaused(out bool isPaused);
        if (isPaused)
            UnPause();
        else
            Pause();
    }
    public bool IsPaused()
    {
        soundInstance.getPaused(out bool isPaused);
        return isPaused;
    }

    public void Play()
    {
        soundInstance.start();
    }

    public void Stop(FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
    {
        soundInstance.stop(stopMode);
    }
    #endregion

    public void SetParamByName(string paramName, float value)
    {
        soundInstance.setParameterByName(paramName, value);
    }
    public void SetParamByName(string paramName, string value)
    {
        soundInstance.setParameterByNameWithLabel(paramName, value);
    }

    public float GetParamByName(string paramName)
    {
        soundInstance.getParameterByName(paramName, out float value);
        return value;
    }

    public void LoadClip(EventReference clip, bool startPlaying = false)
    {
        Stop();
        soundInstance.release();
        this.clip = clip;
        soundInstance = AudioManager.CreateFmodEventInstance(clip);
        if (startPlaying)
            Play();
    }
}
