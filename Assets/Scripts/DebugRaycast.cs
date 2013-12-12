using UnityEngine;
using System.Collections;

public class DebugRaycast : MonoBehaviour {

	Vector3 direction;
	
	void Update () {
		transform.position += direction / 200f;
	}
	
	public void Reset (Vector3 origin, Vector3 direction) {
		transform.position = origin;
		this.direction = direction;
	}
}
