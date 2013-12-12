using UnityEngine;
using System.Collections;

public class AudioBank : MonoBehaviour {
	
	[SerializeField]
	private AudioClip elephantSound;
	[SerializeField]
	private AudioClip elephantWater;
	
	public void PlayElephantSound () {
		audio.PlayOneShot (elephantSound);
	}
	
	public void PlayElephantWater() {
		audio.PlayOneShot(elephantWater);
	}
}
