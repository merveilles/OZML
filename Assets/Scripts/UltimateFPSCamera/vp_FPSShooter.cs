/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSShooter.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	this class adds firearm features to a vp_FPSWeapon. it handles
//					recoil, firing rate, projectiles, muzzle flashes, shell casings,
//					and shooting sound
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

[RequireComponent(typeof(vp_FPSWeapon))]
[RequireComponent(typeof(AudioSource))]

public class vp_FPSShooter : MonoBehaviour
{

	private vp_FPSWeapon m_Weapon = null;				// the weapon affected by the shooter
	private vp_FPSCamera m_Camera = null;				// the main first person camera view

	// projectile
	public GameObject ProjectilePrefab = null;			// prefab with a mesh and projectile script
	public float ProjectileScale = 1.0f;				// scale of the projectile decal
	public float ProjectileFiringRate = 0.3f;			// delay between shots fired
	public int ProjectileCount = 1;						// amount of projectiles to fire at once
	public float ProjectileSpread = 0.0f;				// accuracy deviation in degrees (0 = spot on)
	private float m_NextAllowedFireTime = 0.0f;			// the next time firing is allowed when having recently fired a shot

	// motion
	public Vector3 MotionPositionRecoil = new Vector3(0, 0, -0.035f);	// positional force applied to weapon upon firing
	public Vector3 MotionRotationRecoil = new Vector3(-10.0f, 0, 0);	// angular force applied to weapon upon firing
	public float MotionPositionReset = 0.5f;			// how much to reset weapon to its normal position upon firing (0-1)
	public float MotionRotationReset = 0.5f;
	public float MotionPositionPause = 1.0f;			// time interval over which to freeze and fade swaying forces back in upon firing
	public float MotionRotationPause = 1.0f;

	// muzzle flash
	public Vector3 MuzzleFlashPosition = Vector3.zero;	// position of the muzzle in relation to the camera
	public Vector3 MuzzleFlashScale = Vector3.one;		// scale of the muzzleflash
	public float MuzzleFlashFadeSpeed = 0.075f;			// the amount of muzzle flash alpha to deduct each frame
	public GameObject MuzzleFlashPrefab = null;			// muzzleflash prefab with a mesh and vp_MuzzleFlash script
	private GameObject m_MuzzleFlash = null;			// the instantiated muzzle flash. one per weapon that's always there.
	private vp_MuzzleFlash m_MuzzleFlashComponent = null;	// the 'vp_MuzzleFlash' script governing muzzleflash behavior

	// shell casing
	public GameObject ShellPrefab = null;				// shell prefab with a mesh and vp_Shell script
	public float ShellScale = 1.0f;						// scale of ejected shell casings
	public Vector3 ShellEjectDirection = new Vector3(1, 1, 1);	// direction of ejected shell casing
	public Vector3 ShellEjectPosition = new Vector3(1, 0, 1);	// position of ejected shell casing in relation to camera
	public float ShellEjectVelocity = 0.2f;				// velocity of ejected shell casing
	public float ShellEjectDelay = 0.0f;				// time to wait before ejecting shell after firing (for shotguns, grenade launchers etc.)
	public float ShellEjectSpin = 0.0f;					// amount of angular rotation of the shell upon spawn

	// sound
	public AudioClip SoundFire = null;					// sound to play upon firing
	public Vector2 SoundFirePitch = new Vector2(1.0f, 1.0f);	// random pitch range for firing sound
	
	public bool Persist;


	///////////////////////////////////////////////////////////
	// properties
	///////////////////////////////////////////////////////////
	public GameObject MuzzleFlash { get { return m_MuzzleFlash; } } 

	
	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Start()
	{

		// get hold of the FPSWeapon
		m_Weapon = transform.GetComponent<vp_FPSWeapon>();

		audio.dopplerLevel = 0.0f;

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Awake()
	{

		m_Camera = transform.root.GetComponentInChildren<vp_FPSCamera>();
		if (m_Camera == null)
		{
			Debug.LogError("Camera is null.");
			return;
		}

		// instantiate muzzleflash
		if (MuzzleFlashPrefab != null)
		{
			m_MuzzleFlash = (GameObject)Object.Instantiate(MuzzleFlashPrefab,
															m_Camera.transform.position,
															m_Camera.transform.rotation);
			m_MuzzleFlash.name = transform.name + "MuzzleFlash";
			m_MuzzleFlash.transform.parent = m_Camera.transform;
		}

		RefreshSettings();

	}


	///////////////////////////////////////////////////////////
	// this method handles firing rate, recoil forces and fire
	// sounds for an FPSWeapon. you should call it from your
	// gameplay class handling player input when the player
	// presses the fire button.
	///////////////////////////////////////////////////////////
	public void Fire()
	{

		if (m_Weapon == null)
			return;

		if (Time.time < m_NextAllowedFireTime)
			return;

		m_NextAllowedFireTime = Time.time + ProjectileFiringRate;

		// return the weapon to its forward looking state by certain
		// position, rotation and velocity factors
		m_Weapon.ResetState(MotionPositionReset, MotionRotationReset,
							MotionPositionPause, MotionRotationPause);
																	  
		// add a positional and angular force to the weapon for one frame
		m_Weapon.AddForce2(MotionPositionRecoil, MotionRotationRecoil);

		// play shoot sound
		audio.pitch = Random.Range(SoundFirePitch.x, SoundFirePitch.y);
		if (audio != null)
			audio.PlayOneShot(SoundFire);
	
		// spawn projectiles
		for (int v = 0; v < ProjectileCount; v++)
		{
			if (ProjectilePrefab != null)
			{
				GameObject p = null;
				p = (GameObject)Object.Instantiate(ProjectilePrefab, m_Camera.transform.position, m_Camera.transform.rotation);
				p.transform.localScale = new Vector3(ProjectileScale, ProjectileScale, ProjectileScale);	// preset defined scale

				// apply conical spread as defined in preset
				p.transform.Rotate(0, 0, Random.Range(0, 360));									// first, rotate up to 360 degrees around z for circular spread
				p.transform.Rotate(0, Random.Range(-ProjectileSpread, ProjectileSpread), 0);		// then rotate around y with user defined deviation
			}
		}

		// spawn shell casing
		if (ShellPrefab != null)
		{
			if (ShellEjectDelay > 0.0f)
				vp_Timer.At(ShellEjectDelay, EjectShell);
			else
				EjectShell();
		}

		// show muzzle flash
		if (m_MuzzleFlashComponent != null)
			m_MuzzleFlashComponent.Shoot();

	}


	///////////////////////////////////////////////////////////
	// spawns the 'ShellPrefab' gameobject and gives it a velocity
	///////////////////////////////////////////////////////////
	private void EjectShell()
	{

		// spawn the shell
		GameObject s = null;
		s = (GameObject)Object.Instantiate(ShellPrefab, 
										m_Camera.transform.position + m_Camera.transform.TransformDirection(ShellEjectPosition),
										m_Camera.transform.rotation);
		s.transform.localScale = new Vector3(ShellScale, ShellScale, ShellScale);
		vp_Layer.Set(s.gameObject, vp_Layer.Debris);

		// send it flying
		if (s.rigidbody)
			s.rigidbody.AddForce((transform.TransformDirection(ShellEjectDirection) * ShellEjectVelocity), ForceMode.Impulse);

		// add random spin if user defined
		if (ShellEjectSpin > 0.0f)
		{
			if (Random.value > 0.5f)
				s.rigidbody.AddRelativeTorque(-Random.rotation.eulerAngles * ShellEjectSpin);
			else
				s.rigidbody.AddRelativeTorque(Random.rotation.eulerAngles * ShellEjectSpin);
		}

	}


	///////////////////////////////////////////////////////////
	// this method prevents the weapon from firing for 'seconds',
	// useful e.g. while switching weapons.
	///////////////////////////////////////////////////////////
	public void PreventFiring(float seconds)
	{

		m_NextAllowedFireTime = Time.time + seconds;

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


	///////////////////////////////////////////////////////////
	// this method is called to reset various shooter settings,
	// typically after creating or loading a shooter
	///////////////////////////////////////////////////////////
	public void RefreshSettings()
	{

		// update muzzle flash position, scale and fadespeed from preset
		if (m_MuzzleFlash != null)
		{
			m_MuzzleFlash.transform.localPosition = MuzzleFlashPosition;
			m_MuzzleFlash.transform.localScale = MuzzleFlashScale;
			m_MuzzleFlashComponent = (vp_MuzzleFlash)m_MuzzleFlash.GetComponent("vp_MuzzleFlash");
			if (m_MuzzleFlashComponent != null)
				m_MuzzleFlashComponent.FadeSpeed = MuzzleFlashFadeSpeed;
		}

	}


}

	