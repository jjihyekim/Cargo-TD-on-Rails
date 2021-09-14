using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTrainBackCart : MonoBehaviour {

	public Transform locationTarget;

	void Update() {
		if (LevelLoader.s.isLevelFinished) {
			transform.position = locationTarget.position;
			transform.LookAt(Train.s.trainBack);
		}
	}
}
