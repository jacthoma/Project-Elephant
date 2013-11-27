using UnityEngine;
using System.Collections;

public class ElephantControls : MonoBehaviour {
	
	/*
	 * Variables used up Lerp
	 **/
	private Vector3 startLocation;
	private Vector3 endLocation;
	public float speed = 0.5F;
	private float startTime;
	private float pathLength;
	private bool startPathing = false;
	private bool isPathing = false;
	/**/
	
	private bool tracking = false;
	private Vector3 newPosition;
//	private Vector3 finalPosition;
	
	public ZSStylusSelector stylus;
	
	// Use this for initialization
	void Start () {
		//start stuff here
		endLocation = this.transform.position;
	}
	
	// Update is called once per frame
	void Update () { 
		Vector3 curPosition = this.transform.eulerAngles;
		this.transform.eulerAngles = new Vector3(curPosition.x, curPosition.y, 0);

		/*
		if (stylus.GetButtonDown (0)) {
			tracking = true;
			isPathing = false;
		}
		if (stylus.GetButtonUp (0))
		{
			if (tracking) {
				//this.transform.position = 
				//	new Vector3 (stylus.HoverPoint.x, stylus.HoverPoint.y, stylus.HoverPoint.z);
				//Vector3 position = new Vector3 (transform.position.x, 0, transform.position.z);
				
				//rigidbody.velocity += (stylus.HoverPoint - transform.position).normalized * 
					//Vector3.Distance (stylus.HoverPoint, transform.position);
				
			}
			tracking = false;
		}
		*/
		
		/*
		 * Move the elephant to a location pointed at by the stylus
		 * 
		 * Author: James Thomas
		 */
		if (stylus.GetButtonDown (1))
		{
			startPathing = true;
		}
		if (stylus.GetButtonUp (1))
		{
			if(startPathing && (this.gameObject != stylus.HoverObject)){
				startLocation = this.transform.position;
				endLocation = stylus.HoverPoint;
				startTime = Time.time;
				pathLength = Vector3.Distance (this.transform.position, endLocation);
				isPathing = true;
				startPathing = false;
				transform.rotation = Quaternion.Euler (0, -Mathf.Atan2 ((startLocation - endLocation).z, (startLocation - endLocation).x) * 180 / Mathf.PI - 90, 0);
			}
		}
		if(isPathing){
			float distanceCovered = (Time.time - startTime)*speed;
			float fractionOfPath = distanceCovered/pathLength;
			this.transform.position = Vector3.Lerp (startLocation, endLocation, fractionOfPath);
			if(startLocation == endLocation)
			{
				isPathing = false;
			}
		}
		/* End 
		 */
	}
	
	void OnCollisionEnter (Collision col) {
		if (col.gameObject.name == "ElephantCalf_fbx") {
			Physics.IgnoreCollision(col.collider, this.collider);
		}
	}
	
	void OnCollisionExit (Collision col) {
		
	}
}
