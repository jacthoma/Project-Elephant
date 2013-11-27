using UnityEngine;
using System.Collections;

public class LandscapeObject : MonoBehaviour {
	
	public LandscapeTile TileRef { get { if (transform.parent == null) return null; return transform.parent.GetComponent<LandscapeTile> (); } }
	
	public void Move (Vector3 movement) {
		MoveVertical (movement);
		MoveHorizontal (movement);
	}
	
	private void MoveVertical (Vector3 movement) {
		Vector3 newPosition = transform.localPosition + new Vector3 (0, 0, movement.z);
		float elevation = TileRef.ElevationAt (newPosition.x, newPosition.z);
		if (elevation - transform.localPosition.y > 2.0f)
			return;
		transform.localPosition = new Vector3 (newPosition.x, elevation, newPosition.z);
	}
	
	private void MoveHorizontal (Vector3 movement) {
		Vector3 newPosition = transform.localPosition + new Vector3 (movement.x, 0, 0);
		float elevation = TileRef.ElevationAt (newPosition.x, newPosition.z);
		if (elevation - transform.localPosition.y > 2.0f)
			return;
		transform.localPosition = new Vector3 (newPosition.x, elevation, newPosition.z);
	}
}
