using UnityEngine;
using System.Collections;

public class CameraControls : MonoBehaviour {

	[SerializeField]
	private GameObject targetObject;
	
	private float distance;
	private Vector3 direction;
	private float angle = 60;
	
	
	void Start () {
		direction = new Vector3 (0, Mathf.Sin (angle * Mathf.PI / 180), -Mathf.Cos (angle * Mathf.PI / 180));
		distance = Vector3.Distance (targetObject.transform.position, transform.position);
	}
	
	void Update () {
		transform.position = targetObject.transform.position + direction * distance;
	}
}
