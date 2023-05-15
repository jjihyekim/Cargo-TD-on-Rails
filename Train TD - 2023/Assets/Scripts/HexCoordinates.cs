using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public struct HexCoordinates {

	[SerializeField]
	private int x, z;

	public int X {
		get {
			return x;
		}
	}

	public int Z {
		get {
			return z;
		}
	}

	public int Y {
		get {
			return -X - Z;
		}
	}

	public HexCoordinates (int x, int z) {
		this.x = x;
		this.z = z;
	}

	public static Vector2Int HexToOrdinal(HexCoordinates cube) {
		var col = cube.X + (cube.Z - (cube.Z & 1)) / 2;
		var row = cube.Z;
		return new Vector2Int(col, row);
	}

	public static HexCoordinates OrdinalToHex(Vector2Int hex) {
		var x = hex.x - (hex.y - (hex.y & 1)) / 2;
		var z = hex.y;
		return new HexCoordinates(x, z);
	}
	
	public static Vector3  HexToPosition (HexCoordinates cube, float y) {
		var col = cube.X + (cube.Z - (cube.Z & 1)) / 2;
		var row = cube.Z;
		var xDist = (HexMetrics.outerRadius * 1.1f);
		var zDist = HexMetrics.outerRadius;
		return new Vector3(col *xDist + ((xDist/2f) * (cube.Z&1)), y, row*zDist);
	}

	public static HexCoordinates PositionToHex(Vector3 posFloat) {
		var _z = Mathf.RoundToInt(posFloat.z/HexMetrics.innerRadius);
		var _y = Mathf.RoundToInt(posFloat.y);
		var _x = Mathf.RoundToInt(posFloat.x/HexMetrics.outerRadius - ((HexMetrics.outerRadius/2f) * (_z)));

		var pos = new Vector3Int(_x, _y, _z);
		var x = pos.x - (pos.z - (pos.z & 1)) / 2;
		var z = pos.z;
		return new HexCoordinates(x,  z);
	}

	public override string ToString () {
		return "(" +
			X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
	}

	public string ToStringOnSeparateLines () {
		return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
	}
}