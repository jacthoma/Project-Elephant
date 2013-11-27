using UnityEngine;
using System;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent (typeof (MeshFilter), typeof (MeshRenderer), typeof (MeshCollider))]
public partial class LandscapeTile : MonoBehaviour {
	
	[SerializeField]
	private Mesh tilePlane;
	[SerializeField]
	private int x;
	[SerializeField]
	private int y;
	[SerializeField]
	private LandscapeTile topLeftTile;
	[SerializeField]
	private LandscapeTile topTile;
	[SerializeField]
	private LandscapeTile topRightTile;
	[SerializeField]
	private LandscapeTile leftTile;
	[SerializeField]
	private LandscapeTile rightTile;
	[SerializeField]
	private LandscapeTile bottomLeftTile;
	[SerializeField]
	private LandscapeTile bottomTile;
	[SerializeField]
	private LandscapeTile bottomRightTile;
	[SerializeField]
	private int topLeftElevation;
	[SerializeField]
	private int topRightElevation;
	[SerializeField]
	private int bottomLeftElevation;
	[SerializeField]
	private int bottomRightElevation;
	
	public int X { get { return x; } set { x = value; } }
	public int Y { get { return y; } set { y = value; } }
	public LandscapeTile TopLeftTile { get { return topLeftTile; } set { topLeftTile = value; } }
	public LandscapeTile TopTile { get { return topTile; } set { topTile = value; } }
	public LandscapeTile TopRightTile { get { return topRightTile; } set { topRightTile = value; } }
	public LandscapeTile LeftTile { get { return leftTile; } set { leftTile = value; } }
	public LandscapeTile RightTile { get { return rightTile; } set { rightTile = value; } }
	public LandscapeTile BottomLeftTile { get { return bottomLeftTile; } set { bottomLeftTile = value; } }
	public LandscapeTile BottomTile { get { return bottomTile; } set { bottomTile = value; } }
	public LandscapeTile BottomRightTile { get { return bottomRightTile; } set { bottomRightTile = value; } }
	public int TopLeftElevation { get { return topLeftElevation; } set { topLeftElevation = value; } }
	public int TopRightElevation { get { return topRightElevation; } set { topRightElevation = value; } }
	public int BottomLeftElevation { get { return bottomLeftElevation; } set { bottomLeftElevation = value; } }
	public int BottomRightElevation { get { return bottomRightElevation; } set { bottomRightElevation = value; } }
	
	public void RefreshTileMesh () {
		Vector3[] vertices = new Vector3[121];
		for (int i = -5; i <= 5; i++)
			for (int j = -5; j <= 5; j++)
				vertices[(i+5)*11+(j+5)] = new Vector3 (i, ElevationAt (i, j), j);
		tilePlane.vertices = vertices;
	}
	
	public float ElevationAt (float x, float z) {
		return (HorElevationAt (x, z) + VerElevationAt (x, z)) / 2f;
	}
	private float HorElevationAt (float x, float z) {
		float leftElevation = leftModifierDel (bottomLeftElevation, topLeftElevation, z);
		float rightElevation = rightModifierDel (bottomRightElevation, topRightElevation, z);
		float topElevation = topModifierDel (leftElevation, rightElevation, x);
		float bottomElevation = bottomModifierDel (leftElevation, rightElevation, x);
		
		float topInfluence = (5 + z) / 10;
		float bottomInfluence = (5 - z) / 10;
		
		return (topElevation * topInfluence + bottomElevation * bottomInfluence);
	}
	private float VerElevationAt (float x, float z) {
		float topElevation = topModifierDel (topLeftElevation, topRightElevation, x);
		float bottomElevation = bottomModifierDel (bottomLeftElevation, bottomRightElevation, x);
		float leftElevation = leftModifierDel (bottomElevation, topElevation, z);
		float rightElevation = rightModifierDel (bottomElevation, topElevation, z);
		
		float leftInfluence = (5 - x) / 10;
		float rightInfluence = (5 + x) / 10;
		
		return (leftElevation * leftInfluence + rightElevation * rightInfluence);
	}
	
	private void CreateTilePlane () {
		tilePlane = new Mesh ();
		tilePlane.name = "TilePlane";
		
		Vector3[] vertices = new Vector3[121];
		Vector2[] uv = new Vector2[121];
		for (int i = -5; i <= 5; i++) {
			for (int j = -5; j <= 5; j++) {
				vertices[(i+5)*11+(j+5)] = new Vector3 (i, 0, j);
				uv[(i+5)*11+(j+5)] = new Vector3 ((i+5)/10f, (j+5)/10f);
			}
		}
		int[] triangles = new int[600];
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 5; j++) {
				int location = (i * 5 + j) * 24;
				int interval = i * 22 + j * 2;
				triangles[location] = interval; triangles[location + 1] = interval + 1; triangles[location + 2] = interval + 11;
				triangles[location + 3] = interval + 1; triangles[location + 4] = interval + 12; triangles[location + 5] = interval + 11;
				triangles[location + 6] = interval + 11; triangles[location + 7] = interval + 12; triangles[location + 8] = interval + 23;
				triangles[location + 9] = interval + 11; triangles[location + 10] = interval + 23; triangles[location + 11] = interval + 22;
				triangles[location + 12] = interval + 1; triangles[location + 13] = interval + 2; triangles[location + 14] = interval + 13;
				triangles[location + 15] = interval + 1; triangles[location + 16] = interval + 13; triangles[location + 17] = interval + 12;
				triangles[location + 18] = interval + 12; triangles[location + 19] = interval + 13; triangles[location + 20] = interval + 23;
				triangles[location + 21] = interval + 13; triangles[location + 22] = interval + 24; triangles[location + 23] = interval + 23;
			}
		}
		
		tilePlane.vertices = vertices;
		tilePlane.uv = uv;
		tilePlane.triangles = triangles;
		
		tilePlane.RecalculateNormals ();
	}
	
	private void Reset () {
		CreateTilePlane ();
		GetComponent<MeshFilter> ().mesh = tilePlane;
		
		topLeftElevation = topRightElevation = bottomLeftElevation = bottomRightElevation = 0;
		topModifier = leftModifier = rightModifier = bottomModifier = LandscapeTileModifierType.Flat;
		
		RefreshTileMesh ();
	}
}