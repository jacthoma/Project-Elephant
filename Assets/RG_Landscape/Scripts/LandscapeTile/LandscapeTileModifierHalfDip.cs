using UnityEngine;
using System.Collections;

public class LandscapeTileModifierHalfDip : LandscapeTileModifier {

	public override float Modifier (float firstElevation, float secondElevation, float x) {
		if (firstElevation == secondElevation)
			return firstElevation;
		
		float k = Mathf.Min (firstElevation, secondElevation);
		float h = (k == firstElevation ? -5 : 5);
		float a = (Mathf.Max (firstElevation, secondElevation) - k) / 100f;
		
		return a * (x - h) * (x - h) + k;
	}
}
