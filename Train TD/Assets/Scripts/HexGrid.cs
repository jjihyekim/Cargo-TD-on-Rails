using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class HexGrid : MonoBehaviour {

	public int flatDistance = 4;
	public float terrainRandomnessMagnitude = 1f;
	public float terrainSlope = 1f;

	public Color defaultColor = Color.white;

	private HexCell[] cells;

	public GameObject hexPrefab;
	public GameObject railsPrefab;

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
				
				var hex = Instantiate(hexPrefab, hexParent);
				hexChunk.AddCell(hex.GetComponent<HexCell>(), x+(gridSize.x/2),z+(gridSize.x/2));

				if (z == 0) {
					var pos = HexCoordinates.HexToPosition(HexCoordinates.OrdinalToHex(new Vector2Int(x, z)), 0);
					var rails = Instantiate(railsPrefab, hexParent);
					rails.transform.localPosition = pos;
				}
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
				if (Mathf.Abs(z) > flatDistance) {
					y = (z - flatDistance) * terrainSlope;
					y += Random.Range(0, Mathf.Abs(z * terrainRandomnessMagnitude));
				}
				
				var pos = HexCoordinates.HexToPosition(HexCoordinates.OrdinalToHex(new Vector2Int(x, z)), y);
				var hex = hexChunk.GetCell( x+(gridSize.x/2),z+(gridSize.x/2));
				hex.transform.localPosition = pos;
			}
		}
	}


	private void Start() {
		//ClearGrids();
		//CreateGrids();
	}

	public int gridCount = 3;
	[Button]
	void CreateGrids() {
		for (int i = 0; i < gridCount; i++) {
			var hex = new GameObject("yeet");
			hex.gameObject.name = "Hex Chunk";
			/*var rigid= hex.AddComponent<Rigidbody>();
			rigid.useGravity = false;
			rigid.isKinematic = true;*/
			hex.transform.SetParent(this.transform);
			hex.transform.localRotation = Quaternion.identity;
			CreateGrid(hex.transform);
			hex.transform.position = Vector3.forward * i * (gridOffset) + transform.position;
			hexParents.Add(hex.transform);
		}
	}

	void ClearGrids() {
		var count = hexParents.Count;
		for (int i =  count-1; i >= 0; i--) {
			var obj = hexParents[i].gameObject;
			Destroy(obj);
		}
		hexParents.Clear();
	}
	
	[Button]
	void ClearGridsEditor() {
		var count = hexParents.Count;
		for (int i =  count-1; i >= 0; i--) {
			var obj = hexParents[i].gameObject;
			DestroyImmediate(obj);
		}
		
		hexParents.Clear();
	}

	public float lastRealDistance = 0;
	public float distance = 0;
	private void Update() {
		var delta = SpeedController.s.currentDistance - lastRealDistance;
		distance += delta;

		foreach (var parent in hexParents) {
			//parent.GetComponent<Rigidbody>().MovePosition(parent.position + Vector3.back * GlobalReferences.s.speed * Time.deltaTime);
			
			// correct one
			parent.transform.position += Vector3.back * delta;
		}

		if (distance > gridOffset) {
			distance -= gridOffset;
			var lastHex = hexParents[0];
			hexParents.RemoveAt(0);
			lastHex.GetComponent<HexChunk>().ClearForeign();
			//UpdateGrid(lastHex);
			lastHex.position = hexParents[hexParents.Count - 1].transform.position + Vector3.forward * gridOffset;
			hexParents.Add(lastHex);
		}

		lastRealDistance = SpeedController.s.currentDistance;
	}
}