using UnityEngine;
using System;
using System.Collections;

public enum LandscapeTileModifierType {
	Flat,
	FullHill,
	FullDip,
	HalfHill,
	HalfDip,
	Cliff,
}

public partial class LandscapeTile : MonoBehaviour {
	
	[SerializeField]
	private LandscapeTileModifierType topModifier;
	[SerializeField]
	private LandscapeTileModifierType leftModifier;
	[SerializeField]
	private LandscapeTileModifierType rightModifier;
	[SerializeField]
	private LandscapeTileModifierType bottomModifier;
	private Func<float, float, float, float> topModifierDel;
	private Func<float, float, float, float> leftModifierDel;
	private Func<float, float, float, float> rightModifierDel;
	private Func<float, float, float, float> bottomModifierDel;
	
	public LandscapeTileModifierType TopModifier { get { return topModifier; } set { topModifier = value; SetModifier (topModifier, ref topModifierDel); } }
	public LandscapeTileModifierType LeftModifier { get { return leftModifier; } set { leftModifier = value; SetModifier (leftModifier, ref leftModifierDel); } }
	public LandscapeTileModifierType RightModifier { get { return rightModifier; } set { rightModifier = value; SetModifier (rightModifier, ref rightModifierDel); } }
	public LandscapeTileModifierType BottomModifier { get { return bottomModifier; } set { bottomModifier = value; SetModifier (bottomModifier, ref bottomModifierDel); } }
	
	void Awake () {
		SetModifiers ();
	}
	
	private static float FlatModifier (float firstElevation, float secondElevation, float x) {
		float m = (secondElevation - firstElevation) / 10f;
		float b = (firstElevation + secondElevation) / 2f;
		return m * x + b;
	}
	
	private static float FullHillModifier (float firstElevation, float secondElevation, float x) {
		float k = Mathf.Max (firstElevation, secondElevation) + Mathf.Max (4, Mathf.Abs ((firstElevation - secondElevation) / 4f));
		float h = (5 - 5 * Mathf.Sqrt ((secondElevation - k) / (firstElevation - k))) / (Mathf.Sqrt ((secondElevation - k) / (firstElevation - k)) + 1);
		float a = (secondElevation - k) / (25 - 10 * h + h * h);
		
		return a * (x - h) * (x - h) + k;
	}
	
	private static float FullDipModifier (float firstElevation, float secondElevation, float x) {
		float k = Mathf.Min (firstElevation, secondElevation) - Mathf.Max (4, Mathf.Abs ((firstElevation - secondElevation) / 4f));
		float h = (5 - 5 * Mathf.Sqrt ((secondElevation - k) / (firstElevation - k))) / (Mathf.Sqrt ((secondElevation - k) / (firstElevation - k)) + 1);
		float a = (secondElevation - k) / (25 - 10 * h + h * h);
		
		return a * (x - h) * (x - h) + k;
	}
	
	private static float HalfHillModifier (float firstElevation, float secondElevation, float x) {
		if (firstElevation == secondElevation)
			return firstElevation;
		
		float k = Mathf.Max (firstElevation, secondElevation);
		float h = (k == firstElevation ? -5 : 5);
		float a = (Mathf.Min (firstElevation, secondElevation) - k) / 100f;
		
		return a * (x - h) * (x - h) + k;
	}
	
	private static float HalfDipModifier (float firstElevation, float secondElevation, float x) {
		if (firstElevation == secondElevation)
			return firstElevation;
		
		float k = Mathf.Min (firstElevation, secondElevation);
		float h = (k == firstElevation ? -5 : 5);
		float a = (Mathf.Max (firstElevation, secondElevation) - k) / 100f;
		
		return a * (x - h) * (x - h) + k;
	}
	
	private static float CliffModifier (float firstElevation, float secondElevation, float x) {
		if (x < 0) {
			return firstElevation;
		} else if (x > 0)
			return secondElevation;
		else
			return (firstElevation + secondElevation) / 2f;
	}
	
	private static void SetModifier (LandscapeTileModifierType modifier, ref Func<float, float, float, float> modifierDel) {
		switch (modifier) {
		case LandscapeTileModifierType.Flat:
			modifierDel = FlatModifier;
			break;
		case LandscapeTileModifierType.FullHill:
			modifierDel = FullHillModifier;
			break;
		case LandscapeTileModifierType.FullDip:
			modifierDel = FullDipModifier;
			break;
		case LandscapeTileModifierType.HalfHill:
			modifierDel = HalfHillModifier;
			break;
		case LandscapeTileModifierType.HalfDip:
			modifierDel = HalfDipModifier;
			break;
		case LandscapeTileModifierType.Cliff:
			modifierDel = CliffModifier;
			break;
		}
	}
	
	public void SetModifiers () {
		GetComponent<MeshCollider> ().sharedMesh = tilePlane;
		SetModifier (topModifier, ref topModifierDel);
		SetModifier (leftModifier, ref leftModifierDel);
		SetModifier (rightModifier, ref rightModifierDel);
		SetModifier (bottomModifier, ref bottomModifierDel);
	}
}
