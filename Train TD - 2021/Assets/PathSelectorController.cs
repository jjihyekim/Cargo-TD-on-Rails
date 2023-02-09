using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PathSelectorController : MonoBehaviour {
	public static PathSelectorController s;

	private void Awake() {
		s = this;
	}

	public Transform trackParent;
	public GameObject trackPrefab;
	public GameObject switchPrefab;

	private List<MiniGUI_TrackPath> _tracks = new List<MiniGUI_TrackPath>();
	private List<MiniGUI_TrackLever> _levers = new List<MiniGUI_TrackLever>();

	private void Start() {
		//StarterUIController.s.OnLevelChanged.AddListener(UpdatePathData);
		UpdatePathData();
	}

	public LevelArchetypeScriptable myLevel;
	
	void UpdatePathData() {
		var levelData = myLevel.GenerateLevel();
		
		_tracks.Clear();
		_levers.Clear();
		
		trackParent.DeleteAllChildren();

		Instantiate(trackPrefab, trackParent);
		
		for (int i = 0; i < levelData.mySegmentsA.Length - 1; i++) {
			var _lever = Instantiate(switchPrefab, trackParent).GetComponent<MiniGUI_TrackLever>();
			_lever.SetTrackState(Random.value > 0.5f);
			_lever.leverId = i;
			_levers.Add(_lever);

			var _track = Instantiate(trackPrefab, trackParent).GetComponent<MiniGUI_TrackPath>();
			_track.trackId = i;
			_tracks.Add(_track);
		}
	}

	public void ActivateLever(int id) {
		_levers[id].SetTrackState(!_levers[id].currentState);
	}
}
