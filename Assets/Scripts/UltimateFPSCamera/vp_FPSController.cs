/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSController.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	a first person controller class with tweakable physics parameters
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class vp_FPSController : MonoBehaviour
{

	CharacterController Controller = null;

	// motor
	public float MotorAcceleration = 0.5f;
	public float MotorDamping = 0.1f;
	public float MotorJumpForce = 0.25f;
	public float MotorAirSpeed = 0.7f;
	public float MotorSlopeSpeedUp = 1.0f;
	public float MotorSlopeSpeedDown = 1.0f;
	private float m_MoveFactor = 1.0f;
	private Vector3 m_MotorThrottle = Vector3.zero;
	private bool m_MoveForward = false;
	private bool m_MoveLeft = false;
	private bool m_MoveRight = false;
	private bool m_MoveBack = false;

	// physics
	public float PhysicsForceDamping = 0.05f;			// damping of external forces
	public float PhysicsPushForce = 5.0f;				// mass for pushing around rigidbodies
	public float PhysicsGravityModifier = 0.2f;			// affects fall speed
	public float PhysicsWallBounce = 0.0f;				// how much to bounce off walls
	private Vector3 m_ExternalForce = Vector3.zero;		// velocity from external forces (explosion knockback, jump pads, rocket packs)

	// fall impact variables
	private bool m_WasGroundedLastFrame = true;
	private float m_FallSpeed = 0.0f;
	private float m_LastFallSpeed = 0.0f;
	private float m_HighestFallSpeed = 0.0f;
	private float m_FallImpact = 0.0f;
	
	// misc
	private float m_Delta;
	public bool Persist;


	//////////////////////////////////////////////////////////
	// properties
	//////////////////////////////////////////////////////////
	public float FallImpact { get { return m_FallImpact; } }	// TIP: check this property every frame to apply falling damage or play impact sounds


	//////////////////////////////////////////////////////////
	// 
	//////////////////////////////////////////////////////////
	void Awake()
	{
		Controller = gameObject.GetComponent<CharacterController>();
	}


	//////////////////////////////////////////////////////////
	// 
	//////////////////////////////////////////////////////////
	void Update()
	{

		// treat delta as 1 at an application target framerate of 60
		m_Delta = (Time.deltaTime * 60.0f);
		
		UpdateMoves();

		Vector3 moveDirection = Vector3.zero;

		// --- dampen forces ---

		// dampen motor force, but only apply vertical damping
		// if going up, or jumping will be damped too
		float motorDamp = (1 + (MotorDamping * m_Delta));
		m_MotorThrottle.x /= motorDamp;
		m_MotorThrottle.y = (m_MotorThrottle.y > 0.0f) ? (m_MotorThrottle.y / motorDamp) : m_MotorThrottle.y;
		m_MotorThrottle.z /= motorDamp;

		// dampen external force, but only apply vertical damping
		// if going up, or gravity will be damped too
		float physDamp = (1 + (PhysicsForceDamping * m_Delta));
		m_ExternalForce.x /= physDamp;
		m_ExternalForce.y = (m_ExternalForce.y > 0.0f) ? (m_ExternalForce.y / physDamp) : m_ExternalForce.y;
		m_ExternalForce.z /= physDamp;

		// --- apply gravity ---
		if (Controller.isGrounded && m_FallSpeed <= 0)
			m_FallSpeed = ((Physics.gravity.y * (PhysicsGravityModifier * 0.002f)));	// grounded
		else
		{
			m_FallSpeed += ((Physics.gravity.y * (PhysicsGravityModifier * 0.002f)) * m_Delta);	// flying
		}

		// --- detect falling impact ---
		m_FallImpact = 0.0f;
		if (m_FallSpeed < m_LastFallSpeed)
			m_HighestFallSpeed = m_FallSpeed;
		if (Controller.isGrounded && !m_WasGroundedLastFrame)
		{
			m_FallImpact = -m_HighestFallSpeed;
			m_HighestFallSpeed = 0.0f;
		}
		m_WasGroundedLastFrame = Controller.isGrounded;
		m_LastFallSpeed = m_FallSpeed;
		
		// --- move controller ---
		moveDirection += m_ExternalForce * m_Delta;
		moveDirection += m_MotorThrottle * m_Delta;
		moveDirection.y += m_FallSpeed * m_Delta;
		
		// apply anti-bump offset. this pushes the controller towards the
		// ground to prevent the character from "bumpety-bumping" when
		// walking down slopes or stairs.
		float antiBumpOffset = 0.0f;
		if (Controller.isGrounded && m_MotorThrottle.y <= 0.0f)
		{
			antiBumpOffset = Mathf.Max(Controller.stepOffset,
										new Vector3(moveDirection.x, 0, moveDirection.z).magnitude); 
			moveDirection -= antiBumpOffset * Vector3.up;
		}

		// do some prediction in order to absorb external forces upon collision
		Vector3 predictedXZ = Vector3.Scale((Controller.transform.localPosition + moveDirection), new Vector3(1, 0, 1));
		float predictedY = Controller.transform.localPosition.y + moveDirection.y;

		Controller.Move(moveDirection);

		// if we lost grounding during this move, undo anti-bump offset to
		// make the fall smoother
		if (antiBumpOffset != 0.0f && !Controller.isGrounded && m_WasGroundedLastFrame)
			Controller.Move(antiBumpOffset * Vector3.up);
		
		Vector3 actualXZ = Vector3.Scale(Controller.transform.localPosition, new Vector3(1, 0, 1));
		float actualY = Controller.transform.localPosition.y;

		// --- absorb forces on collision ---
		
		// if the controller didn't end up at the predicted position,
		// some external object has blocked its way, so absorb the
		// movement forces to avoid getting stuck at walls.
		if (predictedXZ != actualXZ)
			AbsorbHorisontalForce(actualXZ - predictedXZ);


		// absorb forces that push the controller upward, in order not to
		// get stuck in ceilings.
		// NOTE: no need to absorb downward collision forces. there is
		// always a ground collision imposed by gravity.
		if ((predictedY > actualY) && (m_ExternalForce.y > 0 || m_MotorThrottle.y > 0))
			AbsorbUpForce(actualY - predictedY);

	}


	///////////////////////////////////////////////////////////
	// transforms user input data into motion on the controller
	///////////////////////////////////////////////////////////
	private void UpdateMoves()
	{

		// if we're moving diagonally, slow down using square root of 2
		if ((m_MoveForward && m_MoveLeft) || (m_MoveForward && m_MoveRight) ||
			(m_MoveBack && m_MoveLeft) || (m_MoveBack && m_MoveRight))
			m_MoveFactor = 0.70710678f;
		else
			m_MoveFactor = 1.0f;

		// if on the ground, modify movement depending on ground slope
		if (Controller.isGrounded)
			m_MoveFactor *= GetSlopeMultiplier();

		// if in the air, apply air damping
		else		
			m_MoveFactor *= MotorAirSpeed;

		// keep framerate independent
		m_MoveFactor *= m_Delta;

		// apply speeds to motor
		if (m_MoveForward)
			m_MotorThrottle += transform.TransformDirection(Vector3.forward * (MotorAcceleration * 0.1f) * m_MoveFactor);

		if (m_MoveBack)
			m_MotorThrottle += transform.TransformDirection(Vector3.back * (MotorAcceleration * 0.1f) * m_MoveFactor);

		if (m_MoveLeft)
			m_MotorThrottle += transform.TransformDirection(Vector3.left * (MotorAcceleration * 0.1f) * m_MoveFactor);

		if (m_MoveRight)
			m_MotorThrottle += transform.TransformDirection(Vector3.right * (MotorAcceleration * 0.1f) * m_MoveFactor);

		// reset input for next frame
		m_MoveForward = false;
		m_MoveLeft = false;
		m_MoveRight = false;
		m_MoveBack = false;

	}


	///////////////////////////////////////////////////////////
	// this method calculates a controller velocity multiplier
	// depending on ground slope. at 'MotorSlopeSpeed' 1.0,
	// velocity in slopes will be kept roughly the same as on
	// flat ground. values lower or higher than 1 will make the
	// controller slow down / speed up on slopes, respectively.
	///////////////////////////////////////////////////////////
	private float GetSlopeMultiplier()
	{

		// calculate slope factor from the controller's vertical velocity
		float slopeMultiplier = 1.0f - (Controller.velocity.normalized.y / 1.41421356f);

		if (Mathf.Abs(1 - slopeMultiplier) < 0.01f)
		{
			// ground is essentially flat, so don't do anything
			slopeMultiplier = 1.0f;
		}
		else if (slopeMultiplier > 1.0f)
		{
			// ground is sloping up
			if (MotorSlopeSpeedDown == 1.0f)
			{
				// 1.0 means 'no change' so we'll alter the value to get
				// roughly the same velocity as if ground was flat
				slopeMultiplier = 1.0f / slopeMultiplier;
				slopeMultiplier *= 1.2f;
			}
			else
				slopeMultiplier *= MotorSlopeSpeedDown;	// apply user defined multiplier
		}
		else
		{
			// ground is sloping down
			if (MotorSlopeSpeedUp == 1.0f)
			{
				// 1.0 means 'no change' so we'll alter the value to get
				// roughly the same velocity as if ground was flat
				slopeMultiplier *= 1.2f;
			}
			else
				slopeMultiplier *= MotorSlopeSpeedUp;	// apply user defined multiplier
		}

		return slopeMultiplier;

	}


	///////////////////////////////////////////////////////////
	// adds external force to the controller, such as explosion
	// knockback, wind or jump pads
	///////////////////////////////////////////////////////////
	public void AddForce(Vector3 force)
	{
		m_ExternalForce += force;
	}

	public void AddForce(float x, float y, float z)
	{
		AddForce(new Vector3(x, y, z));
	}

	
	///////////////////////////////////////////////////////////
	// applies the jump force upwards. returns false if jump
	// failed, so you know whether to play some jump sound
	///////////////////////////////////////////////////////////
	public bool Jump()
	{

		if(!Controller.isGrounded)
			return false;

		m_MotorThrottle += new Vector3(0, MotorJumpForce, 0);

		return true;

	}

	
	///////////////////////////////////////////////////////////
	// simple solution for pushing rigid bodies. the push force
	// of the FPSController is used to determine how much we
	// can affect the other object, and we don't affect
	// falling objects.
	///////////////////////////////////////////////////////////
	private void OnControllerColliderHit(ControllerColliderHit hit)
	{

		Rigidbody body = hit.collider.attachedRigidbody;

		if (body == null || body.isKinematic)
			return;

		if (hit.moveDirection.y < -0.3F)
			return;

		float difference = PhysicsPushForce / body.mass;
		
		Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
		body.velocity = (pushDir * difference);

	}


	///////////////////////////////////////////////////////////
	// since the forces acting on this controller are not part of
	// the real physics simulation, they will not get properly
	// absorbed if it runs into things, which can lead to strange 
	// behaviors such as getting stuck at walls. this method
	// removes motor and external velocity from the FPSController
	// given an impact vector. it operates only horisontally.
	///////////////////////////////////////////////////////////
	private void AbsorbHorisontalForce(Vector3 impact)
	{

		impact *= 1 + PhysicsWallBounce;

		// figure out how much of the current x/z velocity is external
		// and motor velocity, respectively
		float externalShare = Vector3.Scale(m_ExternalForce, new Vector3(1, 0, 1)).sqrMagnitude;
		float motorShare = Vector3.Scale(m_MotorThrottle, new Vector3(1, 0, 1)).sqrMagnitude;
		float fullSpeed = (externalShare + motorShare);
		externalShare = externalShare / fullSpeed;
		motorShare = motorShare / fullSpeed;

		// remove the appropriate share of the impact from external
		// and motor velocites

		m_ExternalForce.x += (impact.x * externalShare) / m_Delta;
		m_ExternalForce.z += (impact.z * externalShare / m_Delta);

		m_MotorThrottle.x += (impact.x * motorShare) / m_Delta;
		m_MotorThrottle.z += (impact.z * motorShare) / m_Delta;

	}


	///////////////////////////////////////////////////////////
	// this method does the same thing as 'AbsorbHorisontalForce',
	// for vertical forces such as jumping, explosions or jump pads.
	// NOTE: this calculation won't realistically deflect y force
	// into horisontal force. i.e. should you need the controller
	// to get pushed sideways by a tilted ceiling, you may want to
	// handle this using collision logic instead.
	///////////////////////////////////////////////////////////
	private void AbsorbUpForce(float impact)
	{

		float externalShare =	m_ExternalForce.y;
		float motorShare =		m_MotorThrottle.y;
		float fullSpeed = (externalShare + motorShare);
		externalShare = externalShare / fullSpeed;
		motorShare = motorShare / fullSpeed;

		m_ExternalForce.y += (impact * externalShare) / m_Delta;
		m_MotorThrottle.y += (impact * motorShare) / m_Delta;

	}


	///////////////////////////////////////////////////////////
	// move methods for external use
	///////////////////////////////////////////////////////////
	public void MoveForward()	{	m_MoveForward = true;	}
	public void MoveBack()		{	m_MoveBack = true;	}
	public void MoveLeft()		{	m_MoveLeft = true;	}
	public void MoveRight()		{	m_MoveRight = true;	}


	///////////////////////////////////////////////////////////
	// loads a preset from the resources folder, for cleaner syntax
	///////////////////////////////////////////////////////////
	public void Load(string path)
	{
		vp_ComponentPreset.LoadFromResources(this, path);
	}


	///////////////////////////////////////////////////////////
	// sets the position of the character controller
	///////////////////////////////////////////////////////////
	public void SetPosition(Vector3 position)
	{
		transform.position = position;
	}

}

