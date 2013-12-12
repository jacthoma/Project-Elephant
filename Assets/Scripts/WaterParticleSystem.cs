using UnityEngine;
using System.Collections;

public class WaterParticleSystem : MonoBehaviour {
	
	[SerializeField]
	private GameObject waterParticle;
	[SerializeField]
	private float timer = 2.0f;
	
	private float elapsedTime;
	
	void FixedUpdate () {
		int particlesThisFrame = UnityEngine.Random.Range (1, 5);
		for (int i = 0; i < particlesThisFrame; i++)
			CreateWaterParticle ();
		
		elapsedTime += Time.deltaTime;
		if (elapsedTime > timer)
			enabled = false;
	}
	
	private void CreateWaterParticle () {
		GameObject waterParticle = Instantiate (this.waterParticle) as GameObject;
		waterParticle.transform.parent = transform;
	}
	
	public void Enable () {
		elapsedTime = 0;
		enabled = true;
	}
}
