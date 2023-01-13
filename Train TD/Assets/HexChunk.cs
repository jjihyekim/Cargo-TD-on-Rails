using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexChunk : MonoBehaviour {
    //public HexCell[,] myCells;
    public List<GameObject> myCells = new List<GameObject>();

    public List<GameObject> foreignObjects = new List<GameObject>();


    private float gridScale = 1f;

    private List<TempBatch> tempBatches = new List<TempBatch>();

    public bool adjustPosition = true;
    public void Initialize(float _gridScale = 1) {
	    gridScale = _gridScale;
	    //myCells = new HexCell[gridSize.x, gridSize.y];
	    
	    tempBatches = new List<TempBatch>();
	    /*tempBatches.Add(new TempBatch() {
		    positions = new List<Matrix4x4>(),
		    material = grass,
		    mesh = top
	    });
		
	    tempBatches.Add(new TempBatch() {
		    positions = new List<Matrix4x4>(),
		    material = dirt,
		    mesh = bottom
	    });
		
	    tempBatches.Add( new TempBatch() {
		    positions = new List<Matrix4x4>(),
		    material = mountain,
		    mesh = top
	    });
		
	    tempBatches.Add( new TempBatch() {
		    positions = new List<Matrix4x4>(),
		    material = mountain,
		    mesh = bottom
	    });*/
    }

    public void AddCell(GameObject cell) {
        //myCells[x, z] = cell;
        myCells.Add(cell);
        
        var scale = Vector3.one  * gridScale;
        
        var allRenderers = cell.GetComponentsInChildren<MeshRenderer>();

        for (int i = 0; i < allRenderers.Length; i++) {
	        var rend = allRenderers[i];

	        bool gotMatch = false;
	        for (int j = 0; j < tempBatches.Count; j++) {
		        var checkBatch = tempBatches[j];

		        if (checkBatch.material == rend.sharedMaterial && checkBatch.mesh == rend.GetComponent<MeshFilter>().sharedMesh) {
			        checkBatch.positions.Add(Matrix4x4.TRS(rend.transform.position, Quaternion.identity, scale));
			        gotMatch = true;
			        break;
		        }
	        }

	        if (!gotMatch) {
		        var newBatch = new TempBatch() {
			        positions = new List<Matrix4x4>(),
			        material = rend.sharedMaterial,
			        mesh = rend.GetComponent<MeshFilter>().sharedMesh
		        };
		        newBatch.positions.Add(Matrix4x4.TRS(rend.transform.position, Quaternion.identity, scale));
		        
		        tempBatches.Add(newBatch);
	        }
	        
	        /*if (rend.material.name.StartsWith(grassSub)) {
		        tempBatches[0].positions.Add(Matrix4x4.TRS(rend.transform.position, Quaternion.identity, scale));
				
	        }else if (rend.material.name.StartsWith(dirtSub)) {
		        tempBatches[1].positions.Add(Matrix4x4.TRS(rend.transform.position, Quaternion.identity, scale));
				
	        } else { // mountain
		        if (rend.GetComponent<MeshFilter>().mesh.name.StartsWith(topSub)) {
			        tempBatches[2].positions.Add(Matrix4x4.TRS(rend.transform.position, Quaternion.identity, scale));
					
		        } else {
			        tempBatches[3].positions.Add(Matrix4x4.TRS(rend.transform.position, Quaternion.identity, scale));
					
		        }
	        }*/
        }
    }

    public void FinalizeBatches() {
	    print($"Got {tempBatches.Count} tempBatches to convert");
	    batches = new List<InstancingBatch>();
	    
	    
	    for (int i = 0; i < tempBatches.Count; i++) {
		    var splitBatch = tempBatches[i];
		    var posLength = splitBatch.positions.Count;
		    for (int j = 0; j < splitBatch.positions.Count; j+=1023) {
			    var newBatch = new InstancingBatch() {
				    material = splitBatch.material,
				    mesh = splitBatch.mesh,
				    positions = new List<Matrix4x4>(splitBatch.positions.GetRange(j, Mathf.Min(1023, posLength-j))).ToArray(),
				    positionsMoved = new List<Matrix4x4>(splitBatch.positions.GetRange(j, Mathf.Min(1023, posLength-j))).ToArray()
			    };


			    var propertyBlock = new MaterialPropertyBlock();
			    var posOffsets = new Vector4[newBatch.positions.Length];
			    for (int k = 0; k < newBatch.positions.Length; k++) {
				    var pos = newBatch.positions[k].GetColumn(3);
				    posOffsets[k] = pos;
			    }
			    
			    propertyBlock.SetVectorArray("WorldOffset", posOffsets);
			    
			    
			    batches.Add(newBatch);
		    }
	    }

	    drawChunk = true;

	    for (int i = 0; i < myCells.Count; i++) {
		    Destroy(myCells[i].gameObject);
	    }
	    myCells.Clear();
    }
    
    /*public HexCell GetCell(int x, int z) {
        return myCells[x, z];
    }*/

    /*public Vector2Int GetCellCoords(Vector3 worldPosition) {
        Vector2Int closest = new Vector2Int();
        var minDist = float.MaxValue;
        for (int x = 0; x < myCells.GetLength(0); x++) {
            for (int z = 0; z < myCells.GetLength(1); z++) {

                var dist = Vector3.Distance(myCells[x, z].transform.position, worldPosition);
                if (dist < minDist) {
                    minDist = dist;
                    closest.x = x;
                    closest.y = z;
                }
            }
        }

        return closest;
    }*/
    
    /*public HexCell GetCell(Vector3 worldPosition) {
        var coords = GetCellCoords(worldPosition);
        return myCells[coords.x, coords.y];
    }*/


    public void ClearForeign() {
        int count = foreignObjects.Count;
        for (int i = count-1; i >= 0; i--) {
            Destroy(foreignObjects[i].gameObject);
        }
        
        foreignObjects.Clear();
        foreignObjects.TrimExcess();
    }
    
    
    public struct TempBatch {
	    public List<Matrix4x4> positions;
	    public Material material;
	    public Mesh mesh;
    }
    public struct InstancingBatch {
		public Matrix4x4[] positions;
		public Matrix4x4[] positionsMoved;
		public MaterialPropertyBlock propertyBlock;
		public Material material;
		public Mesh mesh;
	}

	public List<InstancingBatch> batches;

	public bool drawChunk = false;

	private void Update() {
		if (drawChunk) {
			var translationMatrix = Matrix4x4.Translate(transform.position);
			
			for (int i = 0; i < batches.Count; i++) {
				var batch = batches[i];

				if (adjustPosition) {
					for (int j = 0; j < batch.positions.Length; j++) {
						batch.positionsMoved[j] = batch.positions[j] * translationMatrix;
					}
				}

				Graphics.DrawMeshInstanced(batch.mesh, 0, batch.material, batch.positionsMoved, batch.positions.Length ,batch.propertyBlock);
			}
		}
	}
}
