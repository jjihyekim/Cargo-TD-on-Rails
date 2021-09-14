using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexChunk : MonoBehaviour {
    public HexCell[,] myCells;

    public List<GameObject> foreignObjects = new List<GameObject>();

    public void AddCell(HexCell cell, int x, int z) {
        myCells[x, z] = cell;
    }
    
    public HexCell GetCell(int x, int z) {
        return myCells[x, z];
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
