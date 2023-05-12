using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailWorldScript : MonoBehaviour {

	public GameObject background;
	public GameObject previousPath;
	public GameObject possiblePath;

	private CastleWorldScript source;
	private CastleWorldScript target;
	private int outgoingIndex;

	public GameObject enemyMarker;
	public GameObject encounterMarker;

	private HexRailAffector myAffector;

	public void Initialize(CastleWorldScript _from, CastleWorldScript _to, int _outgoingIndex) {
		source = _from;
		target = _to;
		outgoingIndex = _outgoingIndex;
		
		var sourcePos = source.transform.position;
		var targetPos = target.transform.position;
		
		myAffector = GetComponent<HexRailAffector>();
		myAffector.startPos = sourcePos;
		myAffector.endPos = targetPos;

		transform.position = Vector3.Lerp(sourcePos, targetPos, 0.5f);
		
		Refresh();
	}

	public void Refresh() {
		SetHighlightState(false);
		
		if (source.myInfo.isPlayerHere) {
			SetHighlightState(true);
			target.SetTravelable(true);
					
			CameraController.s.SetMapPos(source.transform.position);
					
		}else if (source.myInfo.previouslyVisited && target.myInfo.previouslyVisited) {
			SetPreviouslyVisited();
		}
				
		SetEncounter(false);
	}

	void SetHighlightState(bool isHighlight) {
		if (isHighlight) {
			myAffector.myPrefab = possiblePath;
			
			enemyMarker.transform.localScale = Vector3.one;
			encounterMarker.transform.localScale = Vector3.one;
		} else {
			myAffector.myPrefab = background;
			
			enemyMarker.transform.localScale = Vector3.one*0.6f;
			encounterMarker.transform.localScale = Vector3.one*0.6f;
		}
	}
	void SetPreviouslyVisited() {
		myAffector.myPrefab = previousPath;
		
		enemyMarker.transform.localScale = Vector3.one*0.6f;
		encounterMarker.transform.localScale = Vector3.one*0.6f;
	}

	void SetEncounter(bool isEncounter) {
		enemyMarker.SetActive(!isEncounter);
		encounterMarker.SetActive(isEncounter);
	}

}
