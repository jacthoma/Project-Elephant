using UnityEngine;
using System.Collections;

public class LandscapeTileModifierFlat : LandscapeTileModifier {

	public override float Modifier (float firstElevation, float secondElevation, float x) {
		float m = (secondElevation - firstElevation) / 10f;
		float b = (firstElevation + secondElevation) / 2f;
		return m * x + b;
	}
}
