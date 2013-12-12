using UnityEngine;
using System.Collections;

public class LandscapeTileModifierFullDip : LandscapeTileModifier {

	public override float Modifier (float firstElevation, float secondElevation, float x) {
		float k = Mathf.Min (firstElevation, secondElevation) - Mathf.Max (4, Mathf.Abs ((firstElevation - secondElevation) / 4f));
		float h = (5 - 5 * Mathf.Sqrt ((secondElevation - k) / (firstElevation - k))) / (Mathf.Sqrt ((secondElevation - k) / (firstElevation - k)) + 1);
		float a = (secondElevation - k) / (25 - 10 * h + h * h);
		
		return a * (x - h) * (x - h) + k;
	}
}
