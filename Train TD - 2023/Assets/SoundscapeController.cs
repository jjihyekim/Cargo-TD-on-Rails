using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundscapeController : MonoBehaviour {
    public static SoundscapeController s;

    private void Awake() {
        s = this;
        _source = GetComponent<AudioSource>();
    }

    [Serializable]
    public class DelayWithTimer {
        public float delay = 0.2f;
        public float playChance = 1f;
        [NonSerialized]
        public float timer;
    }



    public DelayWithTimer enemyEnter;
    public DelayWithTimer enemyDie;
    public DelayWithTimer moduleBuilt;
    public DelayWithTimer moduleExplode;


    private AudioSource _source;

    public List<DelayWithTimer> delayWithTimers = new List<DelayWithTimer>();

    private void Start() {
        delayWithTimers.Add(enemyEnter);
        delayWithTimers.Add(enemyDie);
        delayWithTimers.Add(moduleBuilt);
        delayWithTimers.Add(moduleExplode);
        delayWithTimers.Add(noMoreAmmoDelay);
        delayWithTimers.Add(noMoreFuelDelay);
        delayWithTimers.Add(noMoreScrapDelay);
    }

    void Update() {
        for (int i = 0; i < delayWithTimers.Count; i++) {
            delayWithTimers[i].timer -= Time.deltaTime;
        }
    }

    void PlayClip(AudioClip clip, float volume = 1f) {
        if (clip != null && _source != null && MusicPlayer.s != null) {
            _source.PlayOneShot(clip, volume);
            //MusicPlayer.s.TemporaryVolumeReduce(clip.length);
            FMODMusicPlayer.s.TemporaryVolumeReduce(clip.length);
        } else {
            Debug.LogError("Tried to play empty clip!");
        }
    }

    void PlayClipWithExtraOptions(AudioClip clip, DelayWithTimer timer) {
        if (timer.timer <= 0f) {
            timer.timer = timer.delay;
            if (Random.value < timer.playChance) {
                PlayClip(clip);
            }
        }
    }
    

    public void PlayEnemyEnter(AudioClip clip) {
        PlayClipWithExtraOptions(clip, enemyEnter);
    }

    public void PlayEnemyDie(AudioClip clip) {
        PlayClipWithExtraOptions(clip, enemyDie);
    }
    
    public void PlayModuleBuilt(AudioClip clip) {
        PlayClipWithExtraOptions(clip, moduleBuilt);
    }

    public void PlayModuleSkillActivate(AudioClip clip) {
        PlayClip(clip);
    }

    public AudioClip[] moduleExplodeSounds;
    public void PlayModuleExplode() {
        PlayClipWithExtraOptions(moduleExplodeSounds[Random.Range(0, moduleExplodeSounds.Length)], moduleExplode);
    }

    [Space]
    public AudioClip[] noMoreAmmo;
    public DelayWithTimer noMoreAmmoDelay;
    public AudioClip[] noMoreFuel;
    public DelayWithTimer noMoreFuelDelay;
    public AudioClip[] noMoreScrap;
    public DelayWithTimer noMoreScrapDelay;

    public void PlayNoMoreResource(ResourceTypes type) {
        switch (type) {
            case ResourceTypes.scraps:
                PlayClipWithExtraOptions(noMoreScrap[Random.Range(0, noMoreScrap.Length)], noMoreScrapDelay);
                break;
        }
    }

    [Space]
    public AudioClip[] missionStartSound;
    public AudioClip[] missionWonSound;
    public void PlayMissionStartSound() {
        PlayClip(missionStartSound[Random.Range(0, missionStartSound.Length)]);
    }

    public void PlayMissionWonSound() {
        PlayClip(missionWonSound[Random.Range(0, missionWonSound.Length)], 1.3f);
    }
}
