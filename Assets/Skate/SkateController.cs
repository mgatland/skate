using UnityEngine;
using System.Collections;

[RequireComponent (typeof (CharacterController))]

public class SkateController : MonoBehaviour {
	
	//settings:
	private float rotationSpeed = 160f;
	private float turningDeadZone = 0.2f;
	private float acceleration = 10f;
	private float deceleration = 30f;
	private float fallingAcceleration = 1f;
	private float jumpVelocity = 0.3f;
	private float slopeBoostFactor = 0.01f; //amount that going uphill boosts your jump velocity
	private float maxNaturalSpeed = 250f;
	
	private CharacterController controller;
	
	//state:
	private float movementSpeed = 0f;
	private CollisionFlags collisionFlags;
	private float fallingSpeed = 0f;
	private bool wasGroundedLastFrame = false;
	PreviousVelocityTracker slopeSpeedTracker = new PreviousVelocityTracker();
	
	
	// Use this for initialization
	void Start () {
		controller = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () {
		updateRotation ();
		updateVelocity ();
	}

	void updateRotation ()
	{
		float rawTurning = Input.GetAxisRaw("Horizontal");
		if (Mathf.Abs(rawTurning) > turningDeadZone) {
			Vector3 targetDirection = transform.right * rawTurning;
			Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, rotationSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);
			transform.rotation = Quaternion.LookRotation(newDirection);
		}
	}

	void updateVelocity ()
	{
		
		//HitWall() is really crude - it doesn't tell us whether we hit head-on or just brushed a wall
		//We hack around that by using the character controller's velocity (the distance we moved last frame)
		//a head on collision would have set our velocity to zero
		//a grazing touch wouldn't have affected our velocity much.
		if (HitWall()) {
			Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
			movementSpeed = Mathf.Min(movementSpeed, horizontalVelocity.magnitude);
		}
		
		float rawThrottle = Input.GetAxisRaw("Vertical");
		if (rawThrottle > 0) {
			if (movementSpeed < maxNaturalSpeed) {
				float speedIncrease = acceleration * rawThrottle * Time.deltaTime;
				movementSpeed = Mathf.Min(movementSpeed + speedIncrease, maxNaturalSpeed);
			}
		} else {
			movementSpeed = Mathf.Max(movementSpeed - deceleration * Time.deltaTime, 0);
		}
		
		Vector3 movement = transform.forward * movementSpeed;
		movement *= Time.deltaTime;
		
		//falling
		if (!IsGrounded()) {
			if (wasGroundedLastFrame) {
				fallingSpeed += slopeSpeedTracker.getMax() * slopeBoostFactor;
			} else {
				fallingSpeed -= fallingAcceleration * Time.deltaTime;
			}
			wasGroundedLastFrame = false;
			slopeSpeedTracker.clear();
		} else {
			slopeSpeedTracker.addSpeed(controller.velocity.y);	
			fallingSpeed = -0.01f;
			wasGroundedLastFrame = true;
		}
		
		//jumping
		bool jumpButton = Input.GetButton("Jump");
		if (jumpButton && IsGrounded()) {
			fallingSpeed = jumpVelocity;
			fallingSpeed += slopeSpeedTracker.getMax() * slopeBoostFactor; //If I was running up or down a slope.
			wasGroundedLastFrame = false;
			slopeSpeedTracker.clear();
		}
	
		movement += Vector3.up * fallingSpeed;
		
		
		collisionFlags = controller.Move(movement);
	}
	
	//maybe should use controller.isGrounded instead
	bool IsGrounded () {
		return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
	}
	
	bool HitWall() {
		return (collisionFlags & CollisionFlags.Sides) != 0;
	}
	
	private class PreviousVelocityTracker {
		//hacks for maintaining momentum when launching off a ramp
		//TODO: this is very framerate-dependent, fix it somehow.
		
		private static int numberOfFramesToTrack = 15;
		private int valuesStored = 0;
		private int index = 0;
		private float[] storedValues = new float[numberOfFramesToTrack];
		
		public void addSpeed(float speed) {
			storedValues[index] = speed;
			index++;
			if (index == numberOfFramesToTrack) {
				index = 0;
			}
			if (valuesStored < numberOfFramesToTrack) {
				valuesStored++;
			}
		}
		
		public void clear() {
			index = 0;
			valuesStored = 0;
		}
		
		public float getMax() {
			float max = 0;
			for (int i = 0; i < valuesStored; i++) {
				if (storedValues[i] > max){
					max = storedValues[i];
				}
			}
			return max;
		}
	}
}
