using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using FMOD;

public class FMODAudioSource : MonoBehaviour
{
    public EventReference clip;
    public bool pauseOnGamePause = true;
    public bool playOnAwake = true;

    private EventInstance soundInstance;
    private int pausedPosition;

    private void Awake()
    {
        if(!clip.Equals(default(EventReference)))
            soundInstance = AudioManager.CreateFmodEventInstance(clip);

        if (playOnAwake)
            Play();
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
        //Debug.Log("PAUSE");
        soundInstance.getTimelinePosition(out int position);
        pausedPosition = position;
        Stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        
        //soundInstance.getChannelGroup(out ChannelGroup channel);
        //channel.setPaused(true);
        //soundInstance.setPaused(true);
    }
    public void UnPause()
    {
        //Play();
        LoadClip(clip, true);
        soundInstance.setTimelinePosition(pausedPosition);
        //channel.setPaused(false);
        //soundInstance.setPaused(false);
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
