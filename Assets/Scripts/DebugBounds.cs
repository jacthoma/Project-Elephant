using UnityEngine;
using System.Collections;

public class DebugBounds : MonoBehaviour {
	
	public bool displayScreenInformation = true;
	public bool displayFloorInformation = true;
	public bool displayGuideInformation = true;
	public bool displayTextInformation = true;
	public float screenSurfaceDepth = 0.11F;
	public float floorDepth = 0.2F;
	public Color FloorColor;
	
	Camera focusedCamera;
	Vector3 topLeft;
	Vector3 topRight;
	Vector3 bottomLeft;
	Vector3 bottomRight;
	
	void OnDrawGizmos() {
		focusedCamera = camera;
		
		topRight = focusedCamera.ScreenToWorldPoint(new Vector3(focusedCamera.pixelWidth, focusedCamera.pixelHeight, focusedCamera.nearClipPlane));
		bottomRight  = focusedCamera.ScreenToWorldPoint(new Vector3(focusedCamera.pixelWidth, 0, focusedCamera.nearClipPlane));
		topLeft = focusedCamera.ScreenToWorldPoint(new Vector3(0, focusedCamera.pixelHeight, focusedCamera.nearClipPlane));
		bottomLeft = focusedCamera.ScreenToWorldPoint(new Vector3(0, 0, focusedCamera.nearClipPlane)) ;
		Vector3 offset = (focusedCamera.transform.forward * screenSurfaceDepth);
		
		if(displayScreenInformation){
	        Gizmos.color = Color.blue;
			
			if(displayTextInformation)
				Gizmos.DrawIcon((topLeft + bottomRight) * 0.5F, "CloseClippingPlane.png");
			
	        Gizmos.DrawLine(topLeft, topRight);
			Gizmos.DrawLine(topRight, bottomRight);
			Gizmos.DrawLine(bottomRight, bottomLeft);
			Gizmos.DrawLine(bottomLeft, topLeft);
			
			Gizmos.color = Color.cyan;
			
			if(displayTextInformation)
				Gizmos.DrawIcon(((topLeft + offset) + (bottomRight + offset)) * 0.5F, "ScreenSpace.png");
			
			Gizmos.DrawLine(topLeft + offset, topRight + offset);
			Gizmos.DrawLine(topRight + offset, bottomRight + offset);
			Gizmos.DrawLine(bottomRight + offset, bottomLeft + offset);
			Gizmos.DrawLine(bottomLeft + offset, topLeft + offset);
		}
		
		if(displayGuideInformation){
			Gizmos.color = Color.yellow;
			drawFlatForwardLine(bottomLeft + offset, 1);
			drawFlatForwardLine(bottomRight + offset, 1);
			drawLineFromCameraPastPoint(topLeft, 1);
			drawLineFromCameraPastPoint(topRight, 1);
			drawLineFromCameraPastPoint(bottomLeft, 1);
			drawLineFromCameraPastPoint(bottomRight, 1);
			
			drawFlatForwardLine(slidePointDownCameraEdge(bottomLeft, floorDepth), 1);
			drawFlatForwardLine(slidePointDownCameraEdge(bottomRight, floorDepth), 1);
		}
		
		if(displayFloorInformation){
			Gizmos.color = FloorColor;
			drawFloorExample(slidePointDownCameraEdge(bottomRight, floorDepth), slidePointDownCameraEdge(bottomLeft, floorDepth), 1);
		}
    }
	
	Vector3 slidePointDownCameraEdge(Vector3 v, float length){
		Ray r = new Ray(v, v - focusedCamera.transform.position);
		return r.GetPoint(length);
	}
	
	void drawLineFromCameraPastPoint(Vector3 v, float length){
		Ray r = new Ray(v, v - focusedCamera.transform.position);
		Gizmos.DrawLine(v, r.GetPoint(length));
		
		if(displayTextInformation)
			Gizmos.DrawIcon(r.GetPoint(length), "EdgeOfViewDistance.png");
		else
			Gizmos.DrawSphere(r.GetPoint(length), 0.02F);
			
	}
	
	void drawFloorExample(Vector3 left, Vector3 right, float length){
		Ray lr = new Ray(left, new Vector3(focusedCamera.transform.forward.x, 0, focusedCamera.transform.forward.z));
		Ray rr = new Ray(right, new Vector3(focusedCamera.transform.forward.x, 0, focusedCamera.transform.forward.z));
		Vector3 center = (left + right + lr.GetPoint(length) + rr.GetPoint(length)) * 0.25F;
		Gizmos.DrawCube(center , new Vector3(Vector3.Distance(left, right), 0.01F, Vector3.Distance(left, lr.GetPoint(length))));
		if(displayTextInformation)
			Gizmos.DrawIcon(center, "TableTopHeight.png");
	}
	
	void drawFlatForwardLine(Vector3 v, float length){
		Ray r = new Ray(v, new Vector3(focusedCamera.transform.forward.x, 0, focusedCamera.transform.forward.z));
		Gizmos.DrawLine(v, r.GetPoint(length));
	}
}
