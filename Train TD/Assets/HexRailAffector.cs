using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HexRailAffector : MonoBehaviour {
   public Vector3 startPos;
   public Vector3 endPos;
   
   private void OnDrawGizmos() {
      Gizmos.color = Color.blue;
      Gizmos.DrawLine(startPos + Vector3.up*3, endPos + Vector3.up*1);
   }
}


/*[CustomEditor(typeof(HexRailAffector))]
public class DrawWireArc : Editor
{
   void OnSceneGUI()
   {
      Handles.color = Color.red;
      HexRailAffector myObj = (HexRailAffector)target;
      //Handles.DrawSphere
      //myObj.shieldArea = (float)Handles.ScaleValueHandle(myObj.shieldArea, myObj.transform.position + myObj.transform.forward * myObj.shieldArea, myObj.transform.rotation, 1, Handles.ConeHandleCap, 1);
   }
}*/
