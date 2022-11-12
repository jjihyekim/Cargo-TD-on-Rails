using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundscapeController : MonoBehaviour {
    public static SoundscapeController s;

    private void Awake() {
        s = this;
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

    private void Start() {
        _source = GetComponent<AudioSource>();
    }

    void Update() {
        enemyEnter.timer -= Time.deltaTime;
        enemyDie.timer -= Time.deltaTime;
        moduleBuilt.timer -= Time.deltaTime;
        moduleExplode.timer -= Time.deltaTime;
    }

    void PlayClip(AudioClip clip, float volume = 1f) {
        if (clip != null && _source != null) {
            _source.PlayOneShot(clip, volume);
            MusicPlayer.s.TemporaryVolumeReduce(clip.length);
        } else {
            Debug.LogError("Tried to play empty clip!");
        }
    }

    void PlayClipWithRandomnessAndDelay(AudioClip clip, DelayWithTimer timer) {
        if (timer.timer <= 0f) {
            timer.timer = timer.delay;
            if (Random.value < timer.playChance) {
                PlayClip(clip);
            }
        }
    }
    

    public void PlayEnemyEnter(AudioClip clip) {
        PlayClipWithRandomnessAndDelay(clip, enemyEnter);
    }

    public void PlayEnemyDie(AudioClip clip) {
        PlayClipWithRandomnessAndDelay(clip, enemyDie);
    }
    
    public void PlayModuleBuilt(AudioClip clip) {
        PlayClipWithRandomnessAndDelay(clip, moduleBuilt);
    }

    public void PlayModuleSkillActivate(AudioClip clip) {
        PlayClip(clip);
    }

    public AudioClip[] moduleExplodeSounds;
    public void PlayModuleExplode() {
        PlayClipWithRandomnessAndDelay(moduleExplodeSounds[Random.Range(0, moduleExplodeSounds.Length)], moduleExplode);
    }

    [Space]
    public AudioClip[] noMoreAmmo;
    public AudioClip[] noMoreFuel;
    public AudioClip[] noMoreScrap;

    public void PlayNoMoreResource(ResourceTypes type) {
        switch (type) {
            case ResourceTypes.ammo:
                PlayClip(noMoreAmmo[Random.Range(0, noMoreAmmo.Length)]);
                break;
            case ResourceTypes.fuel:
                PlayClip(noMoreFuel[Random.Range(0, noMoreFuel.Length)]);
                break;
            case ResourceTypes.money:
                Debug.LogError("Cannot have no moniez");
                break;
            case ResourceTypes.scraps:
                PlayClip(noMoreScrap[Random.Range(0, noMoreScrap.Length)]);
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
