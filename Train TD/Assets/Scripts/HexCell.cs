using System;
using UnityEngine;

public class HexCell : MonoBehaviour {

	public HexCoordinates coordinates;

	public Vector3 expectedCoordinates;

	public float randomHeightAdjustment;

	public enum HexType {
		grass, mountain
	}

	public HexType myType;
}