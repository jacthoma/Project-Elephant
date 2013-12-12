using UnityEngine;
using System.Collections;

public class ElephantMovementControls : MonoBehaviour {

	[SerializeField]
	private CameraControls cameraControls;
	[SerializeField]
	private DebugRaycast debugRaycast;
	
	private Vector3 movement;
	private Vector3 endLocation;
	private float speed = 0.005f;
	
	void FixedUpdate () {
		transform.position += movement * speed;
		SetTerrainInfo ();
		
		if (Vector3.Distance (transform.position, endLocation) < .01f)
			Disable ();
	}
	
	public void Enable (Vector3 destination, Vector3 direction) {
		//debugRaycast.Reset (destination - direction.normalized, direction);
		
		Ray ray = new Ray (destination - direction.normalized, direction);
		RaycastHit[] hits = Physics.RaycastAll (ray);
		
		for (int i = 0; i < hits.Length; i++) {
			if (hits[i].transform.GetComponent<LandscapeTile> () != null) {
				endLocation = hits[i].point;
				movement = (hits[i].point - transform.position).normalized;
				enabled = cameraControls.enabled = true;
				return;
			}
		}
	}
	
	public void Disable () {
		enabled = cameraControls.enabled = false;
	}
	
	void SetTerrainInfo () {
		//debugRaycast.Reset (transform.position + Vector3.up, Vector3.down);
		
		Ray ray = new Ray (transform.position + Vector3.up, Vector3.down);
		RaycastHit[] hits = Physics.RaycastAll (ray);
		
		for (int i = 0; i < hits.Length; i++) {
			if (hits[i].transform.GetComponent<LandscapeTile> () != null) {
				transform.position = hits[i].point;
				
				Vector3 x = new Vector3 (hits[i].normal.x, hits[i].normal.y, 0);
				Vector3 z = new Vector3 (0, hits[i].normal.y, -hits[i].normal.z);
				float aX = (z.magnitude != 0 ? Mathf.Acos (Vector3.Dot (z.normalized, Vector3.forward) / z.magnitude) * 180 / Mathf.PI - 90 : 0);
				float aZ = (x.magnitude != 0 ? Mathf.Acos (Vector3.Dot (x.normalized, Vector3.right) / x.magnitude) * 180 / Mathf.PI - 90 : 0);
				transform.rotation = Quaternion.identity;
				transform.rotation *= Quaternion.Euler (aX, 0, aZ);
				transform.rotation *= Quaternion.Euler (0, -Mathf.Atan2 ((transform.position - endLocation).z, (transform.position - endLocation).x) * 180 / Mathf.PI - 90, 0);
				return;
			}
		}
	}
}
