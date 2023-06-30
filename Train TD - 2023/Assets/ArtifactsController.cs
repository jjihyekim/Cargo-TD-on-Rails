using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactsController : MonoBehaviour {

	public static ArtifactsController s;

	private void Awake() {
		s = this;
	}


	public Transform artifactsParent;
	public List<Artifact> myArtifacts = new List<Artifact>();

	public bool gotBonusArtifact = false;
	public string bonusArtifactUniqueName;

	public Transform bonusArtifactStarLocation;


	public void CreateArtifactsBasedOnSaveData() {
		artifactsParent.DeleteAllChildren();
		myArtifacts.Clear();

		var artifactNames = DataSaver.s.GetCurrentSave().currentRun.artifacts;
		for (int i = 0; i < artifactNames.Length; i++) {
			var artifact = Instantiate(DataHolder.s.GetArtifact(artifactNames[i]).gameObject, artifactsParent).GetComponent<Artifact>();
			artifact.InstantEquipArtifact();
			myArtifacts.Add(artifact);
		}
		
		LayoutRebuilder.ForceRebuildLayoutImmediate(artifactsParent as RectTransform);
		
		Train.s.ArtifactsChanged();
	}

	public void SaveArtifacts() {
		string[] artifactNames = new string[myArtifacts.Count];
		for (int i = 0; i < myArtifacts.Count; i++) {
			artifactNames[i] = myArtifacts[i].uniqueName;
		}

		DataSaver.s.GetCurrentSave().currentRun.artifacts = artifactNames;
	}
	

	public void OnDisarmArtifacts() {
		for (int i = 0; i < myArtifacts.Count; i++) {
			myArtifacts[i].GetComponent<ActivateWhenOnArtifactRow>()?.Disarm();
		}
	}


	public void OnArmArtifacts() {
		for (int i = 0; i < myArtifacts.Count; i++) {
			myArtifacts[i].GetComponent<ActivateWhenOnArtifactRow>()?.Arm();
		}
	}


	public void OnEnterShop() {
		bonusArtifactStarLocation.DeleteAllChildren();
		gotBonusArtifact = false;
		var currentRun = DataSaver.s.GetCurrentSave().currentRun;
		
		if (currentRun.isInEndRunArea) {
			gotBonusArtifact = currentRun.endRunAreaInfo.gotBonusArtifact;
			bonusArtifactUniqueName = currentRun.endRunAreaInfo.bonusArtifactUniqueName;
			Instantiate(LevelReferences.s.enemyHasArtifactStar, bonusArtifactStarLocation).transform.localPosition = Vector3.zero;
		} else {
			DataSaver.s.GetCurrentSave().currentRun.endRunAreaInfo = null;
		}
	}

	public void OnAfterCombat(bool isRealCombat) {
		if (isRealCombat && DataSaver.s.GetCurrentSave().isInARun) {
			DataSaver.s.GetCurrentSave().currentRun.endRunAreaInfo = new DataSaver.EndRunAreaInfo();
			var endRunAreaInfo = DataSaver.s.GetCurrentSave().currentRun.endRunAreaInfo;
			endRunAreaInfo.gotBonusArtifact = gotBonusArtifact;
			endRunAreaInfo.bonusArtifactUniqueName = bonusArtifactUniqueName;
			DataSaver.s.SaveActiveGame();
		}
	}
	
	public void GetBonusArtifact(Transform bonusArtifactStarThing, string _bonusArtifactUniqueName) {
		gotBonusArtifact = true;
		bonusArtifactUniqueName = _bonusArtifactUniqueName;
		bonusArtifactStarThing.GetComponent<UIElementFollowWorldTarget>().enabled = false;
		StartCoroutine(MoveUIStarToTarget(bonusArtifactStarThing, bonusArtifactStarLocation));
	}

	IEnumerator MoveUIStarToTarget(Transform star, Transform target) {
		var prevLocation = star.position;
		star.SetParent(bonusArtifactStarLocation);
		star.position = prevLocation;
		Vector3 velocity = Vector3.zero;
		
		while (Vector3.Distance(star.position, target.position) > 0.01f) {
			star.position = Vector3.SmoothDamp(star.position, target.position, ref velocity, 0.4f);
			yield return null;
		}

		star.localPosition = Vector3.zero;
		
	}

	public void BonusArtifactRewarded(Transform bonusArtifactSpawnLoc) {
		gotBonusArtifact = false;
		StartCoroutine(MoveUIStarToTargetUsingRealWorldLocations(bonusArtifactStarLocation.GetChild(0), bonusArtifactSpawnLoc));
	}
	
	IEnumerator MoveUIStarToTargetUsingRealWorldLocations(Transform star, Transform target) {
		var prevLocation = star.position;
		star.SetParent(LevelReferences.s.uiDisplayParent);
		star.position = prevLocation;
		Vector3 velocity = Vector3.zero;
		
		var CanvasRect = star.root.GetComponent<RectTransform>();
		var UIRect = star.GetComponent<RectTransform>();
		var mainCam = MainCameraReference.s.cam;
		var targetPos = new Vector3(1000000, 100000, 100000);

		var speed = 0f;
		var acc = 4000f;

		while (Vector3.Distance(UIRect.anchoredPosition, targetPos) > 1f) {
			Vector3 ViewportPosition = mainCam.WorldToViewportPoint(target.position);
			Vector2 WorldObject_ScreenPosition = new Vector2(
				((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
				((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));

			targetPos = WorldObject_ScreenPosition;

			UIRect.anchoredPosition = Vector3.MoveTowards(UIRect.anchoredPosition, targetPos, speed*Time.deltaTime);
			speed += acc * Time.deltaTime;
			yield return null;
		}
		Destroy(star.gameObject);
	}


	public void EquipArtifact(Artifact artifact) {
		myArtifacts.Add(artifact);
		Train.s.ArtifactsChanged();
	}

	public void ModifyEnemy(EnemyHealth enemyHealth) {
		for (int i = 0; i < myArtifacts.Count; i++) {
			myArtifacts[i].GetComponent<ActivateWhenEnemySpawns>()?.ModifyEnemy(enemyHealth);
		}
	}
}




public abstract class ActivateWhenOnArtifactRow : MonoBehaviour {

	[ReadOnly]
	public bool isArmed = false;

	public void Arm() {
		if (isArmed == false) {
			isArmed = true;
            
			_Arm();
		}
	}

	protected abstract void _Arm();
    

	public void Disarm() {
		if (isArmed == true) {
			isArmed = false;

			_Disarm();
		}
	}
    
    
	protected abstract void _Disarm();
	
}

public abstract class ActivateWhenEnemySpawns : MonoBehaviour {

	public abstract void ModifyEnemy(EnemyHealth enemyHealth);
    
	
}