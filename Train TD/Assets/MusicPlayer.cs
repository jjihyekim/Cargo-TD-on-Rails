using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicPlayer : MonoBehaviour {
	public static MusicPlayer s;

	private void Awake() {
		if(s == null)
			s = this;
		
		source = GetComponent<AudioSource>();
		realVolume = source.volume;

		currentTracks = menuMusicTracks;
		CreateRandomClipOrder();
		
		if(isPlaying)
			PlayNextTrack();
	}

	public AudioClip[] gameMusicTracks;
	public AudioClip[] menuMusicTracks;

	private AudioClip[] currentTracks;
	private AudioSource source;

	public List<int> clipOrder = new List<int>();
	private int curTrack = 0;

	public string trackNameAndTime;
	public AudioClip curTrackClip;
	public float curTrackTime;

	public bool isPlaying = true;
	public bool isPaused = false;
	public float realVolume = -1f;

	void PauseUnPauseOnGamePause(bool isPaused) {
		if (isPaused) {
			Pause();
		} else {
			Continue();
		}
	}

	public void Stop() {
		isPlaying = false;
		isPaused = false;
		source.Stop();
	}

	public void Pause() {
		if (isPlaying) {
			source.Pause();
			isPaused = true;
		}
	}

	public void Continue() {
		if (isPlaying) {
			source.UnPause();
			isPaused = false;
		}
	}

	void CreateRandomClipOrder() {
		for (int i = 0; i < currentTracks.Length; i++) {
			clipOrder.Add(i);
		}
		
		for (int i = 0; i < clipOrder.Count; i++) {
			int temp = clipOrder[i];
			int randomIndex = Random.Range(i, clipOrder.Count);
			clipOrder[i] = clipOrder[randomIndex];
			clipOrder[randomIndex] = temp;
		}
		
		curTrack = 0;
	}

	public void PlayNextTrack() {
		isPlaying = true;
		isPaused = false;
		CancelInvoke();
		curTrackClip = currentTracks[clipOrder[curTrack]];
		source.clip = curTrackClip;
		curTrackTime = 0;
		source.Play();
		curTrack += 1;
		if (curTrack >= currentTracks.Length) {
			CreateRandomClipOrder();
		}
		Invoke(nameof(PlayNextTrack), curTrackClip.length);
	}

	public void PlayPrevTrack() {
		CancelInvoke();
		curTrack -= 1;
		if (curTrack < 0)
			curTrack = 0;
		PlayNextTrack();
	}

	public void SwapMusicTracksAndPlay(bool isGame) {
		var changeMade = false;
		if (isGame) {
			if (currentTracks != gameMusicTracks) {
				currentTracks = gameMusicTracks;
				changeMade = true;
			}
		} else {
			if (currentTracks != menuMusicTracks) {
				currentTracks = menuMusicTracks;
				changeMade = true;
			}
		}

		if (changeMade) {
			//Stop();
			CreateRandomClipOrder();
			PlayNextTrack();
		}
	}

	private void Update() {
		if (isPlaying) {
			trackNameAndTime = $"{curTrackClip.name} - {curTrackTime:0}/{curTrackClip.length:0}";
			curTrackTime += Time.deltaTime;

			if (TimeController.s != null && SceneLoader.s.isLevelInProgress) {
				if (TimeController.s.isPaused && !isPaused) {
					PauseUnPauseOnGamePause(TimeController.s.isPaused);
				} else if(!TimeController.s.isPaused && isPaused) {
					PauseUnPauseOnGamePause(TimeController.s.isPaused);
				}
			}
		}
	}

	public void TemporaryVolumeReduce(float time) {
		StopAllCoroutines();
		StartCoroutine(TempVolReduce(time));
	}

	IEnumerator TempVolReduce(float time) {
		source.volume = realVolume * 0.7f;
		yield return new WaitForSeconds(time);
		source.volume = realVolume;
	}
}