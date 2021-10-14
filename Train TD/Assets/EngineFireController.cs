using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineFireController : MonoBehaviour {
    private ParticleSystem _particleSystem;
    private AudioSource _audio;

    public float baseDelay = 0.4f;
    public float minDelay = 0.1f;
    public float speedDelayEffect = 0.1f;
    public float currentDelay = 0.4f;

    public float slowSoundMaxSpeed = 1f;
    public float mediumSoundMaxSpeed = 7f;

    public AudioClip[] speedSounds;

    public GameObject soundPrefab;

    public static EngineFireController soundSource;

    private void OnEnable() {
	    if (soundSource == null)
		    soundSource = this;
    }

    private void OnDisable() {
	    if (soundSource == this)
		    soundSource = null;
    }

    void Start() {
	    _particleSystem = GetComponentInChildren<ParticleSystem>();
	    _audio = GetComponentInChildren<AudioSource>();
	    UpdateEngineParticleSystemValues();
    }

    private float lastSpeed = 0;
    public float lastSpeedSound = 0;
    //public float pitchRange = 0.2f;
    void Update()
    {
	    if (Mathf.Abs(LevelReferences.s.speed - lastSpeed) > 0.1f) {
		    UpdateEngineParticleSystemValues();
		    lastSpeed = LevelReferences.s.speed;
	    }

	    if (SceneLoader.s.isLevelInProgress) {
		    var speed = 0;
		    if (LevelReferences.s.speed > slowSoundMaxSpeed)
			    speed += 1;
		    if (LevelReferences.s.speed > mediumSoundMaxSpeed)
			    speed += 1;

		    if (Mathf.Abs(LevelReferences.s.speed - lastSpeedSound) > 0.1f) {
			    StopAllCoroutines();
			    StartCoroutine(ChangeSpeedSound(speedSounds[speed]));
			    lastSpeedSound = speed;
		    }
	    }
    }

    IEnumerator ChangeSpeedSound(AudioClip target) {
	    if (soundSource == null) {
            soundSource = this;
        }
	    
	    if (soundSource == this) {
		    _audio.loop = false;
		    while (_audio.isPlaying) {
			    yield return null;
		    }

		    _audio.clip = target;
		    _audio.loop = true;
		    _audio.Play();
	    } else {
		    _audio.Stop();
	    }
    }


    public bool isLevelFinishTriggered = false;
    void UpdateEngineParticleSystemValues() {
	    var emissionModule = _particleSystem.emission;
	    if (!SceneLoader.s.isLevelStarted) {
		    emissionModule.enabled = false;
	    } else if(lastSpeed > 0.2f) {
		    emissionModule.enabled = true;
		    var burst = emissionModule.GetBurst(0);
		    currentDelay = Mathf.Max(baseDelay/(((LevelReferences.s.speed-1)*speedDelayEffect)+1), minDelay);
		    burst.repeatInterval = currentDelay;
		    emissionModule.SetBurst(0, burst);
		    var forceOverLifeTime = _particleSystem.forceOverLifetime;
		    forceOverLifeTime.y = new ParticleSystem.MinMaxCurve(LevelReferences.s.speed * 25f);
	    } else {
		    emissionModule.enabled = false;
		    if (SceneLoader.s.isLevelFinished && !isLevelFinishTriggered) {
			    isLevelFinishTriggered = true;
			    _audio.clip = speedSounds[0];
			    _particleSystem.Stop();
			    _audio.loop = false;
		    }
	    }
    }
}
