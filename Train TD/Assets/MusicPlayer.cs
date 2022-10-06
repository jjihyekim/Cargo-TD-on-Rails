using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicPlayer : MonoBehaviour {
	public static MusicPlayer s;

	private void Awake() {
		s = this;
	}

	public AudioClip[] musicTracks;
	private AudioSource source;

	public List<int> clipOrder = new List<int>();
	private int curTrack = 0;

	public string trackNameAndTime;
	public AudioClip curTrackClip;
	public float curTrackTime;
	void Start() {
		source = GetComponent<AudioSource>();

		for (int i = 0; i < musicTracks.Length; i++) {
			clipOrder.Add(i);
		}

		curTrack = Random.Range(0, musicTracks.Length);
		CreateRandomClipOrder();
		PlayNextTrack();
	}

	void CreateRandomClipOrder() {
		//return;
		for (int i = 0; i < clipOrder.Count; i++) {
			int temp = clipOrder[i];
			int randomIndex = Random.Range(i, clipOrder.Count);
			clipOrder[i] = clipOrder[randomIndex];
			clipOrder[randomIndex] = temp;
		}
	}

	public void PlayNextTrack() {
		CancelInvoke();
		curTrackClip = musicTracks[clipOrder[curTrack]];
		source.clip = curTrackClip;
		curTrackTime = 0;
		source.Play();
		curTrack += 1;
		if (curTrack >= musicTracks.Length) {
			CreateRandomClipOrder();
			curTrack = 0;
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

	private void Update() {
		trackNameAndTime = $"{curTrackClip.name} - {curTrackTime:0}/{curTrackClip.length:0}";
		curTrackTime += Time.deltaTime;
	}
}