using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class FMODMusicPlayer : MonoBehaviour
{
    [Header("Music Tracks")]
    public EventReference gameMusicTracks, menuMusicTracks;

    private EventReference currentTracks;   // the current track that is loaded
    private EventInstance musicPlayerInstance;  // the FMOD instance that plays the background music

    [Header("Playing Status")]
    public bool isPaused;

    private void Start()
    {
        SwapMusicTracksAndPlay(true);
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
            //Stop();
            //CreateRandomClipOrder();
            //PlayNextTrack();
            PlayTracks();
        }
    }

    /// <summary>
    /// Stop the currently-playing track. Start playing the tracks loaded in the "currentTracks" variable.
    /// </summary>
    private void PlayTracks()
    {
        musicPlayerInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicPlayerInstance.release();
        musicPlayerInstance = AudioManager.instance.CreateFmodEventInstance(currentTracks);
        musicPlayerInstance.start();
    }
    void PauseUnPauseOnGamePause(bool paused)
    {
        if (!paused)
        {
            musicPlayerInstance.setPaused(false);
            isPaused = false;
        }
        else
        {
            musicPlayerInstance.setPaused(true);
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
