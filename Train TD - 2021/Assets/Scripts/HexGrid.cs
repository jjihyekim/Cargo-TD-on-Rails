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

	public GameObject hexChunkPrefab;
	public GameObject railsPrefab;


	public Biome[] biomes;


	[Serializable]
	public class Biome {
		public GameObject groundPrefab;
		public PrefabWithWeights[] sideDecor;
		public GameObject sun;
		public GameObject skybox;
	}
	
	[Serializable]
	public class PrefabWithWeights {
		[HorizontalGroup(LabelWidth = 50)]
		public GameObject prefab;
		[HorizontalGroup(LabelWidth = 20, Width = 100)]
		public float weight = 1f;
		[HorizontalGroup(LabelWidth = 20, Width = 80)]
		public bool allRotation = false;
	}

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

	public float decorSpread = 1f;

	public int biomeOverride = -1;
	public void CreateCells(Transform hexParent) {
		var hexChunk = hexParent.GetComponent<HexChunk>();
		hexChunk.Initialize();

		Biome currentBiome;
		if (biomeOverride < 0) {
			var targetBiome = DataSaver.s.GetCurrentSave().currentRun.map.GetPlayerStar().biome;
			if (targetBiome < 0 || targetBiome > biomes.Length) {
				Debug.LogError($"Illegal biome {targetBiome}");
				targetBiome = 0;
			}

			currentBiome = biomes[targetBiome];
		} else {
			currentBiome = biomes[biomeOverride];
		}

		for (int i = 0; i < biomes.Length; i++) {
			biomes[i].sun.SetActive(false);
			biomes[i].skybox.SetActive(false);
		}
		
		currentBiome.sun.SetActive(true);
		currentBiome.skybox.SetActive(true);

		var guideObj = Instantiate(currentBiome.groundPrefab, hexParent);

		var sideDecorGuides = new GameObject[currentBiome.sideDecor.Length];
		for (int i = 0; i < currentBiome.sideDecor.Length; i++) {
			sideDecorGuides[i] = Instantiate(currentBiome.sideDecor[i].prefab, hexParent);
		}


		for (int x = - (gridSize.x/2); x < gridSize.x/2; x++) {
			for (int z = - (gridSize.y/2); z < gridSize.y/2; z++) {
				
				//var hex = Instantiate(hexPrefab, hexParent);
				
				var y = 0f;
				var isPath = Mathf.Abs(z) <= flatDistance;
				if (!isPath) {
					y = (Mathf.Abs(z) - flatDistance) * terrainSlope;
					y += Random.Range(0, Mathf.Abs(z * terrainRandomnessMagnitude));
				}
				
				var pos = HexCoordinates.HexToPosition(HexCoordinates.OrdinalToHex(new Vector2Int(x, z)), y);
				/*var hex = hexChunk.GetCell( x+(gridSize.x/2),z+(gridSize.x/2));
				hex.transform.localPosition = pos;*/

				guideObj.transform.localPosition = pos;
				
				hexChunk.AddCell(guideObj);

				
				// add decors
				var decors = sideDecorGuides;
				var decorCount = 0;
				if (!isPath) {
					decorCount = Random.Range(0, 5);
				}

				if (decors.Length == 0)
					decorCount = 0;

				for (int i = 0; i < decorCount; i++) {
					var curDecorIndex = WeightedRandomRoll(currentBiome.sideDecor);
					var curDecor = decors[curDecorIndex];
					var randomOffset = Random.insideUnitCircle * decorSpread;
					curDecor.transform.localPosition = pos + new Vector3(randomOffset.x, 0, randomOffset.y);
					if (currentBiome.sideDecor[curDecorIndex].allRotation) {
						curDecor.transform.rotation = Random.rotation;
					} else {
						curDecor.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
					}

					//Instantiate(curDecor);
					hexChunk.AddCell(curDecor);
				}
				
				

				if (z == 0) {
					var railPos = HexCoordinates.HexToPosition(HexCoordinates.OrdinalToHex(new Vector2Int(x, z)), 0);
					var rails = Instantiate(railsPrefab, hexParent);
					rails.transform.localPosition = railPos;
				}
			}
		}
		
		Destroy(guideObj);
		for (int i = 0; i < sideDecorGuides.Length; i++) {
			Destroy(sideDecorGuides[i]);
		}
		
		hexChunk.FinalizeBatches();
		//UpdateGrid(hexParent);
	}

	int WeightedRandomRoll(PrefabWithWeights[] F) {
		var totalFreq = 0f;
		for (int i = 0; i < F.Length; i++) {
			totalFreq += F[i].weight;
		}
		
		var roll = Random.Range(0,totalFreq);
		// Ex: we roll 0.68
		//   #0 subtracts 0.25, leaving 0.43
		//   #1 subtracts 0.4, leaving 0.03
		//   #2 is a $$anonymous$$t
		var index = -1;
		for(int i=0; i<F.Length; i++) {
			if (roll <= F[i].weight) {
				index=i; break;
			}
			roll -= F[i].weight;
		}
		// just in case we manage to roll 0.0001 past the $$anonymous$$ghest:
		if(index==-1) 
			index=F.Length-1;

		return index;
	}
	
	/*public void UpdateGrid(Transform hexParent) {
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
	}*/


	private void Start() {
		RefreshGrid();
	}

	[Button]
	void RefreshGridDebugRUNTIMEONLY(int biome) {
		biomeOverride = biome;
		RefreshGrid();
	}
	void RefreshGrid() {
		ClearGrids();
		CreateChunks();
	}

	public int gridCount = 3;
	void CreateChunks() {
		for (int i = 0; i < gridCount; i++) {
			var hex = Instantiate(hexChunkPrefab, transform);
			CreateCells(hex.transform);
			hex.transform.position = Vector3.forward * (i-1) * (gridOffset) + transform.position;
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