using UnityEngine;
using System.Collections;

public class LandscapeTileModifierCliff : LandscapeTileModifier {

	public override float Modifier (float firstElevation, float secondElevation, float x) {
		if (x < 0) {
			return firstElevation;
		} else if (x > 0)
			return secondElevation;
		else
			return (firstElevation + secondElevation) / 2f;
	}
}
