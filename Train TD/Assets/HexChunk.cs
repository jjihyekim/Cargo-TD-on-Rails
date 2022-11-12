using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexChunk : MonoBehaviour {
    public HexCell[,] myCells;

    public List<GameObject> foreignObjects = new List<GameObject>();

    public void AddCell(HexCell cell, int x, int z) {
        myCells[x, z] = cell;
        cell.coordinates.x = x;
        cell.coordinates.y = z;
    }
    
    public HexCell GetCell(int x, int z) {
        return myCells[x, z];
    }

    public Vector2Int GetCellCoords(Vector3 worldPosition) {
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
    }
    
    public HexCell GetCell(Vector3 worldPosition) {
        var coords = GetCellCoords(worldPosition);
        return myCells[coords.x, coords.y];
    }


    public void ClearForeign() {
        int count = foreignObjects.Count;
        for (int i = count-1; i >= 0; i--) {
            Destroy(foreignObjects[i].gameObject);
        }
        
        foreignObjects.Clear();
        foreignObjects.TrimExcess();
    }
}
