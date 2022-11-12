using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class WorldMapHexGrid : MonoBehaviour {

	private HexCell[] cells;

	public GameObject grassHex;
	public GameObject mountainHex;
	public GameObject trainRailsHex;

	public List<Transform> hexParents = new List<Transform>();

	public Transform otherObjectsParent;

	public Vector2Int gridSize = new Vector2Int(40, 40);

	public Vector3 startOffset = new Vector3(40, 0, 0);

	float gridOffset => gridSize.x * HexMetrics.outerRadius * 1.1f * gridScale;

	public float gridScale = 0.8f;

	void CreateGrid(Transform hexParent) {
		var hexChunk = hexParent.gameObject.AddComponent<HexChunk>();
		hexChunk.myCells = new HexCell[gridSize.x, gridSize.y];
		for (int x = - (gridSize.x/2); x < gridSize.x/2; x++) {
			for (int z = - (gridSize.y/2); z < gridSize.y/2; z++) {
				
				var hex = Instantiate(grassHex, hexParent);
				hexChunk.AddCell(hex.GetComponent<HexCell>(), x+(gridSize.x/2),z+(gridSize.y/2));
			}
		}
		UpdateGrid(hexParent);
	}
	
	void UpdateGrid(Transform hexParent) {
		var hexChunk = hexParent.gameObject.GetComponent<HexChunk>();
		hexChunk.ClearForeign();
		for (int x = - ((int)gridSize.x/2); x < gridSize.x/2; x++) {
			for (int z = - ((int)gridSize.y/2); z < gridSize.y/2; z++) {
				var y = 0f;
				
				var pos = HexCoordinates.HexToPosition(HexCoordinates.OrdinalToHex(new Vector2Int(x, z)), y);
				var hex = hexChunk.GetCell( x+(gridSize.x/2),z+(gridSize.y/2));
				hex.transform.localPosition = pos * gridScale + startOffset;
				hex.transform.localScale = Vector3.one*gridScale;
			}
		}
	}
	

	private int gridCount = 1;
	[Button]
	public void CreateGrids() {
		for (int i = 0; i < gridCount; i++) {
			var hex = new GameObject("yeet");
			hex.gameObject.name = "Hex Chunk";
			hexParents.Add(hex.transform);
			/*var rigid= hex.AddComponent<Rigidbody>();
			rigid.useGravity = false;
			rigid.isKinematic = true;*/
			hex.transform.SetParent(this.transform);
			hex.transform.localRotation = Quaternion.identity;
			CreateGrid(hex.transform);
			hex.transform.position = Vector3.forward * i * (gridOffset) + transform.position;
		}
	}

	public void ClearGrids() {
		var count = hexParents.Count;
		for (int i =  count-1; i >= 0; i--) {
			if (hexParents[i] != null) {
				var obj = hexParents[i].gameObject;
				Destroy(obj);
			}
		}
		hexParents.Clear();
		hexParents.TrimExcess();
	}
	
	[Button]
	public void ClearGridsEditor() {
		var count = hexParents.Count;
		for (int i =  count-1; i >= 0; i--) {
			if (hexParents[i] != null) {
				var obj = hexParents[i].gameObject;
				DestroyImmediate(obj);
			}
		}
		
		hexParents.Clear();
		hexParents.TrimExcess();
	}

	public float mountainThreshold = 1.37f;
	public bool refreshHeightAdjustment = true;
	[Button]
	
	public delegate void Callback();
	public void ApplyHeights(Callback myCallback) {
		StartCoroutine(ApplyHeightsOverAFewFrames(myCallback));
	}

	IEnumerator ApplyHeightsOverAFewFrames(Callback myCallback ) {
		cells = GetComponentsInChildren<HexCell>();
		var heightAffectors = GetComponentsInChildren<HexHeightAffector>();
		heightAffectors.Reverse();
		refreshHeightAdjustment = true;

		// we assume there is only one chunk
		var hexChunk = hexParents[0].GetComponent<HexChunk>();
		var pauseInterval = cells.Length / 10;
		for (int i = 0; i < cells.Length; i++) {
			var currentCell = cells[i];
			var y = 0f;

			for (int j = 0; j < heightAffectors.Length; j++) {
				var affector = heightAffectors[j];
				var cellPosNoY = currentCell.transform.position;
				var affectorPos = affector.transform.position;
				var affectorPosNoY = affectorPos;
				cellPosNoY.y = 0;
				affectorPosNoY.y = 0;
				var distance = Vector3.Distance(cellPosNoY, affectorPosNoY);
				var percent = distance / affector.pinDistance;
				percent = Mathf.Clamp(percent,0, 1);
				var falloff = affector.pinDropOff.Evaluate(percent);
				var weighted = falloff * affector.pinWeight;
				var heightAdjustment = weighted * affectorPos.y;
				var withRandom = heightAdjustment;
				if (refreshHeightAdjustment) {
					withRandom = heightAdjustment * Random.Range(1 - affector.randomness, 1 + affector.randomness);
					currentCell.randomHeightAdjustment = withRandom - heightAdjustment;
				} else {
					withRandom += currentCell.randomHeightAdjustment;
				}

				y += withRandom;
			}

			var currentCellTransform = currentCell.transform;
			var pos = currentCellTransform.localPosition;
			pos.y = y;
			//print(y);
			currentCellTransform.localPosition = pos;

			if (currentCell.transform.localPosition.y > mountainThreshold) {
				if (currentCell.myType != HexCell.HexType.mountain) {
					var hex = Instantiate(mountainHex, hexParents[0]);
					hex.transform.position = currentCell.transform.position;
					hex.transform.localScale = Vector3.one*gridScale;
					hexChunk.AddCell(hex.GetComponent<HexCell>(), currentCell.coordinates.x, currentCell.coordinates.y);
					Destroy(currentCell.gameObject);
					currentCell = hex.GetComponent<HexCell>();
				}
			} else {
				if (currentCell.myType != HexCell.HexType.grass) {
					var hex = Instantiate(grassHex, hexParents[0]);
					hex.transform.position = currentCell.transform.position;
					hex.transform.localScale = Vector3.one*gridScale;
					hexChunk.AddCell(hex.GetComponent<HexCell>(), currentCell.coordinates.x, currentCell.coordinates.y);
					Destroy(currentCell.gameObject);
					currentCell = hex.GetComponent<HexCell>();
				}
			}

			currentCell.gameObject.isStatic = true;

			if (i % pauseInterval == 0) {
				yield return null;
			}
		}
		
		refreshHeightAdjustment = false;

		yield return null;
		myCallback?.Invoke();
	}


	public float railStartGap = 0.2f;
	public float railEndGap = 0.5f;

	public LayerMask railMask;
	
	[Button]
	public void ApplyRails() {
		cells = GetComponentsInChildren<HexCell>();
		var railAffectors = GetComponentsInChildren<HexRailAffector>();

		
		// we assume there is only one chunk
		var hexChunk = hexParents[0].GetComponent<HexChunk>();
		var segmentLength = HexMetrics.innerRadius * gridScale * (1/railDensityMultiplier);
		
		for (int i = 0; i < railAffectors.Length; i++) {
			var affector = railAffectors[i];
			/*var hexCoordStart = hexChunk.GetCellCoords(affector.startPos);
			var hexCoordEnd = hexChunk.GetCellCoords(affector.endPos);
			var targetCells = HexMetrics.cube_linedraw(hexCoordStart, hexCoordEnd);*/
			/*var startNoY = affector.startPos;
			var endNoY = affector.endPos;
			startNoY.y = 0;
			endNoY.y = 0;
			var rotation = Quaternion.LookRotation(startNoY - endNoY);*/
			
			
			var start = affector.startPos;
			var end = affector.endPos;
			start = Vector3.MoveTowards(start, end, railStartGap);
			end = Vector3.MoveTowards(end,start, railEndGap);

			var lastRayResult = new Vector3();

			var ray = new Ray(start+ Vector3.up * 100, Vector3.down);

			if (Physics.Raycast(ray, out RaycastHit hit, 200, railMask)) {
				lastRayResult = hit.point;
			}

			var dist = Mathf.CeilToInt(Vector3.Distance(start, end)/segmentLength);

			for (int j = 1; j < dist+1; j++) {
				var newRayResult = Vector3.zero;
				
				var newRay = new Ray(Vector3.Lerp(start,end, (float)j/dist) + Vector3.up * 100, Vector3.down);

				if (Physics.Raycast(newRay, out RaycastHit newHit, 200, railMask)) {
					newRayResult = newHit.point;
					//Debug.Log(newHit.point);
					//Debug.Log(newHit.collider.gameObject.name);
				}
				
				var rail = Instantiate(trainRailsHex, otherObjectsParent);
				
				rail.transform.position = Vector3.Lerp(lastRayResult, newRayResult, 0.5f);
				rail.transform.rotation = Quaternion.LookRotation(lastRayResult - newRayResult);
				rail.transform.localScale = Vector3.one*gridScale;

				lastRayResult = newRayResult;
			}




			/*
			 
			var targets = LineDraw(affector.startPos, affector.endPos, segmentLength);
			 var lastRailPos2 = affector.startPos;
			var lastRailPos1 = affector.startPos;
			GameObject lastRail1 = null;
			for (int j = 0; j < targets.Count; j++) {
				var rail = Instantiate(trainRailsHex, otherObjectsParent);

				var cell = hexChunk.GetCell(targets[j]);

				var pos = targets[j];
				pos.y = cell.transform.position.y;

				rail.transform.position = pos;
				rail.transform.localScale = Vector3.one*gridScale;
				
				var rotationForLastRail = Quaternion.LookRotation(lastRailPos2 - pos);
				if (lastRail1 != null) {
					lastRail1.transform.rotation = rotationForLastRail;
					lastRail1.transform.position = Vector3.Lerp(lastRailPos2, pos, 0.5f);
				}

				lastRailPos2 = lastRailPos1;
				lastRailPos1 = pos;
				lastRail1 = rail;
			}

			var rotForLastRail = Quaternion.LookRotation(lastRailPos2 - affector.endPos);
			if(lastRail1 != null)
				lastRail1.transform.rotation = rotForLastRail;*/


			/*for (int j = 0; j < targetCells.Count; j++) {
				var cell = hexChunk.GetCell(targetCells[j].x, targetCells[j].y);

				var rail = Instantiate(trainRailsHex, otherObjectsParent);
				rail.transform.position = cell.transform.position;
				rail.transform.localScale = Vector3.one*gridScale;
				rail.transform.rotation = rotation;
			}*/
		}
	}

	public float railDensityMultiplier = 1.2f;
	List<Vector3> LineDraw(Vector3 start, Vector3 end, float segmentLength) {
		start = Vector3.MoveTowards(start, end, railStartGap);
		end = Vector3.MoveTowards(end,start, railEndGap);

		var dist = Mathf.CeilToInt(Vector3.Distance(start, end)/segmentLength);

		var results = new List<Vector3>();
		
		for (int i = 0; i < dist+1; i++) {
			results.Add(Vector3.Lerp(start,end, (float)i/dist));
		}

		return results;
	}
}