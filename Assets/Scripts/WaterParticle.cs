using UnityEngine;
using System.Collections;

public class WaterParticle : MonoBehaviour {
	
	private float elapsedTime;
	private float timer = 1f;
	private Vector3 direction;
	
	private static float MAX_START_LOCATION = .003f;
	private static float MAX_START_ROTATION = .0014f;
	private static float START_SPEED = .006f;
	
	void Start () {
		transform.localPosition = new Vector3 (Random.Range (-MAX_START_LOCATION, MAX_START_LOCATION), Random.Range (-MAX_START_LOCATION, MAX_START_LOCATION), 0);
		direction = new Vector3 (Random.Range (-MAX_START_ROTATION, MAX_START_ROTATION), 
			Random.Range (-MAX_START_ROTATION, MAX_START_ROTATION), Random.Range (START_SPEED - .0015f, START_SPEED + .0015f));
	}
	
	void FixedUpdate () {
		elapsedTime += Time.deltaTime;
		if (elapsedTime > timer)
			DestroyImmediate (gameObject);
		else
			transform.localPosition += direction;
	}
}
