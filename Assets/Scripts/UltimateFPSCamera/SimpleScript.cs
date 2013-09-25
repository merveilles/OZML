/////////////////////////////////////////////////////////////////////////////////
//
//	SimpleScript.cs
//
//	description:	a very basic scripting implementation of Ultimate FPS Camera
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

[RequireComponent(typeof(vp_FPSController))]

public class SimpleScript : MonoBehaviour
{
	// components
	vp_FPSCamera m_Camera = null;
	vp_FPSController m_Controller = null;

	// crosshair texture
	Texture m_ImageCrosshair = null;
	public bool Guns = false;


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Start()
	{

		// get hold of the FPSCamera and FPSController components
		// attached to this game object
		m_Camera = gameObject.GetComponentInChildren<vp_FPSCamera>();
		m_Controller = gameObject.GetComponent<vp_FPSController>();

		// load crosshair texture
		//m_ImageCrosshair = (Texture)Resources.Load("Examples/Images/Crosshair");

		// load 'modern shooter' camera and controller presets
		//m_Camera.Load("Precam");
		//m_Controller.Load("Precon");

		// try to set weapon 1
		//if ( Guns && m_Camera.WeaponCount > 0)
		//	SetWeapon(1);
		
	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Update()
	{
		// classic 'WASD' first person controls
		if (Input.GetKey(KeyCode.W)) { m_Controller.MoveForward(); }
		if (Input.GetKey(KeyCode.S)) { m_Controller.MoveBack(); }
		if (Input.GetKey(KeyCode.A)) { m_Controller.MoveLeft(); }
		if (Input.GetKey(KeyCode.D)) { m_Controller.MoveRight(); }

		// jump on 'SPACE' (the presets regulate jump force)
		if (Input.GetKeyDown(KeyCode.Space))
			m_Controller.Jump();

		// toggle weapons on '1-4' buttons
		if ( Guns && (m_Camera.WeaponCount > 0) && (Input.GetKeyDown(KeyCode.Alpha1)))
			SetWeapon(1);
		if ( Guns && (m_Camera.WeaponCount > 1) && (Input.GetKeyDown(KeyCode.Alpha2)))
			SetWeapon(2);
		if ( Guns && (m_Camera.WeaponCount > 2) && (Input.GetKeyDown(KeyCode.Alpha3)))
			SetWeapon(3);
		
		// end application on 'ESC'
		if (Input.GetKey(KeyCode.Escape)) { Application.Quit(); }

		// handle shooting
		if ( Guns && Input.GetMouseButton(0))
		{
			vp_FPSShooter shooter = m_Camera.CurrentWeapon.GetComponent<vp_FPSShooter>();
			if (shooter != null)
				shooter.Fire();
		}
	}


	///////////////////////////////////////////////////////////
	// sets a new weapon and optionally refreshes zoom and springs
	///////////////////////////////////////////////////////////
	void SetWeapon(int weapon, bool smooth = false)
	{

		m_Camera.SetWeapon(weapon);

		if (!smooth)
		{
			m_Camera.CurrentWeapon.SnapSprings();
			m_Camera.CurrentWeapon.SnapPivot();
			m_Camera.CurrentWeapon.SnapZoom();
		}

	}

	
	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void OnGUI()
	{
		//DrawCrosshair();
	}

		
	///////////////////////////////////////////////////////////
	// draws a traditional FPS style crosshair
	///////////////////////////////////////////////////////////
	void DrawCrosshair()
	{

		GUI.color = new Color(1, 1, 1, 0.8f);
		GUI.DrawTexture(new Rect((Screen.width / 2) - 11, (Screen.height / 2) - 11, 22, 22), m_ImageCrosshair);
		GUI.color = Color.white;

	}
}

