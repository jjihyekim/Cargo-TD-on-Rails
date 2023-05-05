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


	public ManualHorizontalLayoutGroup layoutGroup;

	public int currentSegment = 0;

	private ConstructedLevel activeLevel => PlayStateMaster.s.currentLevel;
	
	public void SetUpPath() {
		if (activeLevel == null) {
			return;
		}

		EnemyWavesController.s.SetUpLevel();

		_tracks.Clear();
		_levers.Clear();
		
		trackParent.DeleteAllChildren();

		var firstTrack = Instantiate(trackPrefab, trackParent);
		firstTrack.GetComponent<MiniGUI_TrackPath>().SetUpTrack(true, activeLevel.mySegmentsA[0],activeLevel.mySegmentsB[0]);
		firstTrack.GetComponent<MiniGUI_TrackPath>().LockTrackState();
		
		EnemyWavesController.s.SpawnEnemiesOnSegment(0,  activeLevel.mySegmentsA[0]);

		currentSegment = 0;
		nextSegmentChangeDistance = activeLevel.mySegmentsA[0].segmentLength;
		HexGrid.s.ClearTrackSwitchDistances();
		HexGrid.s.DoTrackSwitchAtDistance(nextSegmentChangeDistance);
		
		for (int i = 0; i < activeLevel.mySegmentsA.Length - 1; i++) {
			var _lever = Instantiate(switchPrefab, trackParent).GetComponent<MiniGUI_TrackLever>();
			var leverState = Random.value > 0.5f;
			_lever.SetTrackState(leverState);
			_lever.leverId = i;
			_levers.Add(_lever);

			var _track = Instantiate(trackPrefab, trackParent).GetComponent<MiniGUI_TrackPath>();
			_track.trackId = i;
			_track.SetUpTrack(leverState, activeLevel.mySegmentsA[i+1], activeLevel.mySegmentsB[i+1]);
			_track.singleLever = _lever;
			_track.doubleLever.SetTrackState(leverState);
			_track.doubleLever.leverId = i;
			_tracks.Add(_track);
		}

		for (int i = 0; i < _tracks.Count; i++) {
			_tracks[i].SetTrackState(_tracks[i].currentState, true);
		}
		
		ReCalculateMissionLength();
		layoutGroup.isDirty = true;
	}

	public void ActivateLever(int id) {
		var stateToSet = !_levers[id].currentState;
		_levers[id].SetTrackState(stateToSet);
		_tracks[id].doubleLever.SetTrackState(stateToSet);
		_tracks[id].SetTrackState(stateToSet);

		if (id == currentSegment) {
			for (int i = 0; i < myTrackSwitchHexes.Count; i++) {
				myTrackSwitchHexes[i].isGoingLeft = stateToSet;
			}
		}

		ReCalculateMissionLength();
	}

	public void ShowTrackInfo(int id) {
		_tracks[id].ToggleTrackState(!_tracks[id].isShowingBoth);
	}

	private float nextSegmentChangeDistance = -1;

	void ReCalculateMissionLength() {
		var currentLength = activeLevel.mySegmentsA[0].segmentLength;

		for (int i = 0; i < _levers.Count; i++) {
			if (_levers[i].currentState) {
				currentLength += activeLevel.mySegmentsA[i + 1].segmentLength;
			}else {
				currentLength += activeLevel.mySegmentsB[i + 1].segmentLength;
			}
		}
		
		SpeedController.s.SetMissionEndDistance(currentLength);
	}


	private void Update() {
		if (nextSegmentChangeDistance > 0 && SpeedController.s.currentDistance > nextSegmentChangeDistance-HexGrid.s.gridSize.x -1) {
			_tracks[currentSegment].LockTrackState();
			_levers[currentSegment].LockTrackState();
			
			
			LevelSegment upcomingSegment;
			if (_levers[currentSegment].currentState) {
				upcomingSegment = activeLevel.mySegmentsA[currentSegment+1];
			}else {
				upcomingSegment = activeLevel.mySegmentsB[currentSegment+1];
			}
			
			EnemyWavesController.s.SpawnEnemiesOnSegment(nextSegmentChangeDistance, upcomingSegment);

			if (currentSegment < _tracks.Count - 1) {
				nextSegmentChangeDistance += upcomingSegment.segmentLength;
				HexGrid.s.DoTrackSwitchAtDistance(nextSegmentChangeDistance);
			} else {
				nextSegmentChangeDistance += 10000000;
			}

			currentSegment += 1;
		}
	}

	

	public List<TrackSwitchHex> myTrackSwitchHexes = new List<TrackSwitchHex>();
	public void RegisterTrackSwitchHex(TrackSwitchHex hex) {
		myTrackSwitchHexes.Add(hex);
		for (int i = 0; i < myTrackSwitchHexes.Count; i++) {
			myTrackSwitchHexes[i].isGoingLeft = _levers[currentSegment].currentState;
		}
	}
}
