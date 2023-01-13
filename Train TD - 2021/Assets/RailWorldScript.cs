using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailWorldScript : MonoBehaviour {

	public GameObject background;
	public GameObject previousPath;
	public GameObject possiblePath;

	private CastleWorldScript source;
	private CastleWorldScript target;

	public GameObject enemyMarker;
	public GameObject encounterMarker;

	private HexRailAffector myAffector;

	public void Initialize(CastleWorldScript _from, CastleWorldScript _to) {
		source = _from;
		target = _to;
		var sourcePos = source.transform.position;
		var targetPos = target.transform.position;
		
		myAffector = GetComponent<HexRailAffector>();
		myAffector.startPos = sourcePos;
		myAffector.endPos = targetPos;

		transform.position = Vector3.Lerp(sourcePos, targetPos, 0.5f);
			
		SetHighlightState(false);
	}

	public void SetHighlightState(bool isHighlight) {
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

	public void SetPreviouslyVisited() {
		myAffector.myPrefab = previousPath;
		
		enemyMarker.transform.localScale = Vector3.one*0.6f;
		encounterMarker.transform.localScale = Vector3.one*0.6f;
	}

	public void SetEncounter(bool isEncounter) {
		enemyMarker.SetActive(!isEncounter);
		encounterMarker.SetActive(isEncounter);
	}

}
