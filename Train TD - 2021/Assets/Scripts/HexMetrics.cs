using System.Collections.Generic;
using UnityEngine;

public static class HexMetrics {

	public const float outerRadius = 0.75f;

	public const float innerRadius = outerRadius * 0.866025404f;

	public static Vector3[] corners = {
		new Vector3(0f, 0f, outerRadius),
		new Vector3(innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(0f, 0f, -outerRadius),
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(0f, 0f, outerRadius)
	};


	static Vector2Int cube_to_oddr(Vector3Int cube) {
		var col = cube.x + (cube.y - (cube.y & 1)) / 2;
		var row = cube.y;
		return new Vector2Int(row, col);
	}
	
	 static Vector3Int oddr_to_cube(Vector2Int coords) {
		 int x = coords.x;
		 int z = coords.y;
		var q = z - (x - (x & 1)) / 2;
		var r = x;
		return new Vector3Int(q, r, -q-r); // q, r, s
	}


	 static Vector3Int cube_subtract(Vector3Int a, Vector3Int b) {
		return new Vector3Int(a.x - b.x, a.y - b.y, a.z-b.z);
	}

	 static int cube_distance(Vector3Int a, Vector3Int b) {
		 var vec = cube_subtract(a, b);
		return (Mathf.Abs(vec.x)
		        + Mathf.Abs(vec.y)
		        + Mathf.Abs(vec.z)) / 2;
	}


	 static Vector3 cube_lerp(Vector3Int a, Vector3Int b, float t) {
		 return new Vector3(Mathf.Lerp(a.x, b.x, t),
			 Mathf.Lerp(a.y, b.y, t),
			 Mathf.Lerp(a.z, b.z, t));
	 }

	 static Vector3Int cube_round(Vector3 frac) {
		 var q = Mathf.RoundToInt(frac.x);
		 var r = Mathf.RoundToInt(frac.y);
		 var s = Mathf.RoundToInt(frac.z);

		 var q_diff = Mathf.Abs(q - frac.x);
		 var r_diff = Mathf.Abs(r - frac.y);
		 var s_diff = Mathf.Abs(s - frac.z);

		 if (q_diff > r_diff && q_diff > s_diff)
			 q = -r - s;
		 else if (r_diff > s_diff)
			 r = -q - s;
		 else
			 s = -q - r;

		 return new Vector3Int(q, r, s);
	 }

	 public static List<Vector2Int> cube_linedraw(Vector2Int oddr_a, Vector2Int oddr_b) {
		 Vector3Int a = oddr_to_cube(oddr_a);
		 Vector3Int b = oddr_to_cube(oddr_b);
		 var N = cube_distance(a, b);
		 var results = new List<Vector2Int>();
		 for (int i = 0; i < N+1; i++) {
			 results.Add( cube_to_oddr(cube_round(cube_lerp(a, b, 1.0f / N * i))));
		 }

		 return results;
	 }
}