using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof (LandscapeTile))]
public class LandscapeTileEditor : Editor {
	
	private LandscapeTile tile;
	private LandscapeTileModifierType topModifier, leftModifier, rightModifier, bottomModifier;
	
	private static GameObject[] selectedObject = new GameObject[1];
	
	public override void OnInspectorGUI () {
		Reset ();
		
		NavigationButtons ();
		
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.BeginVertical (GUILayout.Width (80));
		TopLeftElevationButtons ();
		LeftModifierSelector ();
		BottomLeftElevationButtons ();
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical (GUILayout.Width (80));
		TopModifierSelector ();
		AllElevationsButtons ();
		BottomModifierSelector ();
		EditorGUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical (GUILayout.Width (80));
		TopRightElevationButtons ();
		RightModifierSelector ();
		BottomRightElevationButtons ();
		EditorGUILayout.EndVertical ();
		EditorGUILayout.EndHorizontal ();
		
		if (GUI.changed) {
			if (topModifier != tile.TopModifier)
				LandscapeTerrain.SetTopModifier (tile, topModifier);
			if (leftModifier != tile.LeftModifier)
				LandscapeTerrain.SetLeftModifier (tile, leftModifier);
			if (rightModifier != tile.RightModifier)
				LandscapeTerrain.SetRightModifier (tile, rightModifier);
			if (bottomModifier != tile.BottomModifier)
				LandscapeTerrain.SetBottomModifier (tile, bottomModifier);
			
			Repaint ();
		}
	}
	
	private void NavigationButtons () {
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.BeginVertical (GUILayout.Width (80));
		if (tile.TopLeftTile == null) {
			if (GUILayout.Button ("+", GUILayout.Width (80))) {
				LandscapeTerrain.AddTile (tile, LandscapeTilePositionAnchor.TopLeft);
				CenterSceneTo (tile.TopLeftTile.gameObject);
			}
		} else {
			if (GUILayout.Button ("", GUILayout.Width (80)))
				CenterSceneTo (tile.TopLeftTile.gameObject);
		}
		if (tile.LeftTile == null) {
			if (GUILayout.Button ("+", GUILayout.Width (80))) {
				LandscapeTerrain.AddTile (tile, LandscapeTilePositionAnchor.Left);
				CenterSceneTo (tile.LeftTile.gameObject);
			}
		} else {
			if (GUILayout.Button ("", GUILayout.Width (80)))
				CenterSceneTo (tile.LeftTile.gameObject);
		}
		if (tile.BottomLeftTile == null) {
			if (GUILayout.Button ("+", GUILayout.Width (80))) {
				LandscapeTerrain.AddTile (tile, LandscapeTilePositionAnchor.BottomLeft);
				CenterSceneTo (tile.BottomLeftTile.gameObject);
			}
		} else {
			if (GUILayout.Button ("", GUILayout.Width (80)))
				CenterSceneTo (tile.BottomLeftTile.gameObject);
		}
		EditorGUILayout.EndVertical ();
		EditorGUILayout.BeginVertical (GUILayout.Width (80));
		if (tile.TopTile == null) {
			if (GUILayout.Button ("+", GUILayout.Width (80))) {
				LandscapeTerrain.AddTile (tile, LandscapeTilePositionAnchor.Top);
				CenterSceneTo (tile.TopTile.gameObject);
			}
		} else {
			if (GUILayout.Button ("", GUILayout.Width (80)))
				CenterSceneTo (tile.TopTile.gameObject);
		}
		EditorGUILayout.LabelField (tile.X + "," + tile.Y, GUILayout.Width (80));
		if (tile.BottomTile == null) {
			if (GUILayout.Button ("+", GUILayout.Width (80))) {
				LandscapeTerrain.AddTile (tile, LandscapeTilePositionAnchor.Bottom);
				CenterSceneTo (tile.BottomTile.gameObject);
			}
		} else {
			if (GUILayout.Button ("", GUILayout.Width (80)))
				CenterSceneTo (tile.BottomTile.gameObject);
		}
		EditorGUILayout.EndVertical ();
		EditorGUILayout.BeginVertical (GUILayout.Width (80));
		if (tile.TopRightTile == null) {
			if (GUILayout.Button ("+", GUILayout.Width (80))) {
				LandscapeTerrain.AddTile (tile, LandscapeTilePositionAnchor.TopRight);
				CenterSceneTo (tile.TopRightTile.gameObject);
			}
		} else {
			if (GUILayout.Button ("", GUILayout.Width (80)))
				CenterSceneTo (tile.TopRightTile.gameObject);
		}
		if (tile.RightTile == null) {
			if (GUILayout.Button ("+", GUILayout.Width (80))) {
				LandscapeTerrain.AddTile (tile, LandscapeTilePositionAnchor.Right);
				CenterSceneTo (tile.RightTile.gameObject);
			}
		} else {
			if (GUILayout.Button ("", GUILayout.Width (80)))
				CenterSceneTo (tile.RightTile.gameObject);
		}
		if (tile.BottomRightTile == null) {
			if (GUILayout.Button ("+", GUILayout.Width (80))) {
				LandscapeTerrain.AddTile (tile, LandscapeTilePositionAnchor.BottomRight);
				CenterSceneTo (tile.BottomRightTile.gameObject);
			}
		} else {
			if (GUILayout.Button ("", GUILayout.Width (80)))
				CenterSceneTo (tile.BottomRightTile.gameObject);
		}
		EditorGUILayout.EndVertical ();
		EditorGUILayout.EndHorizontal ();
	}
	
	private void TopModifierSelector () {
		EditorGUILayout.BeginHorizontal (GUILayout.Width (80));
		topModifier = (LandscapeTileModifierType) EditorGUILayout.EnumPopup (tile.TopModifier);
		EditorGUILayout.EndHorizontal ();
	}
	
	private void LeftModifierSelector () {
		EditorGUILayout.BeginHorizontal (GUILayout.Width (80));
		leftModifier = (LandscapeTileModifierType) EditorGUILayout.EnumPopup (tile.LeftModifier);
		EditorGUILayout.EndHorizontal ();
	}
	
	private void RightModifierSelector () {
		EditorGUILayout.BeginHorizontal (GUILayout.Width (80));
		rightModifier = (LandscapeTileModifierType) EditorGUILayout.EnumPopup (tile.RightModifier);
		EditorGUILayout.EndHorizontal ();
	}
	
	private void BottomModifierSelector () {
		EditorGUILayout.BeginHorizontal (GUILayout.Width (80));
		bottomModifier = (LandscapeTileModifierType) EditorGUILayout.EnumPopup (tile.BottomModifier);
		EditorGUILayout.EndHorizontal ();
	}
	
	private void AllElevationsButtons () {
		EditorGUILayout.BeginHorizontal (GUILayout.Width (80));
		if (GUILayout.Button ("-", GUILayout.Width (24))) {
			LandscapeTerrain.SetTopLeftElevation (tile, tile.TopLeftElevation - 1);
			LandscapeTerrain.SetTopRightElevation (tile, tile.TopRightElevation - 1);
			LandscapeTerrain.SetBottomLeftElevation (tile, tile.BottomLeftElevation - 1);
			LandscapeTerrain.SetBottomRightElevation (tile, tile.BottomRightElevation - 1);
		}
		EditorGUILayout.LabelField ("" + ((tile.TopLeftElevation + tile.TopRightElevation + tile.BottomLeftElevation + tile.BottomRightElevation) / 4f), GUILayout.Width (24));
		if (GUILayout.Button ("+", GUILayout.Width (24))) {
			LandscapeTerrain.SetTopLeftElevation (tile, tile.TopLeftElevation + 1);
			LandscapeTerrain.SetTopRightElevation (tile, tile.TopRightElevation + 1);
			LandscapeTerrain.SetBottomLeftElevation (tile, tile.BottomLeftElevation + 1);
			LandscapeTerrain.SetBottomRightElevation (tile, tile.BottomRightElevation + 1);
		}
		EditorGUILayout.EndHorizontal ();
	}
	
	private void TopLeftElevationButtons () {
		EditorGUILayout.BeginHorizontal (GUILayout.Width (80));
		if (GUILayout.Button ("-", GUILayout.Width (24)))
			LandscapeTerrain.SetTopLeftElevation (tile, tile.TopLeftElevation - 1);
		EditorGUILayout.LabelField ("" + tile.TopLeftElevation, GUILayout.Width (24));
		if (GUILayout.Button ("+", GUILayout.Width (24)))
			LandscapeTerrain.SetTopLeftElevation (tile, tile.TopLeftElevation + 1);
		EditorGUILayout.EndHorizontal ();
	}
	
	private void TopRightElevationButtons () {
		EditorGUILayout.BeginHorizontal (GUILayout.Width (80));
		if (GUILayout.Button ("-", GUILayout.Width (24)))
			LandscapeTerrain.SetTopRightElevation (tile, tile.TopRightElevation - 1);
		EditorGUILayout.LabelField ("" + tile.TopRightElevation, GUILayout.Width (24));
		if (GUILayout.Button ("+", GUILayout.Width (24)))
			LandscapeTerrain.SetTopRightElevation (tile, tile.TopRightElevation + 1);
		EditorGUILayout.EndHorizontal ();
	}
	
	private void BottomLeftElevationButtons () {
		EditorGUILayout.BeginHorizontal (GUILayout.Width (80));
		if (GUILayout.Button ("-", GUILayout.Width (24)))
			LandscapeTerrain.SetBottomLeftElevation (tile, tile.BottomLeftElevation - 1);
		EditorGUILayout.LabelField ("" + tile.BottomLeftElevation, GUILayout.Width (24));
		if (GUILayout.Button ("+", GUILayout.Width (24)))
			LandscapeTerrain.SetBottomLeftElevation (tile, tile.BottomLeftElevation + 1);
		EditorGUILayout.EndHorizontal ();
	}
	
	private void BottomRightElevationButtons () {
		EditorGUILayout.BeginHorizontal (GUILayout.Width (80));
		if (GUILayout.Button ("-", GUILayout.Width (24)))
			LandscapeTerrain.SetBottomRightElevation (tile, tile.BottomRightElevation - 1);
		EditorGUILayout.LabelField ("" + tile.BottomRightElevation, GUILayout.Width (24));
		if (GUILayout.Button ("+", GUILayout.Width (24)))
			LandscapeTerrain.SetBottomRightElevation (tile, tile.BottomRightElevation + 1);
		EditorGUILayout.EndHorizontal ();
	}
	
	private void Reset () {
		if (tile == null) {
			tile = (LandscapeTile) target;
		}
		if (tile.GetComponent<MeshCollider> () == null) {
			tile.gameObject.AddComponent (typeof (MeshCollider));
		}
//		if (tile.rigidbody == null) {
//			Rigidbody b = tile.gameObject.AddComponent (typeof (Rigidbody)) as Rigidbody;
//			b.useGravity = false;
//		}
		tile.SetModifiers ();
		tile.RefreshTileMesh ();
	}
	
	private static void CenterSceneTo (GameObject obj) {
		selectedObject[0] = obj;
		Selection.objects = selectedObject;
		((SceneView)SceneView.sceneViews[0]).LookAt (selectedObject[0].transform.position);
	}
}
