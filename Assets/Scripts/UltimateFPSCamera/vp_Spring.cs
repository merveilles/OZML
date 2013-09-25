/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Spring.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	a simple but powerful spring logic for transform manipulation
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

public class vp_Spring
{

	// a spring can operate on either of three transform types:
	// position, rotation or scale
	public TransformType Modifier = TransformType.Position;
	public enum TransformType
	{
		Position,
		PositionAdditive,
		Rotation,
		RotationAdditive,
		Scale,
		ScaleAdditive
	}

	// a delegate is used to modify the parent transform
	// with the current motion values
	private delegate void TransformDelegate();
	private TransformDelegate m_TransformFunc;

	// the 'State' variable dictates either position, rotation or scale
	// of this TransformModifier's gameobject, as defined by m_TransformType
	public Vector3 State = Vector3.zero;

	// properties can't be serialized, so this variable is used
	// to detect inspector changes to the transform type
	private TransformType m_CurrentTransformType = TransformType.Position;

	// the current velocity of the spring, as resulting from added forces
	private Vector3 m_Velocity = Vector3.zero;

	// the static equilibrium of this spring
	public Vector3 RestState = Vector3.zero;

	// mechanical strength of the spring
	public Vector3 Stiffness = new Vector3(0.5f, 0.5f, 0.5f);

	// 'Damping' makes spring velocity wear off as it
	// approaches its rest state
	public Vector3 Damping = new Vector3(0.75f, 0.75f, 0.75f);

	// force velocity fadein variables
	private float m_VelocityFadeInCap = 1.0f;
	private float m_VelocityFadeInEndTime = 0.0f;
	private float m_VelocityFadeInLength = 0.0f;

	// transform limitations
	public float MaxVelocity = 10000.0f;
	public float MinVelocity = 0.0000001f;
	public Vector3 MaxState = new Vector3(10000, 10000, 10000);
	public Vector3 MinState = new Vector3(-10000, -10000, -10000);
	
	// transform & property
	private Transform m_Transform;
	public Transform Transform
	{
		set
		{
			m_Transform = value;
			RefreshTransformType();
		}
	}


	///////////////////////////////////////////////////////////
	// constructor
	///////////////////////////////////////////////////////////
	public vp_Spring(Transform transform, TransformType modifier)
	{
		m_Transform = transform;
		Modifier = modifier;
		RefreshTransformType();
	}


	///////////////////////////////////////////////////////////
	// this should be called from a monobehaviour's FixedUpdate,
	// i.e. 60 times per second
	///////////////////////////////////////////////////////////
	public void FixedUpdate()
	{

		// handle forced velocity fadein
		if (m_VelocityFadeInEndTime > Time.time)
			m_VelocityFadeInCap = Mathf.Clamp01(1 - ((m_VelocityFadeInEndTime - Time.time) / m_VelocityFadeInLength));
		else
			m_VelocityFadeInCap = 1.0f;

		// detect modifier changes
		if (Modifier != m_CurrentTransformType)
			RefreshTransformType();

		m_TransformFunc();

	}
	

	///////////////////////////////////////////////////////////
	// applies spring state to the transform's local position
	///////////////////////////////////////////////////////////
	void Position()
	{
		Calculate();
		m_Transform.localPosition = State;
	}


	///////////////////////////////////////////////////////////
	// additively applies spring state to the transform's position
	// 'additive' is for using a spring on an object which already
	// has another spring acting upon it (in order not to block
	// out the motion from the existing spring)
	///////////////////////////////////////////////////////////
	void PositionAdditive()
	{
		Calculate();
		m_Transform.localPosition += State;
	}


	///////////////////////////////////////////////////////////
	// applies spring state to the transform's local euler angles
	///////////////////////////////////////////////////////////
	void Rotation()
	{
		Calculate();
		m_Transform.localEulerAngles = State;
	}


	///////////////////////////////////////////////////////////
	// additively applies spring state to the transform's local
	// euler angles
	///////////////////////////////////////////////////////////
	void RotationAdditive()
	{
		Calculate();
		m_Transform.localEulerAngles += State;
	}


	///////////////////////////////////////////////////////////
	// applies spring state to the transform's local scale
	///////////////////////////////////////////////////////////
	void Scale()
	{
		Calculate();
		m_Transform.localScale = State;
	}


	///////////////////////////////////////////////////////////
	// additively applies spring state to the transform's local
	// scale
	///////////////////////////////////////////////////////////
	void ScaleAdditive()
	{
		Calculate();
		m_Transform.localScale += State;
	}


	///////////////////////////////////////////////////////////
	// this method sets the appropriate delegate for the trans-
	// formation and syncs 'State' with the parent transform
	///////////////////////////////////////////////////////////
	private void RefreshTransformType()
	{

		switch (Modifier)
		{
			case TransformType.Position:
				State = m_Transform.localPosition;
				m_TransformFunc = new TransformDelegate(Position);
				break;
			case TransformType.Rotation:
				State = m_Transform.localEulerAngles;
				m_TransformFunc = new TransformDelegate(Rotation);
				break;
			case TransformType.Scale:
				State = m_Transform.localScale;
				m_TransformFunc = new TransformDelegate(Scale);
				break;
			case TransformType.PositionAdditive:
				State = m_Transform.localPosition;
				m_TransformFunc = new TransformDelegate(PositionAdditive);
				break;
			case TransformType.RotationAdditive:
				State = m_Transform.localEulerAngles;
				m_TransformFunc = new TransformDelegate(RotationAdditive);
				break;
			case TransformType.ScaleAdditive:
				State = m_Transform.localScale;
				m_TransformFunc = new TransformDelegate(ScaleAdditive);
				break;
		}

		m_CurrentTransformType = Modifier;

		RestState = State;

	}


	///////////////////////////////////////////////////////////
	// performs the spring physics calculations
	///////////////////////////////////////////////////////////
	private void Calculate()
	{

		if (State == RestState)
			return;

		Vector3 dist = (RestState - State);						// get distance to rest state
		m_Velocity += Vector3.Scale(dist, Stiffness);			// add distance * stiffness to velocity

		m_Velocity = (Vector3.Scale(m_Velocity, Damping));		// dampen velocity

		// clamp velocity to maximum
		m_Velocity = Vector3.ClampMagnitude(m_Velocity, MaxVelocity);

		// apply velocity, or stop if velocity is below minimum
		if (Mathf.Abs(m_Velocity.sqrMagnitude) > (MinVelocity * MinVelocity))
			Move();
		else
			Reset();

	}

	
	///////////////////////////////////////////////////////////
	// adds external velocity to the spring in one frame
	// NOTE: sometimes you may need to multiply 'force' with delta
	// time before calling AddForce, in order for the spring to be
	// framerate independent (even if the spring is updated in
	// FixedUpdate)
	///////////////////////////////////////////////////////////
	public void AddForce(Vector3 force)
	{

		force *= m_VelocityFadeInCap;
		m_Velocity += force;
		m_Velocity = Vector3.ClampMagnitude(m_Velocity, MaxVelocity);
		Move();

	}

	public void AddForce(float x, float y, float z)
	{
		AddForce(new Vector3(x, y, z));
	}


	///////////////////////////////////////////////////////////
	// adds velocity to the state and clamps state between min
	// and max values
	///////////////////////////////////////////////////////////
	private void Move()
	{
		State += m_Velocity;
		State = new Vector3(Mathf.Clamp(State.x, MinState.x, MaxState.x),
							Mathf.Clamp(State.y, MinState.y, MaxState.y),
							Mathf.Clamp(State.z, MinState.z, MaxState.z));
	}


	///////////////////////////////////////////////////////////
	// stops spring velocity and resets state to the static
	// equilibrium
	///////////////////////////////////////////////////////////
	public void Reset()
	{

		m_Velocity = Vector3.zero;
		State = RestState;

	}


	///////////////////////////////////////////////////////////
	// stops spring velocity
	///////////////////////////////////////////////////////////
	public void Stop()
	{
		m_Velocity = Vector3.zero;
	}


	///////////////////////////////////////////////////////////
	// instantly strangles any forces added to the spring,
	// gradually easing them back in over 'seconds'.
	// this is useful when you need a spring to freeze up for a
	// brief amount of time, then slowly relaxing back to normal.
	///////////////////////////////////////////////////////////
	public void ForceVelocityFadeIn(float seconds)
	{

		m_VelocityFadeInLength = seconds;
		m_VelocityFadeInEndTime = Time.time + seconds;
		m_VelocityFadeInCap = 0.0f;

	}


}

