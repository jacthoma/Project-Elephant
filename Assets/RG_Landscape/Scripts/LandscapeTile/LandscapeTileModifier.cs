using UnityEngine;
using System.Collections;

public class LandscapeTileModifier : MonoBehaviour {

	public virtual float Modifier (float firstElevation, float secondElevation, float x) { return 0; }
}
