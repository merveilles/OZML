/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSWeapon.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	animates a weapon transform using springs, bob and perlin noise,
//					in response to user input.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

public class vp_FPSWeapon : MonoBehaviour
{

	// character controller of the parent gameobject
	CharacterController Controller = null;
	
	public float RenderingZoomDamping = 0.5f;
	private float m_FinalZoomTime = 0.0f;
	
	// weapon rendering
	
	public float RenderingFieldOfView = 60.0f;
	public Vector2 RenderingClippingPlanes = new Vector2(0.01f, 10.0f);

	// weapon position
	public Vector3 PositionOffset = new Vector3(0.15f, -0.15f, -0.15f);
	public float PositionSpringStiffness = 0.01f;
	public float PositionSpringDamping = 0.25f;
	public float PositionKneeling = 0.1f;
	public float PositionFallRetract = 1.0f;
	public float PositionPivotSpringStiffness = 0.01f;
	public float PositionPivotSpringDamping = 0.25f;
	public float PositionSpring2Stiffness = 0.95f;
	public float PositionSpring2Damping = 0.25f;
	public Vector3 PositionWalkSlide = new Vector3(0.5f, 0.75f, 0.5f);
	private vp_Spring m_PositionSpring = null;		// spring for player motion (shake, falling impact, sway, bob etc.)
	private vp_Spring m_PositionSpring2 = null;		// spring for secondary forces like recoil (typically with stiffer spring settings)
	private vp_Spring m_PositionPivotSpring = null;
	private GameObject m_WeaponCamera = null;
	private GameObject m_WeaponGroup = null;
	private GameObject m_Pivot = null;
	public Vector3 PositionPivot = Vector3.zero;
	
	// weapon rotation
	public Vector3 RotationOffset = Vector3.zero;
	public float RotationSpringStiffness = 0.01f;
	public float RotationSpringDamping = 0.25f;
	public float RotationSpring2Stiffness = 0.95f;
	public float RotationSpring2Damping = 0.25f;
	public Vector3 RotationLookSway = new Vector3(0.7f, 1.0f, 0.0f);
	public Vector3 RotationStrafeSway = new Vector3(0.3f, 1.0f, 1.5f);
	public Vector3 RotationFallSway = new Vector3(1.0f, -0.5f, -3.0f);
	public float RotationSlopeSway = 0.5f;
	private vp_Spring m_RotationSpring = null;		// spring for player motion (falling impact, sway, bob etc.)
	private vp_Spring m_RotationSpring2 = null;	// spring for secondary forces like recoil (typically with stiffer spring settings)

	// weapon shake
	public float ShakeSpeed = 0.05f;
	public Vector3 ShakeAmplitude = new Vector3(0.25f, 0.0f, 2.0f);
	private Vector3 m_Shake = Vector3.zero;

	// weapon bob
	public Vector4 BobRate = new Vector4(0.45f, 0.9f, 0.0f, 0.0f);			// TIP: y should be (x * 2) for a nice classic curve of motion
	public Vector4 BobAmplitude = new Vector4(0.5f, 0.35f, 0.0f, 0.0f);		// TIP: make x & y negative to invert the curve
	public float BobMaxInputVelocity = 1000;
	private float m_LastBobSpeed = 0.0f;

	// time
	private float m_Delta = 0.0f;

	// component persist state
	public bool Persist;


	//////////////////////////////////////////////////////////
	// properties
	//////////////////////////////////////////////////////////
	public GameObject WeaponCamera { get { return m_WeaponCamera; } }


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Awake()
	{

		Controller = transform.root.GetComponent<CharacterController>();

		transform.eulerAngles = Vector3.zero;

		// setup weapon camera
		m_WeaponCamera = new GameObject(name + "WeaponCamera");
		m_WeaponCamera.transform.parent = transform.parent;
		m_WeaponCamera.AddComponent(typeof(Camera));
		m_WeaponCamera.transform.localPosition = Vector3.zero;
		m_WeaponCamera.transform.localEulerAngles = Vector3.zero;
		m_WeaponCamera.camera.clearFlags = CameraClearFlags.Depth;
		m_WeaponCamera.camera.cullingMask = (1 << vp_Layer.Weapon);
		m_WeaponCamera.camera.depth = 1;
		m_WeaponCamera.camera.farClipPlane = 100;
		m_WeaponCamera.camera.nearClipPlane = 0.01f;
		m_WeaponCamera.camera.fov = 60;

		// set up m_WeaponGroup
		m_WeaponGroup = new GameObject(name + "Transform");
		m_WeaponGroup.transform.parent = transform.parent;
		m_WeaponGroup.transform.localPosition = PositionOffset;

		// reposition weapon under m_WeaponGroup and rename it
		transform.parent = m_WeaponGroup.transform;
		transform.localPosition = Vector3.zero;
		m_WeaponGroup.transform.localEulerAngles = RotationOffset;

		if (collider != null)
			collider.enabled = false;

		// put weapon object in the 'WeaponLayer', so the
		// weapon camera can render it individually
		gameObject.layer = vp_Layer.Weapon;
		foreach (Transform t in transform)
		{

			// all the children too
			t.gameObject.layer = vp_Layer.Weapon;

			// rename transforms at root level, because if the main
			// gameobject has transforms with identical names, unity
			// will override sync rotations across gameobjects
			if (t.parent == transform)
				t.name = t.name + "_";
		}

		// setup weapon pivot
		m_Pivot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		m_Pivot.name = "Pivot";
		m_Pivot.collider.enabled = false;
		m_Pivot.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		m_Pivot.transform.parent = m_WeaponGroup.transform;
		m_Pivot.transform.localPosition = Vector3.zero;
		m_Pivot.layer = vp_Layer.Weapon;
		m_Pivot.gameObject.active = false;
		Material material = new Material(Shader.Find("Transparent/Diffuse"));
		material.color = new Color(0, 0, 1, 0.5f);
		m_Pivot.renderer.material = material;

		m_PositionSpring = new vp_Spring(m_WeaponGroup.gameObject.transform, vp_Spring.TransformType.Position);
		m_PositionSpring.RestState = PositionOffset;

		m_PositionPivotSpring = new vp_Spring(transform, vp_Spring.TransformType.Position);
		m_PositionPivotSpring.RestState = PositionPivot;

		m_PositionSpring2 = new vp_Spring(gameObject.transform, vp_Spring.TransformType.PositionAdditive);
		m_PositionSpring2.MinVelocity = 0.00001f;

		m_RotationSpring = new vp_Spring(m_WeaponGroup.gameObject.transform, vp_Spring.TransformType.Rotation);
		m_RotationSpring.RestState = RotationOffset;

		m_RotationSpring2 = new vp_Spring(m_WeaponGroup.gameObject.transform, vp_Spring.TransformType.RotationAdditive);
		m_RotationSpring2.MinVelocity = 0.00001f;

		RefreshSettings();

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Update()
	{

		// treat delta as 1 at an application target framerate of 60
		m_Delta = (Time.deltaTime * 60.0f);

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void FixedUpdate()
	{
		
		// zoom happens here for framerate independence
		if (m_FinalZoomTime > Time.time)
			UpdateZoom();

		// update fall impact, swaying, bobbing and earthquakes
		UpdateForces();

		m_PositionSpring.FixedUpdate();
		m_PositionPivotSpring.FixedUpdate();
		m_PositionSpring2.FixedUpdate();
		m_RotationSpring.FixedUpdate();
		m_RotationSpring2.FixedUpdate();

		// update camera and weapon shakes
		UpdateShakes();
		
	}


	///////////////////////////////////////////////////////////
	// calls various force handling methods once per FixedUpdate
	///////////////////////////////////////////////////////////
	private void UpdateForces()
	{

		// NOTE: falling impact is detected by the camera, so this
		// doesn't have to be done in two places. though the camera
		// still calls the weapon's 'ApplyFallImpact' method below

		// handle sway
		DoWeaponSwaying(Controller.velocity);

		// handle weapon bob
		if (Controller.isGrounded)
		{
			DoWeaponBob(Controller.velocity.sqrMagnitude);
		}
		else
		{
			DoWeaponBob(0);
		}
		
	}


	///////////////////////////////////////////////////////////
	// applies positional and angular force to the weapon
	///////////////////////////////////////////////////////////
	public void AddForce(Vector3 positional, Vector3 angular)
	{

		m_PositionSpring.AddForce(positional);
		m_RotationSpring.AddForce(angular);

	}


	public void AddForce(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot)
	{
		AddForce(new Vector3(xPos, yPos, zPos), new Vector3(xRot, yRot, zRot));
	}


	///////////////////////////////////////////////////////////
	// applies positional and angular force to the weapon. the
	// typical use for this method is applying recoil force.
	///////////////////////////////////////////////////////////
	public void AddForce2(Vector3 positional, Vector3 angular)
	{

		m_PositionSpring2.AddForce(positional);
		m_RotationSpring2.AddForce(angular);

	}


	public void AddForce2(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot)
	{
		AddForce2(new Vector3(xPos, yPos, zPos), new Vector3(xRot, yRot, zRot));
	}


	///////////////////////////////////////////////////////////
	// applies a falling impact to the weapon position spring
	///////////////////////////////////////////////////////////
	public void ApplyFallImpact(float impact)
	{

		m_PositionSpring.AddForce(new Vector3(0, impact * -PositionKneeling, 0));
	
	}


	///////////////////////////////////////////////////////////
	// interpolates to the target FOV value
	///////////////////////////////////////////////////////////
	private void UpdateZoom()
	{

		RenderingZoomDamping = Mathf.Max(RenderingZoomDamping, 0.01f);
		float zoom = 1 - ((m_FinalZoomTime - Time.time) / RenderingZoomDamping);
		m_WeaponCamera.gameObject.camera.fov = Mathf.SmoothStep(m_WeaponCamera.gameObject.camera.fov,
																RenderingFieldOfView, zoom);

	}


	///////////////////////////////////////////////////////////
	// interpolates to the target FOV using 'WeaponRenderingZoomDamping'
	// as interval
	///////////////////////////////////////////////////////////
	public void Zoom()
	{
		m_FinalZoomTime = Time.time + RenderingZoomDamping;
	}


	///////////////////////////////////////////////////////////
	// instantly sets camera to the target FOV
	///////////////////////////////////////////////////////////
	public void SnapZoom()
	{

		m_WeaponCamera.gameObject.camera.fov = RenderingFieldOfView;

	}

	
	///////////////////////////////////////////////////////////
	// updates the procedural shaking of the weapon.
	// this is a purely aesthetic motion to breathe life into the first
	// person arm / weapon. if one wanted to expand on this, one could
	// alternate between higher & lower speeds and amplitudes.
	///////////////////////////////////////////////////////////
	private void UpdateShakes()
	{

		// apply weapon shake
		if (ShakeSpeed != 0.0f)
		{
			m_Shake = Vector3.Scale(vp_SmoothRandom.GetVector3Centered(ShakeSpeed),
									ShakeAmplitude) * (60 * Time.smoothDeltaTime);
			m_RotationSpring.AddForce(m_Shake);
		}

	}


	///////////////////////////////////////////////////////////
	// speed should be the magnitude speed of the character
	// controller. if controller has no ground contact, '0.0f'
	// should be passed and the bob will fade to a halt.
	///////////////////////////////////////////////////////////
	private void DoWeaponBob(float speed)
	{

		if (BobAmplitude == Vector4.zero || BobRate == Vector4.zero)
			return;

		speed = Mathf.Min(speed, BobMaxInputVelocity);

		// if zero is passed, this means we should just fade out
		// the last stored speed value
		if (speed == 0)
			speed = m_LastBobSpeed * 0.93f;

		float upAmp = (speed * (BobAmplitude.y * -0.0001f));
		float upBob = (Mathf.Cos(Time.time * (BobRate.y * 10.0f))) * upAmp;

		float sideAmp = (speed * (BobAmplitude.x * 0.0001f));
		float sideBob = (Mathf.Cos(Time.time * (BobRate.x * 10.0f))) * sideAmp;

		float rollAmp = (speed * (BobAmplitude.z * 0.0001f));
		float rollBob = (Mathf.Cos(Time.time * (BobRate.z * 10.0f))) * rollAmp;

		float forwAmp = (speed * (BobAmplitude.w * 0.0001f));
		float forwBob = (Mathf.Cos(Time.time * (BobRate.w * 10.0f))) * forwAmp;

		m_RotationSpring.AddForce(new Vector3(upBob, sideBob, rollBob));
		m_PositionSpring.AddForce(new Vector3(0, 0, forwBob));

		m_LastBobSpeed = speed;

	}


	///////////////////////////////////////////////////////////
	// applies swaying forces to the weapon in response to user
	// input and character controller motion. this includes
	// mouselook, falling, strafing and walking.
	///////////////////////////////////////////////////////////
	private void DoWeaponSwaying(Vector3 velocity)
	{

		Vector2 mouseMove = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
		Vector3 localVelocity = transform.InverseTransformDirection(velocity / 60);
		
		// --- pitch & yaw rotational sway ---

		// sway the weapon transform using mouse input and weapon weight
		m_RotationSpring.AddForce(new Vector3(
			(mouseMove.y * (RotationLookSway.y * 0.025f)) / m_Delta,	// multiply with 0.025f to keep the working values within a handier range
			(mouseMove.x * (RotationLookSway.x * -0.025f)) / m_Delta,
			mouseMove.x * (RotationLookSway.z * -0.025f)));
		
		// --- falling ---

		// rotate weapon while falling. this will take effect in reverse when being elevated,
		// for example walking up a ramp. however, the weapon will only rotate around the z
		// vector while going down (falling)
		Vector3 fallSway = (RotationFallSway * (velocity.y * 0.005f));
		// if grounded, optionally reduce fallsway
		if (Controller.isGrounded)
			fallSway *= RotationSlopeSway;
		m_RotationSpring.AddForce(new Vector3(
			fallSway.x,
			fallSway.y,
			Mathf.Max(0, fallSway.z)));
		
		// drag weapon towards ourselves
		m_PositionSpring.AddForce(new Vector3(
			0,
			0,
			-Mathf.Abs((velocity.y) * (PositionFallRetract * 0.000025f))));
		
		// --- weapon strafe & walk slide ---
		// WeaponPositionWalkSlide x will slide sideways when strafing
		// WeaponPositionWalkSlide y will slide down when strafing (it can't push up)
		// WeaponPositionWalkSlide z will slide forward or backward when walking
		m_PositionSpring.AddForce(new Vector3(
			(localVelocity.x * (PositionWalkSlide.x * 0.0016f)),
			-(Mathf.Abs(localVelocity.x * (PositionWalkSlide.y * 0.0016f))),
			(-localVelocity.z * (PositionWalkSlide.z * 0.0016f))));
		
		// --- weapon strafe rotate ---
		// WeaponRotationStrafeSway x will rotate up when strafing (it can't push down)
		// WeaponRotationStrafeSway y will rotate sideways when strafing
		// WeaponRotationStrafeSway z will twist weapon around the forward vector when strafing
		m_RotationSpring.AddForce(new Vector3(
			-Mathf.Abs(localVelocity.x * (RotationStrafeSway.x * 0.16f)),
			-(localVelocity.x * (RotationStrafeSway.y * 0.16f)),
			localVelocity.x * (RotationStrafeSway.z * 0.16f)));

	}
	

	///////////////////////////////////////////////////////////
	// use this method to force the weapon back to its default
	// state by the 'reset' values (0-1). can be used to make
	// a weapon always fire in the forward direction regardless
	// of current weapon angles. optional 'pauseTime' parameters
	// may be provided, instantly freezing any forces acting on
	// the primary position and rotation springs and easing
	// them back in over 'pauseTime' interval in seconds.
	///////////////////////////////////////////////////////////
	public void ResetState(float positionReset, float rotationReset, float positionPauseTime = 0.0f, float rotationPauseTime = 0.0f)
	{

		m_PositionSpring.State = Vector3.Lerp(m_PositionSpring.State, m_PositionSpring.RestState, positionReset);
		m_RotationSpring.State = Vector3.Lerp(m_RotationSpring.State, m_RotationSpring.RestState, rotationReset);

		if (positionPauseTime != 0.0f)
			m_PositionSpring.ForceVelocityFadeIn(positionPauseTime);

		if (rotationPauseTime != 0.0f)
			m_RotationSpring.ForceVelocityFadeIn(rotationPauseTime);
		
	}
	

	///////////////////////////////////////////////////////////
	// this method is called to reset various weapon settings,
	// typically after creating or loading a weapon
	///////////////////////////////////////////////////////////
	public void RefreshSettings()
	{

		if (!Application.isPlaying)
			return;

		m_PositionSpring.Stiffness =
			new Vector3(PositionSpringStiffness, PositionSpringStiffness, PositionSpringStiffness);
		m_PositionSpring.Damping = Vector3.one -
			new Vector3(PositionSpringDamping, PositionSpringDamping, PositionSpringDamping);

		m_PositionPivotSpring.Stiffness =
			new Vector3(PositionPivotSpringStiffness, PositionPivotSpringStiffness, PositionPivotSpringStiffness);
		m_PositionPivotSpring.Damping = Vector3.one -
			new Vector3(PositionPivotSpringDamping, PositionPivotSpringDamping, PositionPivotSpringDamping);

		m_PositionSpring2.Stiffness =
			new Vector3(PositionSpring2Stiffness, PositionSpring2Stiffness, PositionSpring2Stiffness);
		m_PositionSpring2.Damping = Vector3.one -
			new Vector3(PositionSpring2Damping, PositionSpring2Damping, PositionSpring2Damping);

		m_RotationSpring.Stiffness =
			new Vector3(RotationSpringStiffness, RotationSpringStiffness, RotationSpringStiffness);
		m_RotationSpring.Damping = Vector3.one -
			new Vector3(RotationSpringDamping, RotationSpringDamping, RotationSpringDamping);

		m_RotationSpring2.Stiffness =
			new Vector3(RotationSpring2Stiffness, RotationSpring2Stiffness, RotationSpring2Stiffness);
		m_RotationSpring2.Damping = Vector3.one -
			new Vector3(RotationSpring2Damping, RotationSpring2Damping, RotationSpring2Damping);

		m_RotationSpring.RestState = RotationOffset;
		m_PositionSpring.RestState = PositionOffset - PositionPivot;
		m_PositionSpring2.RestState = Vector3.zero;
		m_PositionPivotSpring.RestState = PositionPivot;
		m_RotationSpring2.RestState = Vector3.zero;

		m_WeaponCamera.camera.nearClipPlane = RenderingClippingPlanes.x;
		m_WeaponCamera.camera.farClipPlane = RenderingClippingPlanes.y;

		Zoom();

	}

		
	///////////////////////////////////////////////////////////
	// this method is called to reset the pivot of the weapon
	// model, typically after creating or loading a camera or
	// a weapon
	///////////////////////////////////////////////////////////
	public void SnapPivot()
	{

		if (m_PositionSpring != null)
		{
			m_PositionSpring.RestState = PositionOffset - PositionPivot;
			m_PositionSpring.State = PositionOffset - PositionPivot;
		}
		m_WeaponGroup.transform.localPosition = PositionOffset - PositionPivot;

		if (m_PositionPivotSpring != null)
		{
			m_PositionPivotSpring.RestState = PositionPivot;
			m_PositionPivotSpring.State = PositionPivot;
		}
		transform.localPosition = PositionPivot;

	}


	///////////////////////////////////////////////////////////
	// toggles visibility of the weapon model pivot for editor
	// purposes
	///////////////////////////////////////////////////////////
	public void SetPivotVisible(bool visible)
	{
		m_Pivot.gameObject.active = visible;
	}
	

	///////////////////////////////////////////////////////////
	// resets all the springs to their default positions, i.e.
	// for when loading a new camera or switching a weapon
	///////////////////////////////////////////////////////////
	public void SnapSprings()
	{


		if (m_PositionSpring != null)
		{
			m_PositionSpring.RestState = PositionOffset - PositionPivot;
			m_PositionSpring.State = PositionOffset - PositionPivot;
			m_PositionSpring.Stop();
		}
		m_WeaponGroup.transform.localPosition = PositionOffset - PositionPivot;

		if (m_PositionPivotSpring != null)
		{
			m_PositionPivotSpring.RestState = PositionPivot;
			m_PositionPivotSpring.State = PositionPivot;
			m_PositionPivotSpring.Stop();
		}
		transform.localPosition = PositionPivot;

		if (m_PositionSpring2 != null)
		{
			m_PositionSpring2.RestState = Vector3.zero;
			m_PositionSpring2.State = Vector3.zero;
			m_PositionSpring2.Stop();
		}

		if (m_RotationSpring != null)
		{
			m_RotationSpring.RestState = RotationOffset;
			m_RotationSpring.State = RotationOffset;
			m_RotationSpring.Stop();
		}

		if (m_RotationSpring2 != null)
		{
			m_RotationSpring2.RestState = Vector3.zero;
			m_RotationSpring2.State = Vector3.zero;
			m_RotationSpring2.Stop();
		}

	}


	///////////////////////////////////////////////////////////
	// stops all the springs
	///////////////////////////////////////////////////////////
	public void StopSprings()
	{

		if (m_PositionSpring != null)
			m_PositionSpring.Stop();

		if (m_PositionPivotSpring != null)
			m_PositionPivotSpring.Stop();

		if (m_PositionSpring2 != null)
			m_PositionSpring2.Stop();

		if (m_RotationSpring != null)
			m_RotationSpring.Stop();

		if (m_RotationSpring2 != null)
			m_RotationSpring2.Stop();

	}


	///////////////////////////////////////////////////////////
	// helper method to load a preset from the resources folder,
	// and refresh settings. for cleaner syntax
	///////////////////////////////////////////////////////////
	public void Load(string path)
	{
		vp_ComponentPreset.LoadFromResources(this, path);
		RefreshSettings();
	}


}

	