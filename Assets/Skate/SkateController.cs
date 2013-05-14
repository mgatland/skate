using UnityEngine;
using System.Collections;

[RequireComponent (typeof (CharacterController))]

public class SkateController : MonoBehaviour {
	
	public Transform leftFoot;
	public Transform rightFoot;
	
	//settings:
	private float rotationSpeed = 160f;
	private float turningDeadZone = 0.2f;
	private float acceleration = 10f;
	private float deceleration = 30f;
	private float fallingAcceleration = 1f;
	private float jumpVelocity = 0.3f;
	private float slopeBoostFactor = 1f; //amount that going uphill boosts your jump velocity
	private float maxNaturalSpeed = 250f;
	private float delayAfterLandingBeforeJumping = 0.1f;
	
	private CharacterController controller;
	
	//state:
	private float movementSpeed = 0f;
	private CollisionFlags collisionFlags;
	private float fallingSpeed = 0f;
	private bool wasGroundedLastFrame = false;
	private float heightInPreviousFrame = 0f;
	PreviousVelocityTracker slopeSpeedTracker = new PreviousVelocityTracker();
	private bool isReallyGrounded = false;
	private Transform shadow;
	private float timeOnGround = 0;
	
	// Use this for initialization
	void Start () {
		controller = GetComponent<CharacterController>();
		shadow = transform.Find("Shadow");
	}
	
	public void Win() {
		//foreach (ParticleSystem p in GetComponentsInChildren<ParticleSystem>()) {
		//	p.gameObject.SetActive(true);	
		//}
		//win stuff happens
	}
	
	// Update is called once per frame
	void Update () {
		
		updateGroundedState ();

	updateAnimation ();
		
		updateRotation ();
		updateVelocity ();
		
		
		RaycastHit groundUnderMe = new RaycastHit();
		Physics.Raycast(new Ray(transform.position, Vector3.down), out groundUnderMe, 2000f);
		Vector3 shadowXZ = Vector3.Lerp(leftFoot.transform.position, rightFoot.transform.position, 0.5f);
		shadow.transform.position = new Vector3(shadowXZ.x, groundUnderMe.point.y + 0.1f, shadowXZ.z);
	}
	
	float getThrottle() {
		return Input.GetAxisRaw("Vertical");	
	}
	
	void updateAnimation ()
	{
		Animator animator = GetComponentInChildren<Animator>();
		
		animator.SetFloat("throttle", getThrottle());
		animator.SetBool("isGrounded", IsGrounded());
		
		if (!IsGrounded()) { 
			//when we're grounded, this keeps it's previous value
			//so we can remember, because fallingSpeed gets reset when we touch the ground
			animator.SetBool("needsRecovery", fallingSpeed < -0.45f);	
		}
		
		//TODO: replace all these conditions with "if the running animation is playing"
		if (IsGrounded() && movementSpeed > 0f && IsGrounded() && getThrottle() > 0) {
			animator.speed = 1f + Mathf.Max(movementSpeed / 100f -1f, 0f);
		} else {
			animator.speed = 1f;
		}
	}

	private void updateGroundedState ()
	{
		//Am I grounded? Should I snap to the ground?
		//previously used (collisionFlags & CollisionFlags.CollidedBelow) != 0;
		//to try: controller.isGrounded
		
		wasGroundedLastFrame = IsGrounded();
		float snapDistance = wasGroundedLastFrame ? 1.5f : 1.1f;
		
		if (fallingSpeed > 0) { //we're moving upwards (jumping)
			snapDistance = controller.height / 2; //basically don''t snap.
		}
		
		RaycastHit hitInfo = new RaycastHit();
		if (Physics.Raycast(new Ray(transform.position, Vector3.down), out hitInfo, snapDistance)) {
			isReallyGrounded = true;
			//the magic number works around a bug where the player would move down 0.08 units when stationary...
			transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y + controller.height / 2 + 0.08f, hitInfo.point.z);
			timeOnGround += Time.deltaTime;
			if (wasGroundedLastFrame) {
				slopeSpeedTracker.addSpeed(transform.position.y - heightInPreviousFrame);
			}
			heightInPreviousFrame = transform.position.y;
		} else {
			isReallyGrounded = false;
			timeOnGround = 0f;
		}
	}

	private void updateRotation ()
	{
		float rawTurning = Input.GetAxisRaw("Horizontal");
		if (Mathf.Abs(rawTurning) > turningDeadZone) {
			Vector3 targetDirection = transform.right * rawTurning;
			Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, rotationSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);
			transform.rotation = Quaternion.LookRotation(newDirection);
		}
	}
	
	private void updateVelocity ()
	{
		
		//HitWall() is really crude - it doesn't tell us whether we hit head-on or just brushed a wall
		//We hack around that by using the character controller's velocity (the distance we moved last frame)
		//a head on collision would have set our velocity to zero
		//a grazing touch wouldn't have affected our velocity much.
		if (HitWall()) {
			Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
			movementSpeed = Mathf.Min(movementSpeed, horizontalVelocity.magnitude);
		}
		
		float rawThrottle = getThrottle();
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
			slopeSpeedTracker.clear();
		} else {
			fallingSpeed = 0f;
		}
		
		//jumping
		bool jumpButton = Input.GetButton("Jump");
		if (jumpButton && IsGrounded() && timeOnGround > delayAfterLandingBeforeJumping) {
			fallingSpeed = jumpVelocity;
			fallingSpeed += slopeSpeedTracker.getMax() * slopeBoostFactor; //If I was running up or down a slope.
			wasGroundedLastFrame = false;
			slopeSpeedTracker.clear();
		}
	
		movement += Vector3.up * fallingSpeed;
		
		
		collisionFlags = controller.Move(movement);
	}
	
	private bool IsGrounded () {
		return isReallyGrounded;
	}
	
	private bool HitWall() {
		return (collisionFlags & CollisionFlags.Sides) != 0;
	}
	
	private class PreviousVelocityTracker {
		//hacks for maintaining momentum when launching off a ramp
		//TODO: this is very framerate-dependent, fix it somehow.
		
		private static int numberOfFramesToTrack = 5;
		private int valuesStored = 0;
		private int index = 0;
		private float[] storedValues = new float[numberOfFramesToTrack];
		
		public void addSpeed(float speed) {
			//Debug.Log("yspeed: " + (int)(speed* 100));
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
		
		//Actually get the 2nd highest, to reduce glitches.
		//(when we go up a step, for one frame we get a very high height increase)
		public float getMax() {
			float max = 0;
			float max2 = 0;
			for (int i = 0; i < valuesStored; i++) {
				if (storedValues[i] > max){
					max2 = max;
					max = storedValues[i];
				} else if (storedValues[i] > max2) {
					max2 = storedValues[i];	
				}
			}
			return max2;
		}
	}
}
