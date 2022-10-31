using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class WorldMapHexGrid : MonoBehaviour {

	private HexCell[] cells;

	public GameObject grassHex;
	public GameObject mountainHex;
	public GameObject trainRailsHex;

	public List<Transform> hexParents = new List<Transform>();

	public void ClearGrid(Transform hexParent) {
		int childCount = hexParent.childCount;
		for (int i = childCount-1; i >= 0; i--) {
			DestroyImmediate(hexParent.GetChild(i).gameObject);
		}
	}

	public Vector2Int gridSize = new Vector2Int(40, 40);

	public float gridOffset {
		get {
			return gridSize.x * HexMetrics.outerRadius * 1.1f;
		}
	}

	public void CreateGrid(Transform hexParent) {
		var hexChunk = hexParent.gameObject.AddComponent<HexChunk>();
		hexChunk.myCells = new HexCell[gridSize.x, gridSize.y];
		for (int x = - (gridSize.x/2); x < gridSize.x/2; x++) {
			for (int z = - (gridSize.y/2); z < gridSize.y/2; z++) {
				
				var hex = Instantiate(grassHex, hexParent);
				hexChunk.AddCell(hex.GetComponent<HexCell>(), x+(gridSize.x/2),z+(gridSize.x/2));
			}
		}
		UpdateGrid(hexParent);
	}
	
	public void UpdateGrid(Transform hexParent) {
		var hexChunk = hexParent.gameObject.GetComponent<HexChunk>();
		hexChunk.ClearForeign();
		for (int x = - ((int)gridSize.x/2); x < gridSize.x/2; x++) {
			for (int z = - ((int)gridSize.y/2); z < gridSize.y/2; z++) {
				var y = 0f;
				
				var pos = HexCoordinates.HexToPosition(HexCoordinates.OrdinalToHex(new Vector2Int(x, z)), y);
				var hex = hexChunk.GetCell( x+(gridSize.x/2),z+(gridSize.x/2));
				hex.transform.localPosition = pos;
			}
		}
	}
	

	public int gridCount = 3;
	void CreateGrids() {
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

	void ClearGrids() {
		var count = hexParents.Count;
		for (int i =  count-1; i >= 0; i--) {
			var obj = hexParents[i].gameObject;
			Destroy(obj);
		}
		hexParents.Clear();
		hexParents.TrimExcess();
	}
	
	void ClearGridsEditor() {
		var count = hexParents.Count;
		for (int i =  count-1; i >= 0; i--) {
			var obj = hexParents[i].gameObject;
			DestroyImmediate(obj);
		}
		
		hexParents.Clear();
		hexParents.TrimExcess();
	}

	public float mountainThreshold = 1f;
	public bool refreshHeightAdjustment = true;
	public void ApplyToWorld() {
		cells = GetComponentsInChildren<HexCell>();
		var heightAffectors = GetComponentsInChildren<HexHeightAffector>();
		

		for (int i = 0; i < cells.Length; i++) {
			var currentCell = cells[i];
			var y = 0f;

			for (int j = 0; j < heightAffectors.Length; j++) {
				var affector = heightAffectors[j];
				var cellPosNoY = currentCell.transform.position;
				var affectorPosNoY = affector.transform.position;
				cellPosNoY.y = 0;
				affectorPosNoY.y = 0;
				var distance = Vector3.Distance(cellPosNoY, affectorPosNoY);
				var percent = distance / affector.pinDistance;
				percent = Mathf.Clamp(percent,0, 1);
				var falloff = affector.pinDropOff.Evaluate(percent);
				var weighted = falloff * affector.pinWeight;
				var heightAdjustment = weighted * affector.transform.localPosition.y;
				var withRandom = heightAdjustment;
				if (refreshHeightAdjustment) {
					withRandom = heightAdjustment * Random.Range(1 - affector.randomness, 1 + affector.randomness);
					currentCell.randomHeightAdjustment = withRandom - heightAdjustment;
					refreshHeightAdjustment = false;
				} else {
					withRandom += currentCell.randomHeightAdjustment;
				}

				y += withRandom;
			}

			var pos = currentCell.transform.localPosition;
			pos.y = y;
			currentCell.transform.localPosition = pos;

			if (currentCell.transform.localPosition.y > mountainThreshold) {
				if (currentCell.myType != HexCell.HexType.mountain) {
					var hex = Instantiate(mountainHex, hexParents[0]);
					hex.transform.position = currentCell.transform.position;
					//hexChunk.AddCell(hex.GetComponent<HexCell>(), x+(gridSize.x/2),z+(gridSize.x/2));
					DestroyImmediate(currentCell.gameObject);
				}
			} else {
				if (currentCell.myType != HexCell.HexType.grass) {
					var hex = Instantiate(grassHex, hexParents[0]);
					hex.transform.position = currentCell.transform.position;
					//hexChunk.AddCell(hex.GetComponent<HexCell>(), x+(gridSize.x/2),z+(gridSize.x/2));
					DestroyImmediate(currentCell.gameObject);
				}
			}
		}
	}

	private void Update() {
		ApplyToWorld();
	}

#if UNITY_EDITOR
	[MethodButton("CreateGrids", "ClearGridsEditor", "ApplyToWorld")]
	[SerializeField] private bool editorFoldout;
#endif
}