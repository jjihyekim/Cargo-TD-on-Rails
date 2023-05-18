using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class FMODMusicPlayer : MonoBehaviour
{
    public static FMODMusicPlayer s;

    [Header("Speaker")]
    public FMODAudioSource speaker;

    [Header("Music Tracks")]
    public EventReference gameMusicTracks, menuMusicTracks;

    private EventReference currentTracks;   // the current track that is loaded

    [field: Header("Playing Status")]
    public bool isPaused;

    private void Awake()
    {
        if (s != null)
            Debug.LogError("FMODMusicPlayer should be a singleton class, but multiple instances are found!");
        s = this;

    }

    private void Start()
    {
        SwapMusicTracksAndPlay(false);
    }

    /// <summary>
    /// Based on "isGame" parameter, load game/menu tracks, and play them automatically
    /// </summary>
    /// <param name="isGame"></param>
    public void SwapMusicTracksAndPlay(bool isGame)
    {
        var changeMade = false;
        if (isGame)
        {
            if (!currentTracks.Equals(gameMusicTracks))
            {
                currentTracks = gameMusicTracks;
                changeMade = true;
            }
        }
        else
        {
            if (!currentTracks.Equals(menuMusicTracks))
            {
                currentTracks = menuMusicTracks;
                changeMade = true;
            }
        }

        if (changeMade)
        {
            PlayTracks();
        }
    }
    public void PlayMenuMusic()
    {
        SwapMusicTracksAndPlay(false);
    }

    public void PlayCombatMusic()
    {
        SwapMusicTracksAndPlay(true);
    }


    /// <summary>
    /// Stop the currently-playing track. Start playing the tracks loaded in the "currentTracks" variable.
    /// </summary>
    private void PlayTracks()
    {
        speaker.LoadClip(currentTracks, true);
    }
    void PauseUnPauseOnGamePause(bool paused)
    {
        if (!paused)
        {
            speaker.UnPause();
            isPaused = false;
        }
        else
        {
            speaker.Pause();
            isPaused = true;
        }
    }

    private void Update()
    {
        if (TimeController.s != null && PlayStateMaster.s.isCombatInProgress())
        {
            if (TimeController.s.isPaused && !isPaused)
            {
                PauseUnPauseOnGamePause(TimeController.s.isPaused);
            }
            else if (!TimeController.s.isPaused && isPaused)
            {
                PauseUnPauseOnGamePause(TimeController.s.isPaused);
            }
        }
    }
}
