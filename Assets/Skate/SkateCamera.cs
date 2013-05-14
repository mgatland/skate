using UnityEngine;
using System.Collections;

public class SkateCamera : MonoBehaviour {
	
	private Transform target;
	private Transform cameraTransform;
	
	//settings
	private float cameraOffsetX = -6;
	private float cameraOffsetZ = 2;
	private Vector3 viewOffset = new Vector3(0f, 1f, 0f);
	
	// Use this for initialization
	void Start () {
		target = transform;
		cameraTransform = Camera.main.transform;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		Vector3 cameraPosition = target.position + target.forward * cameraOffsetX + target.up * cameraOffsetZ;
		cameraTransform.position = cameraPosition;
		
		cameraTransform.LookAt(target.position + viewOffset);
	}
}
