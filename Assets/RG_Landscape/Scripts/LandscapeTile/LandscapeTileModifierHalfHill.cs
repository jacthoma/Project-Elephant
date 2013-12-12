using UnityEngine;
using System.Collections;

public class LandscapeTileModifierHalfHill : LandscapeTileModifier {

	public override float Modifier (float firstElevation, float secondElevation, float x) {
		if (firstElevation == secondElevation)
			return firstElevation;
		
		float k = Mathf.Max (firstElevation, secondElevation);
		float h = (k == firstElevation ? -5 : 5);
		float a = (Mathf.Min (firstElevation, secondElevation) - k) / 100f;
		
		return a * (x - h) * (x - h) + k;
	}
}
