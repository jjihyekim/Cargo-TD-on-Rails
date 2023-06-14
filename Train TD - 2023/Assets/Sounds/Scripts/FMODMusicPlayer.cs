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

    [Header("Playing Status")]
    public bool isPaused;
    private float targetVolume = 1;

    [Header("Dynamic Music")]
    public int numOfEngagingWave;
    public int numOfBuggy, numOfBiker;
    private List<EnemyWave> enemyWaves = new List<EnemyWave>();

    private void Awake()
    {
        if (s != null)
            Debug.LogError("FMODMusicPlayer should be a singleton class, but multiple instances are found!");
        s = this;

    }

    private void Start()
    {
        SwapMusicTracksAndPlay(false);
        enemyWaves = EnemyWavesController.s.waves;
    }

    #region Track Handling
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

    #endregion

    #region Pause/Unpause Handling
    public void PauseMusic()
    {
        if (!isPaused)
        {
            speaker.Pause();
            isPaused = true;
        }
    }

    public void UnpauseMusic()
    {
        if (isPaused)
        {
            speaker.UnPause();
            isPaused = false;
        }
    }

    void PauseUnPauseOnGamePause(bool paused)
    {
        if (!paused)
        {
            UnpauseMusic();
        }
        else
        {
            PauseMusic();
        }
    }
    #endregion

    #region Temporary Volume Reduce
    public void TemporaryVolumeReduce(float time)
    {
        CancelInvoke();
        targetVolume = 0.7f;
        Invoke("ResetVolume", time);
    }

    private void ResetVolume() 
    {
        targetVolume = 1;
    }
    #endregion

    private void Update()
    {
        #region update battle status
        // count how many waves are engaging
        int tmp_count, tmp_buggy, tmp_biker;
        tmp_count = tmp_buggy = tmp_biker = 0;
        foreach(EnemyWave wave in enemyWaves)
        {
            // if a wave is close enough and is not leaving combat, count it as "engaging"
            if(wave.distance < 20 && !wave.isLeaving)
            {
                tmp_count += 1;

                string name = wave.myEnemy.enemyUniqueName;
                if (name.Contains("Buggy"))
                    tmp_buggy += 1;
                else if (name.Contains("Biker"))
                    tmp_biker += 1;
            }
        }
        numOfEngagingWave = tmp_count;
        numOfBuggy = tmp_buggy;
        numOfBiker = tmp_biker;

        // update with FMOD
        speaker.SetParamByName("NumOfEngagingWaves", Mathf.Clamp(numOfEngagingWave, 0, 2));
        speaker.SetParamByName("NumOfBuggy", Mathf.Clamp(numOfBuggy, 0, 2 - Mathf.Clamp01(numOfBiker)));
        speaker.SetParamByName("NumOfBiker", Mathf.Clamp(numOfBiker, 0, 2 - Mathf.Clamp01(numOfBuggy)));

        #endregion

        // pause/unpause control
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

        speaker.volume = Mathf.Lerp(speaker.volume, isPaused ? 0 : targetVolume, Time.unscaledDeltaTime * 8f);
    }
}
