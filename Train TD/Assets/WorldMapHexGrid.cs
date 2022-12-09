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

	public void CreateGridsOverAFewFrames(GenericCallback callback) {
		StartCoroutine(_CreateGridsOverAFewFrames(callback));
	}

	private List<Vector3> positions = new List<Vector3>();
	IEnumerator _CreateGridsOverAFewFrames(GenericCallback callback) {
		var pauseInterval = 1000;
		var n = 0;
		
		for (int i = 0; i < gridCount; i++) {
			var hexParent = new GameObject("yeet");
			hexParent.gameObject.name = "Hex Chunk";
			hexParents.Add(hexParent.transform);
			/*var rigid= hex.AddComponent<Rigidbody>();
			rigid.useGravity = false;
			rigid.isKinematic = true;*/
			hexParent.transform.SetParent(this.transform);
			hexParent.transform.localRotation = Quaternion.identity;

			var hexChunk = hexParent.AddComponent<HexChunk>();
			hexChunk.myCells = new HexCell[gridSize.x, gridSize.y];
			for (int x = - (gridSize.x/2); x < gridSize.x/2; x++) {
				for (int z = - (gridSize.y/2); z < gridSize.y/2; z++) {
				
					var hex = Instantiate(grassHex, hexParent.transform);
					hexChunk.AddCell(hex.GetComponent<HexCell>(), x+(gridSize.x/2),z+(gridSize.y/2));
					
					n++;
					
					if (n % pauseInterval == 0) {
						yield return null;
					}
				}
			}
			UpdateGrid(hexParent.transform);
			
			hexParent.transform.position = Vector3.forward * i * (gridOffset) + transform.position;
		}

		yield return null;
		callback?.Invoke();
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
	public GenericCallback myCallback;
	[Button] 
	public void ApplyHeights(GenericCallback _myCallback) {
		StartCoroutine(ApplyHeightsOverAFewFrames());
		//ApplyHeightsOverAFewFrames();
		myCallback = _myCallback;
	}

	public Color regularColor= Color.green;
	public Color crystalColor = Color.magenta;

	public float colorLerpStartZ;
	public float colorLerpEndZ;

	struct AffectorStuff {
		public Vector3 posNoY;
		public float posY;
		public float pinDistance;
		public float pinWeight;
		public AnimationCurve pinDropOff;
		public float randomness;
	}
	IEnumerator ApplyHeightsOverAFewFrames( ) {
		cells = GetComponentsInChildren<HexCell>();
		var heightAffectors = GetComponentsInChildren<HexHeightAffector>();
		heightAffectors.Reverse();

		var n = heightAffectors.Length;
		
		// pre-process and put into array heightAffectorStuff
		var affectorStuff = new AffectorStuff[heightAffectors.Length];
		for (int i = 0; i < heightAffectors.Length; i++) {
			var affector = heightAffectors[i];
			var affectorPos = affector.transform.position;
			var posNoY = affectorPos;
			posNoY.y = 0;
			affectorStuff[i] = new AffectorStuff() {
				posNoY = posNoY,
				posY = affectorPos.y,
				pinDistance = affector.pinDistance,
				pinWeight = affector.pinWeight,
				pinDropOff = affector.pinDropOff,
				randomness = affector.randomness,
			};
		}
		
		//refreshHeightAdjustment = true;

		// we assume there is only one chunk
		var hexChunk = hexParents[0].GetComponent<HexChunk>();
		var pauseInterval = cells.Length / 5;
		for (int i = 0; i < cells.Length; i++) {
			var currentCell = cells[i];
			var y = 0f;
			
			
			var currentCellTransform = currentCell.transform;

			for (int j = 0; j < heightAffectors.Length; j++) {
				/*var affector = heightAffectors[j];
				var affectorPos = affector.transform.position;
				var affectorPosNoY = affectorPos;
				affectorPosNoY.y = 0;*/
				var affector = affectorStuff[j];
				var cellPosNoY = currentCellTransform.position;
				cellPosNoY.y = 0;
				var distance = Vector3.Distance(cellPosNoY, affector.posNoY);
				var percent = distance / affector.pinDistance;
				percent = Mathf.Clamp(percent,0, 1);
				var falloff = affector.pinDropOff.Evaluate(percent);
				var weighted = falloff * affector.pinWeight;
				var heightAdjustment = weighted * affector.posY;
				//var withRandom = heightAdjustment;
				var withRandom = heightAdjustment * Random.Range(1 - affector.randomness, 1 + affector.randomness);
				/*if (refreshHeightAdjustment) {
					withRandom = heightAdjustment * Random.Range(1 - affector.randomness, 1 + affector.randomness);
					currentCell.randomHeightAdjustment = withRandom - heightAdjustment;
				} else {
					withRandom += currentCell.randomHeightAdjustment;
				}*/

				y += withRandom;
			}

			var finalPos = currentCellTransform.localPosition;
			finalPos.y = y;
			//print(y);
			currentCellTransform.localPosition = finalPos;
			
			/*var lerp = (currentCell.transform.position.z - colorLerpStartZ)/(colorLerpEndZ - colorLerpStartZ);
			lerp = Mathf.Clamp01(lerp);
			var propBlock = new MaterialPropertyBlock();
			var renderer = currentCell.GetComponentInChildren<Renderer>();
			renderer.GetPropertyBlock(propBlock);
			propBlock.SetColor("_Color", Color.Lerp(regularColor, crystalColor, lerp));
			renderer.SetPropertyBlock(propBlock);*/
			

			if (finalPos.y > mountainThreshold) {
				if (currentCell.myType != HexCell.HexType.mountain) {
					var hex = Instantiate(mountainHex, hexParents[0]);
					var hexTransform = hex.transform;
					hexTransform.localPosition = finalPos;
					hexTransform.localScale = Vector3.one*gridScale;
					hexChunk.AddCell(hex.GetComponent<HexCell>(), currentCell.coordinates.x, currentCell.coordinates.y);
					Destroy(currentCell.gameObject);
					currentCell = hex.GetComponent<HexCell>();
				}
			} else {
				if (currentCell.myType != HexCell.HexType.grass) {
					var hex = Instantiate(grassHex, hexParents[0]);
					var hexTransform = hex.transform;
					hexTransform.localPosition = finalPos;
					hexTransform.localScale = Vector3.one*gridScale;
					hexChunk.AddCell(hex.GetComponent<HexCell>(), currentCell.coordinates.x, currentCell.coordinates.y);
					Destroy(currentCell.gameObject);
					currentCell = hex.GetComponent<HexCell>();
				}
			}

			//currentCell.gameObject.isStatic = true;
			
			if (i % pauseInterval == 0) {
				yield return null;
			}
		}
		
		//refreshHeightAdjustment = false;

		yield return null;
		//Invoke(nameof(CallbackAfterOneFrame), 0.01f);
		//Debug.Break();
		myCallback?.Invoke();
		//doCallback = true;
	}

	/*public bool doCallback = false;
	private void Update() {
		if (doCallback) {
			myCallback?.Invoke();
			doCallback = false;
		}
	}*/

	/*void CallbackAfterOneFrame() {
		myCallback?.Invoke();
	}*/


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
			var trainRailsHex = affector.myPrefab;
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

				if (j == (dist + 1) / 2) {
					affector.transform.position = Vector3.Lerp(lastRayResult, newRayResult, 0.5f);
				}
				
				var rail = Instantiate(trainRailsHex, otherObjectsParent);
				
				rail.transform.position = Vector3.Lerp(lastRayResult, newRayResult, 0.5f);
				var lookVector = lastRayResult - newRayResult;
				if(lookVector.sqrMagnitude > 0)
					rail.transform.rotation = Quaternion.LookRotation(lastRayResult - newRayResult);
				rail.transform.localScale = Vector3.one*gridScale;

				lastRayResult = newRayResult;
			}
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

	public void MeshCombine() {
		//StartCoroutine(MeshCombineOverFrames());
		SwitchToGPUInstancing();
	}
	public struct InstancingBatch {
		public List<Matrix4x4> positions;
		public Material material;
		public Mesh mesh;
	}

	public List<InstancingBatch> batches;

	public Material grass;
	public Material dirt;
	public Material mountain;

	public Mesh top;
	public Mesh bottom;

	public bool isGPUInstancing = false;

	private void Update() {
		if (isGPUInstancing) {
			for (int i = 0; i < batches.Count; i++) {
				var batch = batches[i];
				Graphics.DrawMeshInstanced(batch.mesh, 0, batch.material, batch.positions);
			}
		}
	}

	void SwitchToGPUInstancing() {
		Debug.Log("Gpu Instancing Map Terrain");

		var allRenderers = hexParents[0].GetComponentsInChildren<MeshRenderer>();

		var tempBatches = new List<InstancingBatch>();
		tempBatches.Add(new InstancingBatch() {
			positions = new List<Matrix4x4>(),
			material = grass,
			mesh = top
		});
		
		tempBatches.Add(new InstancingBatch() {
			positions = new List<Matrix4x4>(),
			material = dirt,
			mesh = bottom
		});
		
		tempBatches.Add( new InstancingBatch() {
			positions = new List<Matrix4x4>(),
			material = mountain,
			mesh = top
		});
		
		tempBatches.Add( new InstancingBatch() {
			positions = new List<Matrix4x4>(),
			material = mountain,
			mesh = bottom
		});

		var scale = Vector3.one * gridScale;
		var grassSub = grass.name.Substring(0, 3);
		var dirtSub = dirt.name.Substring(0, 3);
		var topSub = top.name.Substring(0, 3);

		for (int i = 0; i < allRenderers.Length; i++) {
			var rend = allRenderers[i];
			if (rend.material.name.StartsWith(grassSub)) {
				tempBatches[0].positions.Add(Matrix4x4.TRS(rend.transform.position, Quaternion.identity, scale));
				
			}else if (rend.material.name.StartsWith(dirtSub)) {
				tempBatches[1].positions.Add(Matrix4x4.TRS(rend.transform.position, Quaternion.identity, scale));
				
			} else { // mountain
				if (rend.GetComponent<MeshFilter>().mesh.name.StartsWith(topSub)) {
					tempBatches[2].positions.Add(Matrix4x4.TRS(rend.transform.position, Quaternion.identity, scale));
					
				} else {
					tempBatches[3].positions.Add(Matrix4x4.TRS(rend.transform.position, Quaternion.identity, scale));
					
				}
			}
		}

		batches = new List<InstancingBatch>();
		for (int i = 0; i < 4; i++) {
			var splitBatch = tempBatches[i];
			var posLength = splitBatch.positions.Count;
			for (int j = 0; j < splitBatch.positions.Count; j+=1023) {
				var newBatch = new InstancingBatch() {
					material = splitBatch.material,
					mesh = splitBatch.mesh,
					positions = new List<Matrix4x4>(splitBatch.positions.GetRange(j, Mathf.Min(1023, posLength-j)))
				};
				batches.Add(newBatch);
			}
		}
		
		Destroy(hexParents[0].gameObject);

		isGPUInstancing = true;
	}

	IEnumerator MeshCombineOverFrames() {
		var target = hexParents[0].gameObject;
		Debug.Log("combining meshes");
		//var newMesh = PlaytimeMeshCombiner.CombineMeshes(hexParents[0].gameObject);

		yield return null;
		
		yield return
			PlaytimeMeshCombiner.MeshCombineAlternative(
				target,
				transform,
				target.GetComponentsInChildren<MeshFilter>(),
				target.GetComponentsInChildren<MeshRenderer>()
			);
		Debug.Log("meshes combined");
		
		Destroy(hexParents[0].gameObject);
		//newMeshes.transform.SetParent(transform);
	}
}