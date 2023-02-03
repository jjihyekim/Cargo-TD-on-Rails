using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class WorldMapCreator : MonoBehaviour {
	public static WorldMapCreator s;

	private void Awake() {
		s = this;
	}

	public TMP_Text mapText;
	private void Start() {
		// we want this one frame after so that mapgenerator gets a chance to generate the map if need be
		if(DataSaver.s.GetCurrentSave().isInARun)
			Invoke(nameof(GenerateWorldMap), 0.01f);
	}

	public bool worldMapOpen = false;

	public WorldMapHexGrid hexGrid;
	public Transform objectsParent;

	public LayerMask castleClickLayerMask;

	public Image mapIcon;
	public Sprite openMapIcon;
	public Sprite backToTrainIcon;
	public void ToggleWorldMap() {
		if (!worldMapOpen) {
			OpenWorldMap();
		} else {
			ReturnToRegularMap();
		}
	}


	private CastleWorldScript highlightedCastle;
	public bool ignoreNextHide = false;

	public RectTransform[] ignoredRects;
	private void Update() {
		if (Mouse.current.leftButton.wasPressedThisFrame) {
			var mousePos = Mouse.current.position.ReadValue();
			var ray = LevelReferences.s.mainCam.ScreenPointToRay(mousePos);

			var cam = OverlayCamsReference.s.uiCam;
			for (int i = 0; i < ignoredRects.Length; i++) {
				if (ignoredRects[i].gameObject.activeInHierarchy) {
					var insideRect = RectTransformUtility.RectangleContainsScreenPoint(ignoredRects[i], mousePos, cam);
					if (insideRect) {
						return; // we dont process the click if its inside one of the rects
					}
				}
			}

			if (Physics.Raycast(ray, out RaycastHit hit, 100, castleClickLayerMask)) {
				var castle = hit.rigidbody.GetComponent<CastleWorldScript>();
				if (castle != null) {
					MapController.s.ShowStarInfo(castle.myInfo, InfoScreenHidden);

					if (highlightedCastle != null) {
						highlightedCastle.SetHighlightState(false);
					}

					castle.SetHighlightState(true);
					highlightedCastle = castle;

					ignoreNextHide = true;
					Invoke(nameof(StopIgnoreNextHide),0.01f);
				}
			}
		}
	}

	void StopIgnoreNextHide() {
		ignoreNextHide = false;
	}

	bool InfoScreenHidden() {
		if (!ignoreNextHide) {
			if (highlightedCastle != null) {
				highlightedCastle.SetHighlightState(false);
				highlightedCastle = null;
			}

			return true;
		} else {
			return false;
		}
	}


	public GameObject targetStarInfoScreenBudget;

	void OpenWorldMap() {
		if (!worldMapOpen) {
			for (int i = 0; i < castles.Count; i++) {
				castles[i].Refresh();
				if (castles[i].myInfo.isPlayerHere) {
					playerTrainTargetTransform = castles[i].playerIndicator.transform;
				}
			}
			
			for (int i = 0; i < rails.Count; i++) {
				rails[i].Refresh();
			}
			
			CameraController.s.EnterMapMode();
			mapText.text = "Train";
			StarterUIController.s.SetStarterUIStatus(false);

			Train.s.transform.position = playerTrainTargetTransform.transform.position;
			Train.s.transform.localScale = playerTrainTargetTransform.transform.localScale;

			MiniGUI_HealthBar.showHealthBars = false;
			PlayerModuleSelector.s.DisableModuleSelecting();
			worldMapOpen = true;

			mapIcon.sprite = backToTrainIcon;
		}
	}

	public void ReturnToRegularMap() {
		if (worldMapOpen) {
			CameraController.s.ExitMapMode();
			mapText.text = "Map";
			StarterUIController.s.SetStarterUIStatus(true);

			Train.s.ResetTrainPosition();

			MiniGUI_HealthBar.showHealthBars = true;
			PlayerModuleSelector.s.EnableModuleSelecting();
			worldMapOpen = false;
			targetStarInfoScreenBudget.SetActive(false);

			mapIcon.sprite = openMapIcon;
		}
	}

	public void ResetMapPos() {
		CameraController.s.ResetMapPos();
	}

	private List<CastleWorldScript> castles;

	[Button]
	public void DebugGenerateWorldMap() {
		GenerateWorldMap();
	}

	public void GenerateWorldMap() {
		ReturnToRegularMap();

		ResetMapPos();
		objectsParent.DeleteAllChildren();
		hexGrid.ClearGrids();
		castles = CreateCastles();
		CreateRandomWeights(castles);
		CreateRails();

		hexGrid.CreateGridsOverAFewFrames(AfterGridWasMade);
	}

	void AfterGridWasMade() {
		hexGrid.ApplyHeights(OnceApplyHeightsIsDone);
	}

	void OnceApplyHeightsIsDone() {
		hexGrid.ApplyRails();
		RePositionCastles();

		for (int i = 0; i < castles.Count; i++) {
			castles[i].SetHighlightState(false);
		}
		
		hexGrid.MeshCombine();
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

	public GameObject castlePrefab;

	public Transform playerTrainTargetTransform;

	List<CastleWorldScript> CreateCastles() {
		var map = DataSaver.s.GetCurrentSave().currentRun.map;

		var curDist = rightDistancePerChunk;

		var castles = new List<CastleWorldScript>();
		for (int i = 0; i < map.chunks.Count; i++) {

			var totalZRange = (map.chunks[i].myStars.Count-1) * upDownDistancePerCastle;
			var curZOffset = -totalZRange / 2;
			
			for (int j = 0; j < map.chunks[i].myStars.Count; j++) {
				var myStar = map.chunks[i].myStars[j];

				var castle = Instantiate(castlePrefab, objectsParent);
				castle.transform.localPosition = new Vector3(curDist, 0, curZOffset) +
				                                 new Vector3(
					                                 Random.Range(-castleRandomPosOffsets.x, castleRandomPosOffsets.x),
					                                 Random.Range(-castleRandomPosOffsets.y, castleRandomPosOffsets.y),
					                                 Random.Range(-castleRandomPosOffsets.z, castleRandomPosOffsets.z)
				                                 );

				var script = castle.GetComponent<CastleWorldScript>();
				script.Initialize(myStar);
				if (myStar.isPlayerHere) {
					playerTrainTargetTransform = script.playerIndicator.transform;
				}
				
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

	public List<RailWorldScript> rails = new List<RailWorldScript>();
	void CreateRails() {
		var map = DataSaver.s.GetCurrentSave().currentRun.map;

		rails.Clear();
		for (int i = 0; i < castles.Count; i++) {
			var a = castles[i];

			for (int j = 0; j < a.myInfo.outgoingConnections.Count; j++) {
				var b = GetCastleWithName(castles, a.myInfo.outgoingConnections[j]);

				var rail = Instantiate(railAffectorPrefab, objectsParent).GetComponent<RailWorldScript>();
				rail.Initialize(a,b, j);
				rails.Add(rail);
			}
		}
	}

	CastleWorldScript GetCastleWithName(List<CastleWorldScript> castles, string starName) {

		for (int i = 0; i < castles.Count; i++) {
			if (castles[i].myInfo.starName == starName)
				return castles[i];
		}

		return null;
	}

}
