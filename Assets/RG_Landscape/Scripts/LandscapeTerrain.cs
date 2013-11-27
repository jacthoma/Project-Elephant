using UnityEngine;
using System.Collections;

public enum LandscapeTilePositionAnchor {
	TopLeft,
	Top,
	TopRight,
	Left,
	Right,
	BottomLeft,
	Bottom,
	BottomRight
}

public class LandscapeTerrain : MonoBehaviour {
	
	public static void SetTopModifier (LandscapeTile tile, LandscapeTileModifierType modifier, bool recurse = true) {
		if (tile != null) {
			tile.TopModifier = modifier;
			tile.RefreshTileMesh ();
			if (recurse)
				SetBottomModifier (tile.TopTile, modifier, false);
		}
	}
	
	public static void SetLeftModifier (LandscapeTile tile, LandscapeTileModifierType modifier, bool recurse = true) {
		if (tile != null) {
			tile.LeftModifier = modifier;
			tile.RefreshTileMesh ();
			if (recurse)
				SetRightModifier (tile.LeftTile, modifier, false);
		}
	}
	
	public static void SetRightModifier (LandscapeTile tile, LandscapeTileModifierType modifier, bool recurse = true) {
		if (tile != null) {
			tile.RightModifier = modifier;
			tile.RefreshTileMesh ();
			if (recurse)
				SetLeftModifier (tile.RightTile, modifier, false);
		}
	}
	
	public static void SetBottomModifier (LandscapeTile tile, LandscapeTileModifierType modifier, bool recurse = true) {
		if (tile != null) {
			tile.BottomModifier = modifier;
			tile.RefreshTileMesh ();
			if (recurse)
				SetTopModifier (tile.BottomTile, modifier, false);
		}
	}
	
	public static void SetTopLeftElevation (LandscapeTile tile, int elevation, bool recurse = true) {
		if (tile != null) {
			tile.TopLeftElevation = elevation;
			tile.RefreshTileMesh ();
			if (recurse) {
				SetBottomRightElevation (tile.TopLeftTile, elevation, false);
				SetBottomLeftElevation (tile.TopTile, elevation, false);
				SetTopRightElevation (tile.LeftTile, elevation, false);
			}
		}
	}
	
	public static void SetTopRightElevation (LandscapeTile tile, int elevation, bool recurse = true) {
		if (tile != null) {
			tile.TopRightElevation = elevation;
			tile.RefreshTileMesh ();
			if (recurse) {
				SetBottomRightElevation (tile.TopTile, elevation, false);
				SetBottomLeftElevation (tile.TopRightTile, elevation, false);
				SetTopLeftElevation (tile.RightTile, elevation, false);
			}
		}
	}
	
	public static void SetBottomLeftElevation (LandscapeTile tile, int elevation, bool recurse = true) {
		if (tile != null) {
			tile.BottomLeftElevation = elevation;
			tile.RefreshTileMesh ();
			if (recurse) {
				SetBottomRightElevation (tile.LeftTile, elevation, false);
				SetTopRightElevation (tile.BottomLeftTile, elevation, false);
				SetTopLeftElevation (tile.BottomTile, elevation, false);
			}
		}
	}
	
	public static void SetBottomRightElevation (LandscapeTile tile, int elevation, bool recurse = true) {
		if (tile != null) {
			tile.BottomRightElevation = elevation;
			tile.RefreshTileMesh ();
			if (recurse) {
				SetBottomLeftElevation (tile.RightTile, elevation, false);
				SetTopLeftElevation (tile.BottomRightTile, elevation, false);
				SetTopRightElevation (tile.BottomTile, elevation, false);
			}
		}
	}
	
	public static void AddTile (LandscapeTile centerTile, LandscapeTilePositionAnchor anchor) {
		LandscapeTile tile = null;
		Transform neighborTile = null;
		switch (anchor) {
		case LandscapeTilePositionAnchor.TopLeft:
			tile = CreateTile (centerTile.transform.parent, centerTile.X - 1, centerTile.Y + 1);
			break;
		case LandscapeTilePositionAnchor.Top:
			tile = CreateTile (centerTile.transform.parent, centerTile.X, centerTile.Y + 1);
			break;
		case LandscapeTilePositionAnchor.TopRight:
			tile = CreateTile (centerTile.transform.parent, centerTile.X + 1, centerTile.Y + 1);
			break;
		case LandscapeTilePositionAnchor.Left:
			tile = CreateTile (centerTile.transform.parent, centerTile.X - 1, centerTile.Y);
			break;
		case LandscapeTilePositionAnchor.Right:
			tile = CreateTile (centerTile.transform.parent, centerTile.X + 1, centerTile.Y);
			break;
		case LandscapeTilePositionAnchor.BottomLeft:
			tile = CreateTile (centerTile.transform.parent, centerTile.X - 1, centerTile.Y - 1);
			break;
		case LandscapeTilePositionAnchor.Bottom:
			tile = CreateTile (centerTile.transform.parent, centerTile.X, centerTile.Y - 1);
			break;
		case LandscapeTilePositionAnchor.BottomRight:
			tile = CreateTile (centerTile.transform.parent, centerTile.X + 1, centerTile.Y - 1);
			break;
		}
		
		neighborTile = centerTile.transform.parent.Find ("LandscapeTile_" + (tile.X - 1) + "_" + (tile.Y + 1)); // TopLeft
		if (neighborTile != null) {
			tile.TopLeftTile = neighborTile.GetComponent<LandscapeTile> ();
			neighborTile.GetComponent<LandscapeTile> ().BottomRightTile = tile;
			tile.TopLeftElevation = neighborTile.GetComponent<LandscapeTile> ().BottomRightElevation;
		}
		neighborTile = centerTile.transform.parent.Find ("LandscapeTile_" + (tile.X) + "_" + (tile.Y + 1)); // Top
		if (neighborTile != null) {
			tile.TopTile = neighborTile.GetComponent<LandscapeTile> ();
			neighborTile.GetComponent<LandscapeTile> ().BottomTile = tile;
			tile.TopLeftElevation = neighborTile.GetComponent<LandscapeTile> ().BottomLeftElevation;
			tile.TopRightElevation = neighborTile.GetComponent<LandscapeTile> ().BottomRightElevation;
			tile.TopModifier = neighborTile.GetComponent<LandscapeTile> ().BottomModifier;
		}
		neighborTile = centerTile.transform.parent.Find ("LandscapeTile_" + (tile.X + 1) + "_" + (tile.Y + 1)); // TopRight
		if (neighborTile != null) {
			tile.TopRightTile = neighborTile.GetComponent<LandscapeTile> ();
			neighborTile.GetComponent<LandscapeTile> ().BottomLeftTile = tile;
			tile.TopRightElevation = neighborTile.GetComponent<LandscapeTile> ().BottomLeftElevation;
		}
		neighborTile = centerTile.transform.parent.Find ("LandscapeTile_" + (tile.X - 1) + "_" + (tile.Y)); // Left
		if (neighborTile != null) {
			tile.LeftTile = neighborTile.GetComponent<LandscapeTile> ();
			neighborTile.GetComponent<LandscapeTile> ().RightTile = tile;
			tile.TopLeftElevation = neighborTile.GetComponent<LandscapeTile> ().TopRightElevation;
			tile.BottomLeftElevation = neighborTile.GetComponent<LandscapeTile> ().BottomRightElevation;
			tile.LeftModifier = neighborTile.GetComponent<LandscapeTile> ().RightModifier;
		}
		neighborTile = centerTile.transform.parent.Find ("LandscapeTile_" + (tile.X + 1) + "_" + (tile.Y)); // Right
		if (neighborTile != null) {
			tile.RightTile = neighborTile.GetComponent<LandscapeTile> ();
			neighborTile.GetComponent<LandscapeTile> ().LeftTile = tile;
			tile.TopRightElevation = neighborTile.GetComponent<LandscapeTile> ().TopLeftElevation;
			tile.BottomRightElevation = neighborTile.GetComponent<LandscapeTile> ().BottomLeftElevation;
			tile.RightModifier = neighborTile.GetComponent<LandscapeTile> ().LeftModifier;
		}
		neighborTile = centerTile.transform.parent.Find ("LandscapeTile_" + (tile.X - 1) + "_" + (tile.Y - 1)); // BottomLeft
		if (neighborTile != null) {
			tile.BottomLeftTile = neighborTile.GetComponent<LandscapeTile> ();
			neighborTile.GetComponent<LandscapeTile> ().TopRightTile = tile;
			tile.BottomLeftElevation = neighborTile.GetComponent<LandscapeTile> ().TopRightElevation;
		}
		neighborTile = centerTile.transform.parent.Find ("LandscapeTile_" + (tile.X) + "_" + (tile.Y - 1)); // Bottom
		if (neighborTile != null) {
			tile.BottomTile = neighborTile.GetComponent<LandscapeTile> ();
			neighborTile.GetComponent<LandscapeTile> ().TopTile = tile;
			tile.BottomLeftElevation = neighborTile.GetComponent<LandscapeTile> ().TopLeftElevation;
			tile.BottomRightElevation = neighborTile.GetComponent<LandscapeTile> ().TopRightElevation;
			tile.BottomModifier = neighborTile.GetComponent<LandscapeTile> ().TopModifier;
		}
		neighborTile = centerTile.transform.parent.Find ("LandscapeTile_" + (tile.X + 1) + "_" + (tile.Y - 1)); // BottomRight
		if (neighborTile != null) {
			tile.BottomRightTile = neighborTile.GetComponent<LandscapeTile> ();
			neighborTile.GetComponent<LandscapeTile> ().TopLeftTile = tile;
			tile.BottomRightElevation = neighborTile.GetComponent<LandscapeTile> ().TopLeftElevation;
		}
		tile.renderer.materials = centerTile.renderer.sharedMaterials;
		tile.RefreshTileMesh ();
	}
	
	private static LandscapeTile CreateTile (Transform parent, int x, int y) {
		GameObject tileObject = new GameObject ("LandscapeTile_" + x + "_" + y);
		LandscapeTile tile = (LandscapeTile) tileObject.AddComponent (typeof (LandscapeTile));
		tileObject.transform.parent = parent;
		tileObject.transform.localPosition = new Vector3 (x * 10, 0, y * 10);
		tileObject.transform.localRotation = Quaternion.identity;
		tile.X = x;
		tile.Y = y;
		return tile;
	}
	
	private void Reset () {
		gameObject.name = "LandscapeTerrain";
		transform.parent = null;
		transform.position = Vector3.zero;
		transform.rotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		
		for (int i = transform.childCount - 1; i >= 0; i--)
			DestroyImmediate (transform.GetChild (i).gameObject);
		
		CreateTile (transform, 0, 0);
	}
}
