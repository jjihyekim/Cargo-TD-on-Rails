using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class WorldMapCreator : MonoBehaviour {
	public static WorldMapCreator s;

	private void Awake() {
		s = this;
	}

	public TMP_Text mapText;
	private void Start() {
		// we want this one frame after so that mapgenerator gets a chance to generate the map if need be
		Invoke(nameof(GenerateWorldMap), 0.01f);
	}

	public bool worldMapOpen = false;

	public WorldMapHexGrid hexGrid;
	public Transform objectsParent;

	public LayerMask castleClickLayerMask;

	public void ToggleWorldMap() {
		worldMapOpen = !worldMapOpen;

		if (worldMapOpen) {
			OpenWorldMap();
		} else {
			ReturnToRegularMap();
		}
	}


	private CastleWorldScript highlightedCastle;
	private void Update() {
		if (Mouse.current.leftButton.wasPressedThisFrame) {
			var ray = LevelReferences.s.mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

			if (Physics.Raycast(ray, out RaycastHit hit, 100, castleClickLayerMask)) {
				var castle = hit.rigidbody.GetComponent<CastleWorldScript>();
				if (castle != null) {
					MapController.s.ShowStarInfo(castle.myStar, InfoScreenHidden);

					if (highlightedCastle != null) {
						highlightedCastle.GetComponent<Outline>().enabled = false;
					}

					castle.GetComponent<Outline>().enabled = true;
					highlightedCastle = castle;
				}
			}
		}
	}

	void InfoScreenHidden() {
		if (highlightedCastle != null) {
			highlightedCastle.GetComponent<Outline>().enabled = false;
			highlightedCastle = null;
		}
	}


	void OpenWorldMap() {
		CameraController.s.EnterMapMode();
		mapText.text = "Train";
		StarterUIController.s.SetStarterUIStatus(false);
	}

	public void ReturnToRegularMap() {
		CameraController.s.ExitMapMode();
		mapText.text = "Map";
		StarterUIController.s.SetStarterUIStatus(true);
	}

	public void ResetMapPos() {
		CameraController.s.ResetMapPos();
	}

	private List<CastleWorldScript> castles;
	[Button]
	public void GenerateWorldMap() {
		objectsParent.DeleteAllChildren();
		// we dont need to re-create these
		hexGrid.ClearGrids();
		hexGrid.CreateGrids();
		castles = CreateCastles();
		CreateRandomWeights(castles);
		CreateRails(castles);
		Invoke(nameof(OneFrameLater), 0.01f);
	}

	void OneFrameLater() {
		hexGrid.ApplyHeights(OnceApplyHeightsIsDone);
	}

	void OnceApplyHeightsIsDone() {
		hexGrid.ApplyRails();
		RePositionCastles();
		ResetMapPos();

		for (int i = 0; i < castles.Count; i++) {
			castles[i].GetComponent<Outline>().enabled = false;
		}
	}

	public LayerMask castleSnapGroundLayerMask;

	void RePositionCastles() {
		for (int i = 0; i < castles.Count; i++) {
			var ray = new Ray(castles[i].transform.position + Vector3.up * 100, Vector3.down);

			if (Physics.Raycast(ray, out RaycastHit hit, 200, castleSnapGroundLayerMask)) {
				castles[i].transform.position = hit.point;
			}
		}
	}


	public Vector3 castleRandomPosOffsets = new Vector3(2,2,1);
	public float rightDistancePerChunk = 4;
	public float upDownDistancePerCastle = 3;

	List<CastleWorldScript> CreateCastles() {
		var map = DataSaver.s.GetCurrentSave().currentRun.map;

		var curDist = rightDistancePerChunk;

		var castles = new List<CastleWorldScript>();
		for (int i = 0; i < map.chunks.Count; i++) {

			var totalZRange = (map.chunks[i].myStars.Count-1) * upDownDistancePerCastle;
			var curZOffset = -totalZRange / 2;
			
			for (int j = 0; j < map.chunks[i].myStars.Count; j++) {
				var myStar = map.chunks[i].myStars[j];

				var castle = Instantiate(DataHolder.s.GetCityPrefab(myStar.city.nameSuffix), objectsParent);
				castle.transform.localPosition = new Vector3(curDist, 0, curZOffset) +
				                                 new Vector3(
					                                 Random.Range(-castleRandomPosOffsets.x, castleRandomPosOffsets.x),
					                                 Random.Range(-castleRandomPosOffsets.y, castleRandomPosOffsets.y),
					                                 Random.Range(-castleRandomPosOffsets.z, castleRandomPosOffsets.z)
				                                 );

				var script = castle.GetComponent<CastleWorldScript>();
				script.myStar = myStar;
				castles.Add(script);
				curZOffset += upDownDistancePerCastle;
			}

			curDist += rightDistancePerChunk;
		}

		return castles;
	}
	
	public Vector3 randomWeightPosOffsets = new Vector3(1,1,1);
	public Vector3 randomWeightPosBasePos = new Vector3(0,1,0);
	public GameObject randomWeightPrefab;
	public Vector2 randomWeightStrengthRange = new Vector2(0.5f, 1f);

	public Transform topRight;
	public Transform bottomLeft;

	public Vector2Int randomWeightCounts = new Vector2Int(20, 60);

	public float castleDistance = 1f;

	void CreateRandomWeights(List<CastleWorldScript> castles) {
		var xOffset = (topRight.position.x-bottomLeft.position.x)/randomWeightCounts.x;
		var zOffset = (topRight.position.z - bottomLeft.position.z)/randomWeightCounts.y;

		for (int x = 0; x < randomWeightCounts.x; x++) {
			for (int z = 0; z < randomWeightCounts.y; z++) {
				var basePos = bottomLeft.position + new Vector3(xOffset * x, 0, zOffset * z);

				var tooClose = false;
				for (int i = 0; i < castles.Count; i++) {

					if (Vector3.Distance(basePos, castles[i].transform.position) < castleDistance) {
						tooClose = true;
						break;
					}
				}
				if(tooClose)
					continue;
				

				var randomWeight = Instantiate(randomWeightPrefab, objectsParent);
				
				
				randomWeight.transform.position =  basePos+ randomWeightPosBasePos +
				                                   new Vector3(
					                                   Random.Range(-randomWeightPosOffsets.x, randomWeightPosOffsets.x),
					                                   Random.Range(-randomWeightPosOffsets.y, randomWeightPosOffsets.y),
					                                   Random.Range(-randomWeightPosOffsets.z, randomWeightPosOffsets.z)
				                                   );

				randomWeight.GetComponent<HexHeightAffector>().pinWeight = Random.Range(randomWeightStrengthRange.x, randomWeightStrengthRange.y);
			}
		}
	}

	public GameObject railAffectorPrefab;

	void CreateRails(List<CastleWorldScript> castles) {
		var map = DataSaver.s.GetCurrentSave().currentRun.map;


		for (int i = 0; i < castles.Count; i++) {
			var a = castles[i];

			for (int j = 0; j < a.myStar.outgoingConnections.Count; j++) {
				var b = GetCastleWithName(castles, a.myStar.outgoingConnections[j]);

				var rail = Instantiate(railAffectorPrefab, objectsParent).GetComponent<HexRailAffector>();
				rail.startPos = a.transform.position;
				rail.endPos = b.transform.position;
			}
		}
	}

	CastleWorldScript GetCastleWithName(List<CastleWorldScript> castles, string starName) {

		for (int i = 0; i < castles.Count; i++) {
			if (castles[i].myStar.starName == starName)
				return castles[i];
		}

		return null;
	}
}
