/////////////////////////////////////////////////////////////////////////////////
//
//	DemoScript.cs
//
//	description:	a walkthrough demo of Ultimate FPS Camera, with instructive
//					code examples
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

[RequireComponent(typeof(vp_FPSController))]

public class DemoScript : MonoBehaviour
{

	// components
	vp_FPSCamera m_Camera = null;
	vp_FPSController m_Controller = null;

	// states
	bool m_DemoMode = true;					// if true the demo will be run, otherwise just a level with FPS controls
	int m_CurrentScreen = 1;				// current demo screen
	int m_FadeToScreen = 0;					// demo screen that we are currently transitioning to
	float m_OriginalWeaponFOV = 0.0f;		// stores the original FOV value for when zooming back out
	float m_OriginalCameraFOV = 0.0f;
	float m_GenericZoomInCameraFOV = 35.0f;		// how much to zoom in the camera when holding the zoom mouse button
	float m_GenericZoomInWeaponFOV = 20.0f;		// how much to zoom in the weapon when holding the zoom mouse button
	bool m_ZoomedIn = false;
	string m_LastLoadedCamera = "";			// used for quick-and dirty crouching functionality
	float m_LastInputTime = 0.0f;			// used for detecting how long the user has idled
	enum FadeState
	{
		None,
		FadeOut,
		FadeIn
	}
	FadeState m_FadeState = FadeState.None;

	// timers
	vp_Timer m_SwitchWeaponTimer = null;		// these are used for creating small delays when weapon switching
	vp_Timer m_ShowWeaponTimer = null;

	// GUI
	bool m_FirstFrame = true;				// for making specific settings on the first frame of a new tutorial screen
	bool m_WeaponLayerToggle = false;		// state of the weapon layer toggle button
	bool m_DrawCrosshair = false;
	float m_TextAlpha = 1.0f;				// current alpha of the demo text. may be fading out etc.
	float m_FullScreenTextAlpha = 1.0f;	
	float m_FadeSpeed = 0.03f;				// used as speed of all transitions
	int m_ButtonSelection = 0;				// last pressed button in the various demo screens
	float m_BigArrowFadeAlpha = 1.0f;
	float m_ExtForcesButtonColumnClickTime = 0.0f;			// time when a button was last pressed in the 'EXTERNAL FORCES' demo screen
	float m_ExtForcesButtonColumnArrowY = -100.0f;			// y position of 'EXTERNAL FORCES' arrow
	float m_ExtForcesButtonColumnArrowFadeoutTime = 0.0f;	// varying fadeout time of the 'EXTERNAL FORCES' arrow
	bool m_EditorPreviewSectionExpanded = true;				// some screens have a bottom left editor preview screenshot section
	float m_EditorPreviewScreenshotTextAlpha = 0.0f;
	float m_GlobalAlpha = 0.35f;

	// styles, used mainly for aligning the various texts
	GUIStyle m_LabelStyle = null;
	GUIStyle m_UpStyle = null;
	GUIStyle m_DownStyle = null;
	GUIStyle m_CenterStyle = null;
	bool m_StylesInitialized = false;		// styles will only be set up once
	
	// positions
	Vector3 m_MouseLookPos = new Vector3(-8.093015f, 21.08f, 3.416737f);
	Vector3 m_OverviewPos = new Vector3(1.246535f, 33.08f, 21.43753f);
	Vector3 m_StartPos = new Vector3(-18.14881f, 21.08f, -24.16859f);
	Vector3 m_WeaponLayerPos = new Vector3(-19.43989f, 17.08f, 2.10474f);
	Vector3 m_ForcesPos = new Vector3(-8.093015f, 21.08f, 3.416737f);
	Vector3 m_MechPos = new Vector3(0.02941191f, 1.08f, -93.50691f);
	Vector3 m_DrunkPos = new Vector3(18.48685f, 21.08f, 24.05441f);
	Vector3 m_SniperPos = new Vector3(0.8841875f, 33.08f, 21.3446f);
	Vector3 m_OldSchoolPos = new Vector3(25.88745f, 1.08f, 23.08822f);
	Vector3 m_AstronautPos = new Vector3(20.0f, 20.0f, 16.0f);
	Vector3 m_UnlockPosition = Vector3.zero;

	// angles
	Vector2 m_MouseLookAngle = new Vector2(33.10683f, 0);
	Vector2 m_OverviewAngle = new Vector2(224, 28.89369f);
	Vector2 m_PerspectiveAngle = new Vector2(223, 27);
	Vector2 m_StartAngle = new Vector2(0, 0);
	Vector2 m_WeaponLayerAngle = new Vector2(-90, 0);
	Vector2 m_ForcesAngle = new Vector2(33.10683f, -167);
	Vector2 m_MechAngle = new Vector3(180, 0);
	Vector2 m_DrunkAngle = new Vector3(-90, 0);
	Vector2 m_SniperAngle = new Vector2(180, 20);
	Vector2 m_OldSchoolAngle = new Vector2(180, 0);
	Vector2 m_AstronautAngle = new Vector2(269.5f, 0);
	
	// textures
	Texture m_ImageAllParams = null;
	Texture m_ImageEditorPreview = null;
	Texture m_ImageEditorPreviewShow = null;
	Texture m_ImageCameraMouse = null;
	Texture m_ImagePresetDialogs = null;
	Texture m_ImageShooter = null;
	Texture m_ImageWeaponPosition = null;
	Texture m_ImageWeaponPerspective = null;
	Texture m_ImageWeaponPivot = null;
	Texture m_ImageEditorScreenshot = null;
	Texture m_ImageLeftArrow = null;
	Texture m_ImageRightArrow = null;
	Texture m_ImageCheck = null;
	Texture m_ImageLeftPointer = null;
	Texture m_ImageRightPointer = null;
	Texture m_ImageUpPointer = null;
	Texture m_ImageCrosshair = null;
	Texture m_ImageFullScreen = null;

	// this variable can be used to test framerate indepencence for
	// various features. NOTE: will only work in the editor.
	bool m_SimulateLowFPS = false;

	// sound
	AudioSource m_AudioSource = null;
	public AudioClip m_StompSound = null;
	public AudioClip m_EarthquakeSound = null;
	public AudioClip m_ExplosionSound = null;


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Start()
	{

		// get hold of the FPSCamera and FPSController components
		// attached to this game object
		m_Camera = gameObject.GetComponentInChildren<vp_FPSCamera>();
		m_Controller = gameObject.GetComponent<vp_FPSController>();

		// load textures
		if (m_DemoMode)
		{
			LoadCamera("Examples/ImmobileCamera");
			m_Camera.SetWeapon(1);
			m_ImageAllParams = (Texture)Resources.Load("Examples/Images/AllParams");
			m_ImageEditorPreview = (Texture)Resources.Load("Examples/Images/EditorPreview");
			m_ImageEditorPreviewShow = (Texture)Resources.Load("Examples/Images/EditorPreviewShow");
			m_ImageCameraMouse = (Texture)Resources.Load("Examples/Images/CameraMouse");
			m_ImagePresetDialogs = (Texture)Resources.Load("Examples/Images/PresetDialogs");
			m_ImageShooter = (Texture)Resources.Load("Examples/Images/Shooter");
			m_ImageWeaponPosition = (Texture)Resources.Load("Examples/Images/WeaponPosition");
			m_ImageWeaponPerspective = (Texture)Resources.Load("Examples/Images/WeaponRendering");
			m_ImageWeaponPivot = (Texture)Resources.Load("Examples/Images/WeaponPivot");
			m_ImageEditorScreenshot = (Texture)Resources.Load("Examples/Images/EditorScreenshot");
			m_ImageLeftArrow = (Texture)Resources.Load("Examples/Images/LeftArrow");
			m_ImageRightArrow = (Texture)Resources.Load("Examples/Images/RightArrow");
			m_ImageCheck = (Texture)Resources.Load("Examples/Images/Check");
			m_ImageLeftPointer = (Texture)Resources.Load("Examples/Images/LeftPointer");
			m_ImageRightPointer = (Texture)Resources.Load("Examples/Images/RightPointer");
			m_ImageUpPointer = (Texture)Resources.Load("Examples/Images/UpPointer");
			m_ImageCrosshair = (Texture)Resources.Load("Examples/Images/Crosshair");
			m_ImageFullScreen = (Texture)Resources.Load("Examples/Images/Fullscreen");

			// on small screen resolutions the editor preview screenshot
			// panel is minimized by default, otherwise expanded
			if (Screen.width < 1024)
				m_EditorPreviewSectionExpanded = false;

			// add an audio source to the camera, for playing various demo sounds
			m_AudioSource = (AudioSource)m_Camera.gameObject.AddComponent("AudioSource");
		
		}

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

		// zoom in using middle and right mouse button
		// (allowed if on the 'EXAMPLES' screen, but not for the
		// presets 'system off', 'sniper breath' or 'old school')
		if (m_CurrentScreen == 2 && m_ButtonSelection != 0 && m_ButtonSelection != 4 && m_ButtonSelection != 9)
		{
			if (Input.GetMouseButton(2) || Input.GetMouseButton(1))
				ZoomIn();
			else
				ZoomOut();
		}		

		// jump on 'SPACE' (the presets regulate jump force)
		if (Input.GetKeyDown(KeyCode.Space))
			m_Controller.Jump();

		// allow crouching on 'C' if on the 'EXAMPLES' screen and
		// wielding the gun or machine gun
		if (m_CurrentScreen == 2 && m_ButtonSelection > 0 && m_ButtonSelection < 3)
		{
			if (Input.GetKeyDown(KeyCode.C))
				DoCrouch();
			if (Input.GetKeyUp(KeyCode.C))
				DontCrouch();
		}

		// toggle fullscreen on 'F'
		if (Input.GetKeyDown(KeyCode.F))
			Screen.fullScreen = !Screen.fullScreen;

		// end application on 'ESC'
		if (Input.GetKey(KeyCode.Escape)) { Application.Quit(); }

		// toggle low framerate simulation on 'L'
		if (Input.GetKeyDown(KeyCode.L))
			m_SimulateLowFPS = !m_SimulateLowFPS;
		if (m_SimulateLowFPS)
		{
			for (int v = 0; v < 20000000; v++) { }
		}

		// handle shooting
		if (Input.GetMouseButton(0))
		{
			if ((m_CurrentScreen == 2) && m_ButtonSelection != 0 && (Input.mousePosition.x > 150) &&
				!((Input.mousePosition.y > (Screen.height - 130)&& 
				(Input.mousePosition.x > (Screen.width - 130)))))
				Fire();
		}

		// if user presses 'ENTER' on the 'EXAMPLES' screen, lock the mouse cursor
		if (m_CurrentScreen == 2 && Input.GetKeyUp(KeyCode.Return))
			Screen.lockCursor = !Screen.lockCursor;

		// if the mouse cursor is locked, partly fade out the gui, otherwise show it fully
		if (Screen.lockCursor)
			m_GlobalAlpha = 0.35f;
		else
			m_GlobalAlpha = 1.0f;

	}


	///////////////////////////////////////////////////////////
	// demo screen to show a welcoming message
	///////////////////////////////////////////////////////////
	void DemoIntro()
	{

		// this bracket is only run the first frame of a new demo screen
		// being rendered. it is used to make some initial settings
		// specific to the current demo screen.
		if (m_FirstFrame)
		{
			m_DrawCrosshair = false;
			m_FirstFrame = false;
			LoadCamera("Examples/ImmobileCamera");		// this loads a preset on the FPSCamera component
			m_Camera.SetWeapon(0);
			m_Controller.Load("Examples/ImmobileController");	// this loads a preset on the FPSController component
			LockPlayer(m_OverviewPos, m_OverviewAngle);			// prevent player from moving
			m_LastInputTime -= 20;								// makes the big arrow start fading 20 seconds earlier on this screen
		}

		// draw the three big boxes at the top of the screen;
		// the next & prev arrows, and the main text box
		DrawBoxes("welcome", "Ultimate FPS Camera is a first person camera system with ultra smooth PROCEDURAL ANIMATION of player movements. Camera and weapons are manipulated using over 70 parameters, allowing for a vast range of super-lifelike behaviors.");
		
	}


	///////////////////////////////////////////////////////////
	// demo screen to show various presets that are possible
	// with Ultimate FPS Camera
	///////////////////////////////////////////////////////////
	void DemoExamples()
	{

		if (m_FirstFrame)
		{
			m_DrawCrosshair = true;
			m_Camera.SetWeapon(0);
			LoadCamera("Examples/SystemOFFCamera");
			SwitchWeapon(3, "Examples/SystemOFFWeaponGlideIn");
			m_Controller.Load("Examples/SystemOFFController");
			Teleport(m_StartPos, m_StartAngle);					// place the player at start point
			m_FirstFrame = false;
			m_UnlockPosition = m_Controller.transform.position;	// makes player return to start pos when unfreezed (from sniper, turret presets etc.)
			m_ButtonSelection = 0;
		}

		DrawBoxes("examples", "Try MOVING, JUMPING and STRAFING with the demo presets on the left.\nNote that NO ANIMATIONS are used in this demo. Instead, the camera and weapons are manipulated using realtime SPRING PHYSICS, SINUS BOB and NOISE SHAKING.\nCombining this with traditional animations (e.g. reload) can be very powerful!");

		// show a toggle column, a compound control displaying a
		// column of buttons that can be toggled like radio buttons
		int currentSel = m_ButtonSelection;
		string[] strings = new string[] { "System OFF", "Mafia Boss", "Modern Shooter", "Barbarian", "Sniper Breath", "Astronaut", "Mech... or Dino?", "Tank Turret", "Drunk Person", "Old School", "Crazy Cowboy" };
		m_ButtonSelection = ToggleColumn(140, 150, m_ButtonSelection, strings, false, true);

		// if selected button in toggle column has changed, change
		// the current preset
		if (m_ButtonSelection != currentSel)
		{

			Screen.lockCursor = true;

			if (m_UnlockPosition != Vector3.zero)
				UnlockPlayer();

			switch (m_ButtonSelection)
			{
				case 0:	// --- System OFF ---
					m_DrawCrosshair = true;
					m_Controller.Load("Examples/SystemOFFController");
					if (m_Camera.CurrentWeaponID == 5)	// mech cockpit is not allowed in 'system off' mode
					{
						LoadCamera("Examples/SystemOFFCamera");	// so replace it by pistol
						SwitchWeapon(1, "Examples/SystemOFFWeapon");
					}
					else
					{
						LoadCamera("Examples/SystemOFFCamera");
						LoadWeaponPreset("Examples/SystemOFFWeapon");
					}
					m_ZoomedIn = false;
					break;
				case 1:	// --- Mafia Boss ---
					m_DrawCrosshair = true;
					if (m_LastLoadedCamera == "Examples/SystemOFFCamera")
					{
						LoadCamera("Examples/MafiaCamera");
						m_Camera.SetWeapon(3);
						LoadWeaponPreset("Examples/MafiaWeapon", "Examples/MafiaShooter", true);
					}
					else
					{
						LoadCamera("Examples/MafiaCamera");
						SwitchWeapon(3, "Examples/MafiaWeapon", "Examples/MafiaShooter");
					}
					m_Controller.Load("Examples/ModernController");
					m_ZoomedIn = false;
					break;
				case 2:	// --- Modern Shooter ---
					m_DrawCrosshair = true;
					if (m_LastLoadedCamera == "Examples/SystemOFFCamera")
					{
						LoadCamera("Examples/ModernCamera");
						m_Camera.SetWeapon(1);
						LoadWeaponPreset("Examples/ModernWeapon", "Examples/ModernShooter", true);
					}
					else
					{
						LoadCamera("Examples/ModernCamera");
						SwitchWeapon(1, "Examples/ModernWeapon", "Examples/ModernShooter");
					}
					m_Controller.Load("Examples/ModernController");
					m_ZoomedIn = false;
					break;
				case 3:	// --- Barbarian ---
					m_DrawCrosshair = false;
					if (m_LastLoadedCamera == "Examples/SystemOFFCamera")
					{
						LoadCamera("Examples/MaceCamera");
						m_Camera.SetWeapon(4);
						LoadWeaponPreset("Examples/MaceWeapon", null, true);
					}
					else
					{
						LoadCamera("Examples/MaceCamera");
						SwitchWeapon(4, "Examples/MaceWeapon");
					}
					m_Controller.Load("Examples/ModernController");
					m_ZoomedIn = false;
					break;
				case 4:	// --- Sniper Breath ---
					m_DrawCrosshair = true;
					LoadCamera("Examples/SniperCamera");
					m_Camera.SetWeapon(2);
					LoadWeaponPreset("Examples/SniperWeapon", "Examples/SniperShooter", true);
					m_Controller.Load("Examples/CrouchController");
					Teleport(m_SniperPos, m_SniperAngle);
					m_ZoomedIn = false;
					break;
				case 5:	// --- Astronaut ---
					m_DrawCrosshair = false;
					LoadCamera("Examples/AstronautCamera");
					m_Camera.SetWeapon(0);
					m_Controller.Load("Examples/AstronautController");
					Teleport(m_AstronautPos, m_AstronautAngle);
					m_ZoomedIn = false;
					break;
				case 6:	// --- Mech... or Dino? ---
					m_DrawCrosshair = true;
					m_UnlockPosition = m_DrunkPos;
					m_Controller.Load("Examples/MechController");
					Teleport(m_MechPos, m_MechAngle);
					LoadCamera("Examples/MechCamera");
					m_Camera.SetWeapon(5);
					LoadWeaponPreset("Examples/MechWeapon", "Examples/MechShooter", true);
					m_Camera.BobStepCallback = delegate()
					{
						m_Camera.AddForce2(new Vector3(0.0f, -1.0f, 0.0f));
						m_Camera.CurrentWeapon.AddForce(new Vector3(0, 0, 0), new Vector3(-0.3f, 0, 0));
						m_AudioSource.PlayOneShot(m_StompSound);
					};
					m_ZoomedIn = false;
					break;
				case 7:	// --- Tank Turret ---
					m_DrawCrosshair = true;
					LoadCamera("Examples/TurretCamera");
					m_Camera.SetWeapon(3);
					LoadWeaponPreset("Examples/TurretWeapon", "Examples/TurretShooter", true);
					m_Camera.CurrentWeapon.SnapSprings();
					m_Controller.Load("Examples/DefaultController");
					LockPlayer(m_OverviewPos, m_OverviewAngle);
					m_ZoomedIn = false;
					break;
				case 8:	// --- Drunk Person ---
					m_DrawCrosshair = false;
					LoadCamera("Examples/DrunkCamera");
					m_Camera.SetWeapon(0);
					m_Controller.Load("Examples/DrunkController");
					Teleport(m_DrunkPos, m_DrunkAngle);
					m_ZoomedIn = false;
					break;
				case 9:	// --- Old School ---
					m_DrawCrosshair = true;
					LoadCamera("Examples/OldSchoolCamera");
					m_Camera.SetWeapon(1);
					LoadWeaponPreset("Examples/OldSchoolWeapon", "Examples/OldSchoolShooter");
					m_Controller.Load("Examples/OldSchoolController");
					Teleport(m_OldSchoolPos, m_OldSchoolAngle);
					m_ZoomedIn = false;
					m_Camera.SnapSprings();
					m_Camera.CurrentWeapon.SnapPivot();
					m_Camera.SnapZoom();
					break;
				case 10:// --- Crazy Cowboy ---
					m_DrawCrosshair = true;
					if (m_LastLoadedCamera == "Examples/SystemOFFCamera")
					{
						LoadCamera("Examples/CowboyCamera");
						m_Camera.SetWeapon(2);
						LoadWeaponPreset("Examples/CowboyWeapon", "Examples/CowboyShooter", true);
					}
					else
					{
						LoadCamera("Examples/CowboyCamera");
						SwitchWeapon(2, "Examples/CowboyWeapon", "Examples/CowboyShooter");
					}
					m_Controller.Load("Examples/CowboyController");
					Teleport(m_StartPos, m_StartAngle);
					m_ZoomedIn = false;
					break;
			}
			m_LastInputTime = Time.time;
		}

		// draw menu re-enable text
		if (Screen.lockCursor)
		{
			GUI.color = new Color(1, 1, 1, 1);
			GUI.Label(new Rect((Screen.width / 2) - 200, 140, 400, 20), "(Press ENTER to reenable menu)", m_CenterStyle);
			GUI.color = new Color(1, 1, 1, 1 * m_GlobalAlpha);
		}

	}


	///////////////////////////////////////////////////////////
	// demo screen to explain external forces
	///////////////////////////////////////////////////////////
	void DemoForces()
	{

		DrawBoxes("external forces", "The camera and weapon are mounted on 8 positional and angular springs.\nEXTERNAL FORCES can be applied to these in various ways, creating unique movement patterns every time. This is useful for shockwaves, explosion knockback and earthquakes.");

		if (m_FirstFrame)
		{
			m_DrawCrosshair = true;
			LoadCamera("Examples/StompingCamera");
			SwitchWeapon(1, "Examples/ModernWeapon");
			m_Controller.Load("Examples/SmackController");
			m_Camera.SnapZoom();
			m_FirstFrame = false;
			Teleport(m_ForcesPos, m_ForcesAngle);
			m_ExtForcesButtonColumnArrowY = -100.0f;
			m_Camera.CurrentWeapon.SnapPivot();
			m_Camera.SetWeapon(1);
		}

		// draw toggle column showing force examples
		int m_ButtonSelection = -1;
		string[] strings = new string[] { "Earthquake", "Boss Stomp", "Incoming Artillery" };
		m_ButtonSelection = ButtonColumn(150, m_ButtonSelection, strings);
		if (m_ButtonSelection != -1)
		{
			if (m_UnlockPosition != Vector3.zero)
				UnlockPlayer();

			switch (m_ButtonSelection)
			{
				case 0:	// --- Earthquake ---
					LoadCamera("Examples/StompingCamera");
					m_Controller.Load("Examples/SmackController");
					m_Camera.DoEarthQuake(0.1f, 0.1f, 10.0f);
					m_ExtForcesButtonColumnArrowFadeoutTime = Time.time + 9;
					m_AudioSource.Stop();
					m_AudioSource.PlayOneShot(m_EarthquakeSound);
					break;
				case 1:	// --- Boss Stomp ---
					m_Camera.StopEarthQuake();
					LoadCamera("Examples/StompingCamera");
					m_Controller.Load("Examples/SmackController");
					m_Camera.DoStomp(1.0f);
					m_ExtForcesButtonColumnArrowFadeoutTime = Time.time;
					m_AudioSource.Stop();
					m_AudioSource.PlayOneShot(m_StompSound);
					break;
				case 2:	// --- Incoming Artillery ---
					m_Camera.StopEarthQuake();
					LoadCamera("Examples/ArtilleryCamera");
					m_Controller.Load("Examples/ArtilleryController");
					m_Camera.DoBomb(1.0f);
					m_Controller.AddForce(UnityEngine.Random.Range(-1.5f, 1.5f), 0.1f,
																	UnityEngine.Random.Range(-1.5f, 1.5f));
					m_ExtForcesButtonColumnArrowFadeoutTime = Time.time + 1;
					m_AudioSource.Stop();
					m_AudioSource.PlayOneShot(m_ExplosionSound);
					break;
			}
			m_LastInputTime = Time.time;
		}

		// show a screenshot preview of the mouse input editor section
		// in the bottom left corner.
		DrawEditorPreview(m_ImageWeaponPosition);

	}

	
	///////////////////////////////////////////////////////////
	// demo screen to explain mouse smoothing and acceleration
	///////////////////////////////////////////////////////////
	void DemoMouseInput()
	{

		if (m_FirstFrame)
		{
			m_DrawCrosshair = true;
			LockPlayer(m_MouseLookPos, m_MouseLookAngle);
			LoadCamera("Examples/MouseRawUnityCamera");
			m_Controller.Load("Examples/ImmobileController");
			m_FirstFrame = false;
			m_Camera.SetWeapon(0);
		}

		DrawBoxes("mouse input", "Any good FPS should offer configurable MOUSE SMOOTHING and ACCELERATION.\n• Smoothing interpolates mouse input over several frames to reduce jittering.\n • Acceleration + low mouse sensitivity allows high precision without loss of turn speed.\n• Click the below buttons to compare some example setups.");

		// show a toggle column for mouse examples
		int currentSel = m_ButtonSelection;
		bool showArrow = (m_ButtonSelection == 2) ? false : true;	// small arrow for the 'acceleration' button
		string[] strings = new string[] { "Raw Unity Mouse Input", "Mouse Smoothing", "Low Sens. + Acceleration" };
		m_ButtonSelection = ToggleColumn(200, 150, m_ButtonSelection, strings, true, showArrow);

		if (m_ButtonSelection != currentSel)
		{
			switch (m_ButtonSelection)
			{
				case 0:	// --- Raw Unity Mouse Input ---
					LoadCamera("Examples/MouseRawUnityCamera");
					break;
				case 1:	// --- Mouse Smoothing ---
					LoadCamera("Examples/MouseSmoothingCamera");
					break;
				case 2:	// --- Low Sens. + Acceleration ---
					LoadCamera("Examples/MouseLowSensCamera");
					break;
			}
			m_LastInputTime = Time.time;
		}

		// separate small arrow for the 'ON / OFF' buttons. this one points
		// upward and is only shown if 'acceleration' is chosen
		showArrow = true;
		if (m_ButtonSelection != 2)
		{
			GUI.enabled = false;
			showArrow = false;
		}

		// show a 'button toggle', a compound control for a basic on / off toggle
		m_Camera.MouseAcceleration = ButtonToggle(new Rect((Screen.width / 2) + 110, 215, 90, 40),
													"Acceleration", m_Camera.MouseAcceleration, showArrow);
		GUI.color = new Color(1, 1, 1, 1 * m_GlobalAlpha);
		GUI.enabled = true;

		DrawEditorPreview(m_ImageCameraMouse);

	}

	
	///////////////////////////////////////////////////////////
	// demo screen to explain weapon FOV and offset
	///////////////////////////////////////////////////////////
	void DemoWeaponPerspective()
	{

		DrawBoxes("weapon perspective", "Proper WEAPON PERSPECTIVE is crucial to the final impression of your game!\nThis weapon has its own separate Field of View for full perspective control,\nalong with weapon position and rotation offset.");

		if (m_FirstFrame)
		{
			m_DrawCrosshair = true;
			LoadCamera("Examples/PerspOldCamera");
			SwitchWeapon(3, "Examples/PerspOldWeapon");
			m_Camera.SnapZoom();	// prevents animated zooming and instead sets the zoom in one frame
			m_FirstFrame = false;
			LockPlayer(m_OverviewPos, m_PerspectiveAngle, true);
		}

		// show toggle column for the weapon FOV example buttons
		int currentSel = m_ButtonSelection;
		string[] strings = new string[] { "Old School", "1999 Internet Café", "Modern Shooter" };
		m_ButtonSelection = ToggleColumn(200, 150, m_ButtonSelection, strings, true, true);
		if (m_ButtonSelection != currentSel)
		{
			switch (m_ButtonSelection)
			{
				case 0:	// --- Old School ---
					LoadCamera("Examples/PerspOldCamera");
					LoadWeaponPreset("Examples/PerspOldWeapon", null, true);
					m_Camera.SnapZoom();
					m_Camera.CurrentWeapon.SnapPivot();	// prevents transitioning the pivot and sets its position in one frame
					LockPlayer(m_OverviewPos, m_PerspectiveAngle, true);
					break;
				case 1:	// --- 1999 Internet Café ---
					LoadCamera("Examples/Persp1999Camera");
					LoadWeaponPreset("Examples/Persp1999Weapon", null, true);
					m_Camera.SnapZoom();
					m_Camera.CurrentWeapon.SnapPivot();
					LockPlayer(m_OverviewPos, m_PerspectiveAngle, true);
					break;
				case 2:	// --- Modern Shooter ---
					LoadCamera("Examples/PerspModernCamera");
					LoadWeaponPreset("Examples/PerspModernWeapon", null, true);
					m_Camera.SnapZoom();
					m_Camera.CurrentWeapon.SnapPivot();
					LockPlayer(m_OverviewPos, m_PerspectiveAngle, true);
					break;
			}
			m_LastInputTime = Time.time;
		}

		DrawEditorPreview(m_ImageWeaponPerspective);

	}
	

	///////////////////////////////////////////////////////////
	// demo screen to explain pivot manipulation
	///////////////////////////////////////////////////////////
	void DemoPivot()
	{

		DrawBoxes("weapon pivot", "The PIVOT POINT of the weapon model greatly affects movement pattern.\nManipulating it at runtime can be quite useful, and easy with Ultimate FPS Camera!\nClick the examples below and move the camera around.");

		if (m_FirstFrame)
		{
			m_DrawCrosshair = false;
			LoadCamera("Examples/DefaultCamera");
			m_Controller.Load("Examples/DefaultController");
			m_FirstFrame = false;
			LockPlayer(m_OverviewPos, m_OverviewAngle);
			m_Camera.CurrentWeapon.RefreshSettings();
			m_Camera.SetWeapon(1);
			LoadWeaponPreset("Examples/DefaultWeapon");
			LoadWeaponPreset("Examples/PivotMuzzleWeapon", null, true);
			m_Camera.CurrentWeapon.SetPivotVisible(true);

		}

		// show toggle column for the various pivot examples
		int currentSel = m_ButtonSelection;
		string[] strings = new string[] { "Muzzle", "Grip", "Chest", "Elbow (Uzi Style)" };
		m_ButtonSelection = ToggleColumn(200, 150, m_ButtonSelection, strings, true, true);
		if (m_ButtonSelection != currentSel)
		{
			switch (m_ButtonSelection)
			{
				case 0:	// --- Muzzle ---
					LoadWeaponPreset("Examples/PivotMuzzleWeapon", null, true);
					break;
				case 1:	// --- Grip ---
					LoadWeaponPreset("Examples/PivotWristWeapon", null, true);
					break;
				case 2:	// --- Chest ---
					LoadWeaponPreset("Examples/PivotChestWeapon", null, true);
					break;
				case 3:	// --- Elbow (Uzi Style) ---
					LoadWeaponPreset("Examples/PivotElbowWeapon", null, true);
					break;
			}
			m_LastInputTime = Time.time;
		}

		DrawEditorPreview(m_ImageWeaponPivot);

	}


	///////////////////////////////////////////////////////////
	// demo screen for explaining weapon camera layer
	// NOTE: weapon layer is hardcoded as layer 31. this is
	// set in vp_Layer.cs
	///////////////////////////////////////////////////////////
	void DemoWeaponLayer()
	{

		DrawBoxes("weapon layer", "\nThe weapon is rendered by a SEPARATE CAMERA so that it never sticks through walls or other geometry. Try toggling the weapon layer ON and OFF below.");

		if (m_FirstFrame)
		{
			m_DrawCrosshair = true;
			LoadCamera("Examples/WallFacingCamera");
			m_Camera.SetWeapon(3);
			LoadWeaponPreset("Examples/WallFacingWeapon");
			m_Camera.SnapZoom();
			m_WeaponLayerToggle = false;
			m_FirstFrame = false;
			LockPlayer(m_WeaponLayerPos, m_WeaponLayerAngle);
			int layer = (m_WeaponLayerToggle ? vp_Layer.Weapon : 0);
			m_Camera.SetWeaponLayer(layer);
		}

		// show button toggle for enabling / disabling the weapon layer
		bool currentWeaponLayer = m_WeaponLayerToggle;
		m_WeaponLayerToggle = ButtonToggle(new Rect((Screen.width / 2) - 45, 180, 90, 40), "Weapon Layer",
												m_WeaponLayerToggle, true);
		if (currentWeaponLayer != m_WeaponLayerToggle)
		{
			LockPlayer(m_WeaponLayerPos, m_WeaponLayerAngle);
			int layer = (m_WeaponLayerToggle ? vp_Layer.Weapon : 0);
			m_Camera.SetWeaponLayer(layer);
			m_LastInputTime = Time.time;
		}
		
	}


	///////////////////////////////////////////////////////////
	// demo screen to explain preset loading and saving features
	///////////////////////////////////////////////////////////
	void DemoPreset()
	{

		if (m_FirstFrame)
		{
			LoadCamera("Examples/ImmobileCamera");
			m_Camera.SetWeapon(0);
			m_Controller.Load("Examples/ImmobileController");
			LockPlayer(m_OverviewPos, m_OverviewAngle, true);
			m_FirstFrame = false;
		}

		DrawBoxes("preset system", "Perhaps our most powerful feature is the COMPONENT PRESET class, which allows you to save and load components in editable text files using a practical file dialog. Presets allow the game to quickly load and combine complex behaviors at runtime. Our 'ComponentPreset' and 'FileDialog' classes are generic and might be useful with your other types of components.");

		// show an image of all the editor parameters
		DrawImage(m_ImagePresetDialogs);

	}


	///////////////////////////////////////////////////////////
	// demo screen to present the shooter component
	///////////////////////////////////////////////////////////
	void DemoShooter()
	{

		if (m_FirstFrame)
		{
			LoadCamera("Examples/ImmobileCamera");
			m_Camera.SetWeapon(0);
			m_Controller.Load("Examples/ImmobileController");
			LockPlayer(m_OverviewPos, m_OverviewAngle);
			m_FirstFrame = false;
		}

		DrawBoxes("shooting & fx", "Ultimate FPS Camera v1.2 has a battery of new SHOOTER FEATURES including advanced recoil settings, raycasting bullets, a decal manager for bulletholes, realistic shell case physics and muzzle flashes. Several example particle FX prefabs are included.");
		DrawImage(m_ImageShooter);
		
	}
	
	
	///////////////////////////////////////////////////////////
	// demo screen to show a final summary message and editor screenshot
	///////////////////////////////////////////////////////////
	void DemoOutro()
	{

		if (m_FirstFrame)
		{
			m_DrawCrosshair = false;
			LoadCamera("Examples/ImmobileCamera");
			m_Camera.SetWeapon(0);
			m_Controller.Load("Examples/ImmobileController");
			LockPlayer(m_OverviewPos, m_OverviewAngle);
			m_FirstFrame = false;
		}

		DrawBoxes("putting it all together", "Included in the package is full, well commented C# source code & documentation,\na basic FPS WALKER component plus all the PRESETS and 3D ART used in this demo.\nA fantastic starting point - or upgrade - for any FPS project.\nGet it from the Asset Store today!");

		DrawImage(m_ImageAllParams);

	}


	///////////////////////////////////////////////////////////
	// draws a 'button toggle', a compound control for a basic on / off toggle
	// used for mouse input and weapon layer demos
	///////////////////////////////////////////////////////////
	bool ButtonToggle(Rect rect, string label, bool state, bool arrow)
	{

		GUIStyle onStyle = m_UpStyle;
		GUIStyle offStyle = m_DownStyle;

		// if state is true, button is 'ON'
		float arrowOffset = 0.0f;
		if (state)
		{
			onStyle = m_DownStyle;
			offStyle = m_UpStyle;
			arrowOffset = (rect.width * 0.5f) + 2;
		}

		// draw toggle label
		GUI.Label(new Rect(rect.x, rect.y - 30, rect.width, rect.height), label, m_CenterStyle);
		
		// draw buttons
		if (GUI.Button(new Rect(rect.x, rect.y, (rect.width * 0.5f)-2, rect.height), "OFF", offStyle))
			state = false;
		if (GUI.Button(new Rect(rect.x + (rect.width * 0.5f) + 2, rect.y,
											(rect.width * 0.5f), rect.height), "ON", onStyle))
			state = true;

		// if button has an arrow, draw it
		if(arrow)
			GUI.Label(new Rect(rect.x + ((rect.width * 0.5f) * 0.5f) - 14 + arrowOffset,
											rect.y + rect.height, 32, 32), m_ImageUpPointer);
		
		return state;

	}


	///////////////////////////////////////////////////////////
	// this method draws the three big boxes at the top of the screen;
	// the next & prev arrows, and the main text box
	///////////////////////////////////////////////////////////
	void DrawBoxes(string caption, string description)
	{

		GUI.color = new Color(1, 1, 1, 1 * m_GlobalAlpha);
		float screenCenter = Screen.width / 2;

		GUILayout.BeginArea(new Rect(screenCenter - 400, 30, 800, 100));

		// draw box for 'prev' button, unless this is the first screen
		if (m_CurrentScreen > 1)
			GUI.Box(new Rect(30, 10, 80, 80), "");

		// draw big text background box
		GUI.Box(new Rect(120, 0, 560, 100), "");

		GUI.color = new Color(1, 1, 1, m_TextAlpha);

		// draw text
		for (int v = 0; v < 3; v++)
		{
			GUILayout.BeginArea(new Rect(130, 10, 540, 80));
			GUILayout.Label("--- " + caption.ToUpper() + " ---" + "\n" + description, m_LabelStyle);
			GUILayout.EndArea();
		}
		GUI.color = new Color(1, 1, 1, 1 * m_GlobalAlpha);

		// draw box for 'next' button
		GUI.Box(new Rect(690, 10, 80, 80), "");

		// draw 'prev' arrow button, unless this is the first screen
		if (m_CurrentScreen > 1)
		{
			if (GUI.Button(new Rect(35, 15, 80, 80), m_ImageLeftArrow, "Label"))
			{
				m_FadeToScreen = Mathf.Max(m_CurrentScreen - 1, 1);
				m_FadeState = FadeState.FadeOut;
			}
		}

		// handle arrow fade state
		if (Time.time < m_LastInputTime + 30)
			m_BigArrowFadeAlpha = 1;
		else
			m_BigArrowFadeAlpha = 0.5f - Mathf.Sin((Time.time - 0.5f) * 3) * 0.5f;
		GUI.color = new Color(1, 1, 1, m_BigArrowFadeAlpha * m_GlobalAlpha);

		// if this is a normal screen, draw 'next' arrow button ...
		if (m_CurrentScreen < 10)
		{
			if (GUI.Button(new Rect(700, 15, 80, 80), m_ImageRightArrow, "Label"))
			{
				m_FadeToScreen = m_CurrentScreen + 1;
				m_FadeState = FadeState.FadeOut;
			}
		}
		// ... and if it's the last screen, instead draw a check mark
		else
		{
			if (GUI.Button(new Rect(700, 15, 80, 80), m_ImageCheck, "Label"))
			{
				m_FadeToScreen = 1;
				m_FadeState = FadeState.FadeOut;
			}
		}
		GUI.color = new Color(1, 1, 1, 1 * m_GlobalAlpha);
		GUILayout.EndArea();
		GUI.color = new Color(1, 1, 1, m_TextAlpha * m_GlobalAlpha);

	}


	///////////////////////////////////////////////////////////
	// draws a 'toggle column', a compound control displaying a
	// column of buttons that can be toggled like radio buttons
	///////////////////////////////////////////////////////////
	int ToggleColumn(int width, int y, int sel, string[] strings, bool center, bool arrow)
	{

		float height = (strings.Length * 30);

		Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

		// initial alignment
		Rect rect;
		if(center)
			rect = new Rect(screenCenter.x - width, y, width, 30);
		else
			rect = new Rect(Screen.width - width - 10, screenCenter.y - (height / 2), width, 30);

		// draw one button for each string
		int v = 0;
		foreach (string s in strings)
		{

			// individual button alignment
			if (center)
				rect.x = screenCenter.x - (width / 2);
			else
				rect.x = 10;
			rect.width = width;

			// set default style
			GUIStyle style = m_UpStyle;
			
			// pressed buttons
			if (v == sel)
			{

				Color col = GUI.color;
				GUI.color = new Color(1, 1, 1, 1);

				// set pressed style
				style = m_DownStyle;

				// make button rect appear pressed
				if (center)
					rect.x = screenCenter.x - (width / 2) + 10;
				else
					rect.x = 20;
				rect.width = width - 20;

				// draw arrow if applicable
				if (arrow)
				{
					if (center)
						GUI.Label(new Rect(rect.x - 27, rect.y, 32, 32), m_ImageRightPointer);
					else
						GUI.Label(new Rect(rect.x + rect.width + 5, rect.y, 32, 32), m_ImageLeftPointer);
				}

				GUI.color = col;

			}
			
			// draw the button and detect selection
			if (GUI.Button(rect, s, style))
				sel = v;
			
			rect.y += 35;
			v++;

		}

		return sel;

	}

	
	///////////////////////////////////////////////////////////
	// draws a 'button column' compound control, with a selection
	// arrow that fades out after a set amount of time.
	// presently used only in the 'EXTERNAL FORCES' demo
	///////////////////////////////////////////////////////////
	int ButtonColumn(int y, int sel, string[] strings)
	{

		float screenCenter = Screen.width / 2;
		Rect rect = new Rect(screenCenter - 100, y, 200, 30);

		// draw one button for each string
		int v = 0;
		foreach (string s in strings)
		{

			rect.x = screenCenter - 100;
			rect.width = 200;

			// draw button and detect click
			if (GUI.Button(rect, s))
			{
				sel = v;
				m_ExtForcesButtonColumnClickTime = Time.time;
				m_ExtForcesButtonColumnArrowY = rect.y;
			}
			
			rect.y += 35;
			v++;
		}

		// fade out the arrow after a set amount of time
		if (Time.time < m_ExtForcesButtonColumnArrowFadeoutTime)
			m_ExtForcesButtonColumnClickTime = Time.time;

		// draw the arrow
		GUI.color = new Color(1, 1, 1, Mathf.Max(0, 1 - (Time.time - m_ExtForcesButtonColumnClickTime) * 1.0f * m_GlobalAlpha));
		GUI.Label(new Rect(rect.x - 27, m_ExtForcesButtonColumnArrowY, 32, 32), m_ImageRightPointer);
		GUI.color = new Color(1, 1, 1, 1 * m_GlobalAlpha);

		return sel;

	}

	
	///////////////////////////////////////////////////////////
	// resets various settings. used when transitioning between
	// two demo screens
	///////////////////////////////////////////////////////////
	void Reset()
	{

		m_Camera.SetWeaponLayer(vp_Layer.Weapon);

		if (m_SwitchWeaponTimer != null)
			m_SwitchWeaponTimer.Cancel();

		if (m_ShowWeaponTimer != null)
			m_ShowWeaponTimer.Cancel();
		
		m_FirstFrame = true;
		m_LastInputTime = Time.time;
		m_ButtonSelection = 0;
		m_ZoomedIn = false;
		
		LoadCamera("Examples/DefaultCamera");
		m_Camera.SetWeapon(0);
		LoadWeaponPreset("Examples/DefaultWeapon");
		m_Camera.StopEarthQuake();
		m_Camera.BobStepCallback = null;
		m_Camera.CurrentWeapon.SetPivotVisible(false);
		m_Controller.Load("Examples/DefaultController");

		if (Screen.width < 1024)
			m_EditorPreviewSectionExpanded = false;
		else
			m_EditorPreviewSectionExpanded = true;

		if (m_UnlockPosition != Vector3.zero)
			UnlockPlayer();

		m_AudioSource.Stop();

	}


	///////////////////////////////////////////////////////////
	// moves the controller to a coordinate in one frame and
	// freezes movement and optionally camera input
	///////////////////////////////////////////////////////////
	void LockPlayer(Vector3 pos, Vector2 startAngle, bool lockCamera)
	{
		m_UnlockPosition = m_Controller.transform.position;
		m_Controller.Load("Examples/ImmobileController");
		Teleport(pos, startAngle);
		if (lockCamera)
		{
			m_Camera.MouseSensitivity = new Vector2(0, 0);
		}
	}

	void LockPlayer(Vector3 pos, Vector2 startAngle)
	{
		LockPlayer(pos, startAngle, false);
	}

		
	///////////////////////////////////////////////////////////
	// snaps the controller position and camera angle to a certain
	// coordinate and pitch/yaw, respectively
	///////////////////////////////////////////////////////////
	void Teleport(Vector3 pos, Vector2 startAngle)
	{
		m_Controller.SetPosition(pos);
		m_Camera.SetAngle(startAngle.x, startAngle.y);
	}


	///////////////////////////////////////////////////////////
	// restores a locked down controller to the position it
	// had before getting locked
	///////////////////////////////////////////////////////////
	void UnlockPlayer()
	{
		m_Controller.transform.position = m_UnlockPosition;
		m_Controller.Load("Examples/DefaultController");
		m_UnlockPosition = Vector3.zero;
	}


	///////////////////////////////////////////////////////////
	// sets up any special gui styles
	///////////////////////////////////////////////////////////
	private void InitGUIStyles()
	{

		m_LabelStyle = new GUIStyle("Label");
		m_LabelStyle.alignment = TextAnchor.LowerCenter;

		m_UpStyle = new GUIStyle("Button");

		m_DownStyle = new GUIStyle("Button");
		m_DownStyle.normal = m_DownStyle.active;

		m_CenterStyle = new GUIStyle("Label");
		m_CenterStyle.alignment = TextAnchor.MiddleCenter;

		m_StylesInitialized = true;

	}


	///////////////////////////////////////////////////////////
	// helper method to draw an image relative to the center
	// of the screen
	///////////////////////////////////////////////////////////
	void DrawImage(Texture image, float xOffset, float yOffset)
	{

		float screenCenter = Screen.width / 2;
		float width = Mathf.Min(image.width, Screen.width);
		float aspect = (float)image.height / (float)image.width;
		GUI.DrawTexture(new Rect(screenCenter - (width / 2) + xOffset, 140 + yOffset, width, width * aspect), image);

	}

	void DrawImage(Texture image)
	{
		DrawImage(image, 0, 0);
	}


	///////////////////////////////////////////////////////////
	// draws a screenshot preview of the corresponding editor section
	// in the bottom left corner. the screenshot can be expanded and
	// collapsed, and fades in a 'screenshot' label if the mouse is
	// held over it.
	///////////////////////////////////////////////////////////
	void DrawEditorPreview(Texture section)
	{

		Texture caption;
		Color col = GUI.color;
		Vector2 mousePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
		float xPos = 0;

		// handle expanded preview section
		if (m_EditorPreviewSectionExpanded)
		{

			// draw image in lower left corner
			caption = m_ImageEditorPreview;
			float captionY = Screen.height - section.height - caption.height;
			float sectionY = Screen.height - section.height;

			GUI.DrawTexture(new Rect(xPos, captionY, caption.width, caption.height), caption);
			GUI.DrawTexture(new Rect(xPos, sectionY, section.width, section.height), section);

			// if mouse is over the preview section, fade in 'screenshot'
			// text and test for click.
			if ((mousePos.x > xPos) && (mousePos.x < xPos + section.width) &&
										(mousePos.y > captionY) &&
										(mousePos.y < Screen.height - caption.height))
			{
				m_EditorPreviewScreenshotTextAlpha = Mathf.Min(1.0f, m_EditorPreviewScreenshotTextAlpha + 0.01f);
				// if user presses left mouse button, collapse the preview section
				if (Input.GetMouseButtonDown(0))
					m_EditorPreviewSectionExpanded = false;
			}
			else
				m_EditorPreviewScreenshotTextAlpha = Mathf.Max(0.0f, m_EditorPreviewScreenshotTextAlpha - 0.03f);
			GUI.color = new Color(1, 1, 1, (col.a * 0.5f) * m_EditorPreviewScreenshotTextAlpha);
			GUI.DrawTexture(new Rect(xPos + 48, sectionY + (section.height / 2) - (m_ImageEditorScreenshot.height / 2),
							m_ImageEditorScreenshot.width, m_ImageEditorScreenshot.height), m_ImageEditorScreenshot);
		}

		// handle collapsed preview section
		else
		{
			// draw image in lower left corner
			caption = m_ImageEditorPreviewShow;
			float captionY = Screen.height - caption.height;
			GUI.DrawTexture(new Rect(xPos, captionY, caption.width, caption.height), caption);

			// if mouse is over the preview section, test for click
			if ((mousePos.x > xPos) && (mousePos.x < xPos + section.width) && (mousePos.y > captionY))
			{
				// if user presses left mouse button, expand the preview section
				if (Input.GetMouseButtonUp(0))
					m_EditorPreviewSectionExpanded = true;
			}

		}

		GUI.color = col;

	}


	///////////////////////////////////////////////////////////
	// draws a fullscreen info text for 3 seconds, used right
	// after application start
	///////////////////////////////////////////////////////////
	void DrawFullScreenText()
	{

		if (Time.realtimeSinceStartup > 3.0f)
			m_FullScreenTextAlpha -= m_FadeSpeed * 0.25f;

		GUI.color = new Color(1, 1, 1, m_FullScreenTextAlpha * m_GlobalAlpha);
		GUI.DrawTexture(new Rect((Screen.width / 2) - 120, (Screen.height / 2) - 16, 240, 32), m_ImageFullScreen);
		GUI.color = new Color(1, 1, 1, 1 * m_GlobalAlpha);

	}


	///////////////////////////////////////////////////////////
	// draws a traditional military FPS style crosshair
	///////////////////////////////////////////////////////////
	void DrawCrosshair()
	{

		if (m_DrawCrosshair)
		{
			GUI.color = new Color(1, 1, 1, 0.8f);
			GUI.DrawTexture(new Rect((Screen.width / 2) - 11, (Screen.height / 2) - 11, 22, 22), m_ImageCrosshair);
			GUI.color = new Color(1, 1, 1, 1 * m_GlobalAlpha);
		}

	}


	///////////////////////////////////////////////////////////
	// handles logic to fade between demo screens
	///////////////////////////////////////////////////////////
	void DoScreenTransition()
	{

		// fadeout stage
		if (m_FadeState == FadeState.FadeOut)
		{
			// decrement text alpha
			m_TextAlpha -= m_FadeSpeed;
			if (m_TextAlpha <= 0.0f)
			{
				// reached zero alpha, time to switch screens
				m_TextAlpha = 0.0f;					// cap alpha at zero
				Reset();							// reset all standard values
				m_CurrentScreen = m_FadeToScreen;	// switch screens
				m_FadeState = FadeState.FadeIn;		// move to fadein stage
			}
		}

		// fadein stage
		else if (m_FadeState == FadeState.FadeIn)
		{
			// increment text alpha
			m_TextAlpha += m_FadeSpeed;
			if (m_TextAlpha >= 1.0f)
			{
				// reached full alpha, time to disable transition
				m_TextAlpha = 1.0f;					// cap alpha at 1
				m_FadeState = FadeState.None;		// disable transition
			}
		}

	}


	///////////////////////////////////////////////////////////
	// loads a camera preset and remembers it as the last used camera
	// preset. this is used for quick and dirty demo crouching logic
	///////////////////////////////////////////////////////////
	void LoadCamera(string preset)
	{

		m_LastLoadedCamera = preset;
		m_Camera.Load(preset);

	}


	///////////////////////////////////////////////////////////
	// moves the current weapon model out of view, changes the weapon
	// model and loads a new weapon preset, then moves the new weapon
	// into view. an optional shooter preset can be provided.
	///////////////////////////////////////////////////////////
	void SwitchWeapon(int weapon, string weaponPreset, string shooterPreset = "")
	{

		if (m_Camera.CurrentWeapon == null)
			return;

		// switch weapons

		// prevent firing while putting the current weapon away
		vp_FPSShooter shooter = m_Camera.CurrentWeapon.GetComponent<vp_FPSShooter>();
		if (shooter != null)
			shooter.PreventFiring(0.75f);

		// rotate and move the weapon downwards
		m_Camera.CurrentWeapon.RotationOffset.x = 40;
		m_Camera.CurrentWeapon.PositionOffset.y = -1.0f;
		m_Camera.CurrentWeapon.RefreshSettings();

		// cancel any already ongoing weapon switching activity
		if (m_SwitchWeaponTimer != null)
			m_SwitchWeaponTimer.Cancel();

		// create a new event to switch the weapon once camera has had
		// time enough to rotate out of view. 0.15 seconds should do.
		m_SwitchWeaponTimer = vp_Timer.At(0.15f, delegate()
		{

			// switch weapon and load new weapon settings
			m_Camera.SetWeapon(weapon);
			m_Camera.CurrentWeapon.Load(weaponPreset);

			// prevent firing while taking out the new weapon
			shooter = m_Camera.CurrentWeapon.GetComponent<vp_FPSShooter>();
			if (shooter != null)
				shooter.PreventFiring(0.75f);

			// store the loaded weapon's desired position and angle
			Vector3 pos = m_Camera.CurrentWeapon.PositionOffset;
			Vector3 rot = m_Camera.CurrentWeapon.RotationOffset;

			// force the 'rotated down' angle and position onto the new
			// weapon when it spawns, or it will pop into view
			m_Camera.CurrentWeapon.RotationOffset.x = 40;
			m_Camera.CurrentWeapon.PositionOffset.y = -1.0f;
			m_Camera.CurrentWeapon.SnapSprings();
			m_Camera.CurrentWeapon.SnapPivot();
			m_Camera.CurrentWeapon.SnapZoom();
			m_Camera.CurrentWeapon.RefreshSettings();

			// if the calling method specified a shooter preset, load it
			if (!string.IsNullOrEmpty(shooterPreset))
			{
				if (m_Camera.CurrentShooter != null)
					m_Camera.CurrentShooter.Load(shooterPreset);
			}

			// cancel any ongoing weapon showing activity
			if (m_ShowWeaponTimer != null)
				m_ShowWeaponTimer.Cancel();

			// create a new event to show the new weapon in 0.15 seconds
			m_ShowWeaponTimer = vp_Timer.At(0.15f, delegate()
			{
				// we do this by smoothly restoring the loaded weapon's
				// desired position and angle
				m_Camera.CurrentWeapon.PositionOffset = pos;
				m_Camera.CurrentWeapon.RotationOffset = rot;
				m_Camera.CurrentWeapon.RefreshSettings();
			});
		});

	}
	

	///////////////////////////////////////////////////////////
	// loads a new weapon preset. an optional shooter preset
	// may be provided. if the optional 'smoothFade' boolean
	// is set to true, the new preset will fade in smoothly.
	///////////////////////////////////////////////////////////
	void LoadWeaponPreset(string weaponPreset, string shooterPreset = "", bool smoothFade = false)
	{

		if (m_Camera.CurrentWeapon == null)
			return;

		m_Camera.CurrentWeapon.Load(weaponPreset);

		if (!smoothFade)
		{
			m_Camera.CurrentWeapon.SnapSprings();
			m_Camera.CurrentWeapon.SnapPivot();
			m_Camera.CurrentWeapon.SnapZoom();
		}

		m_Camera.CurrentWeapon.RefreshSettings();

		if (!string.IsNullOrEmpty(shooterPreset))
		{
			if (m_Camera.CurrentShooter != null)
				m_Camera.CurrentShooter.Load(shooterPreset);
		}

	}


	///////////////////////////////////////////////////////////
	// this method is called by the input code, and tells the
	// currently relevant FPSShooter component to fire a shot
	///////////////////////////////////////////////////////////
	void Fire()
	{

		// point the weapon ahead if we're crouching
		if(m_LastLoadedCamera == "Examples/CrouchMafiaCamera")
			LoadWeaponPreset("Examples/MafiaWeapon", null, true);
		else if (m_LastLoadedCamera == "Examples/CrouchModernCamera")
			LoadWeaponPreset("Examples/ModernWeapon", null, true);

		// fire
		vp_FPSShooter shooter = m_Camera.CurrentWeapon.GetComponent<vp_FPSShooter>();
		if (shooter != null)
			shooter.Fire();

	}


	///////////////////////////////////////////////////////////
	// zooms in by a set amount and activates a very subtle camera shake
	///////////////////////////////////////////////////////////
	void ZoomIn()
	{

		if(m_ZoomedIn)
			return;

		// make player move slower while zoomed in
		m_Controller.MotorDamping = 0.3f;

		// set up a higher precision camera state that has lower
		// mouse sensitivity but still allows the player to turn
		// around really fast
		m_Camera.MouseSensitivity = new Vector2(1.0f, 1.0f);
		m_Camera.MouseSmoothSteps = 10;
		m_Camera.MouseAcceleration = true;

		// manipulate camera and weapon field of view
		m_OriginalCameraFOV = m_Camera.RenderingFieldOfView;	// store current FOV for 'ZoomOut' method
		m_Camera.RenderingFieldOfView = m_GenericZoomInCameraFOV;
		m_OriginalWeaponFOV = m_Camera.CurrentWeapon.RenderingFieldOfView;
		m_Camera.CurrentWeapon.RenderingFieldOfView = m_GenericZoomInWeaponFOV;

		// subtle camera shake
		m_Camera.ShakeSpeed = 0.1f;

		// an example of additive loading. here we load presets that only
		// contain information about weapon and muzzle flash position, 
		// shell eject position and shake speed for specific weapons
		if (m_Camera.CurrentWeaponID == 1)	// pistol
		{
			LoadWeaponPreset("Examples/ModernWeaponZoomIn", "Examples/ModernShooterZoomIn", true);
			m_Camera.ShakeSpeed = 0.0f;
		}
		else if (m_Camera.CurrentWeaponID == 2)	// revolver
		{
			LoadWeaponPreset("Examples/CowboyWeaponZoomIn", "Examples/CowboyShooterZoomIn", true);
			m_Camera.ShakeSpeed = 0.0f;
		}
		else if (m_Camera.CurrentWeaponID == 3)	// machinegun
		{
			LoadWeaponPreset("Examples/MafiaWeaponZoomIn", "Examples/MafiaShooterZoomIn", true);
			m_Camera.ShakeSpeed = 0.0f;
		}

		// tell the camera and weapon to start animating the zoom
		m_Camera.Zoom();
		m_Camera.CurrentWeapon.Zoom();

		m_ZoomedIn = true;

	}



	///////////////////////////////////////////////////////////
	// zooms out to the previously loaded preset's zoom value,
	// and disables camera shake
	///////////////////////////////////////////////////////////
	void ZoomOut()
	{

		if (!m_ZoomedIn)
			return;

		// restore all settings changed in 'ZoomIn' to default
		m_Controller.MotorDamping = 0.1f;
		m_Camera.MouseSensitivity = new Vector2(5, 5);
		m_Camera.MouseSmoothSteps = 5;
		m_Camera.MouseAcceleration = false;
		m_Camera.RenderingFieldOfView = m_OriginalCameraFOV;
		m_Camera.CurrentWeapon.RenderingFieldOfView = m_OriginalWeaponFOV;
		m_Camera.ShakeSpeed = 0.0f;

		if (m_Camera.CurrentWeaponID == 1)	// pistol
			LoadWeaponPreset("Examples/ModernWeaponZoomOut", "Examples/ModernShooterZoomOut", true);
		else if (m_Camera.CurrentWeaponID == 2)	// revolver
			LoadWeaponPreset("Examples/CowboyWeaponZoomOut", "Examples/CowboyShooterZoomOut", true);
		else if (m_Camera.CurrentWeaponID == 3)	// machinegun
			LoadWeaponPreset("Examples/MafiaWeaponZoomOut", "Examples/MafiaShooterZoomOut", true);

		// tell the camera and weapon to start animating the zoom
		m_Camera.Zoom();
		m_Camera.CurrentWeapon.Zoom();

		m_ZoomedIn = false;

	}


	///////////////////////////////////////////////////////////
	// quick and dirty crouching logic for demo purposes, hardcoded
	// for the 'modern shooter' and 'mafia boss' presets. your game
	// will likely need a more robust and generic logic.
	///////////////////////////////////////////////////////////
	void DoCrouch()
	{

		if (m_LastLoadedCamera == "Examples/MafiaCamera")
		{
			LoadCamera("Examples/CrouchMafiaCamera");
			LoadWeaponPreset("Examples/CrouchMafiaWeapon", null, true);
		}
		else if (m_LastLoadedCamera == "Examples/ModernCamera")
		{
			LoadCamera("Examples/CrouchModernCamera");
			LoadWeaponPreset("Examples/CrouchModernWeapon", null, true);
		}
		m_Controller.Load("Examples/CrouchController");

	}


	///////////////////////////////////////////////////////////
	// gets the user out of the demo crouching mode
	///////////////////////////////////////////////////////////
	void DontCrouch()
	{

		if (m_LastLoadedCamera == "Examples/CrouchMafiaCamera")
		{
			LoadCamera("Examples/MafiaCamera");
			LoadWeaponPreset("Examples/MafiaWeapon", null, true);
		}
		else if (m_LastLoadedCamera == "Examples/CrouchModernCamera")
		{
			LoadCamera("Examples/ModernCamera");
			LoadWeaponPreset("Examples/ModernWeapon", null, true);
		}

		m_Controller.Load("Examples/ModernController");

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void OnGUI()
	{

		// bail out if demo mode is disabled
		if (!m_DemoMode)
			return;

		// set up gui styles, but only once
		if (!m_StylesInitialized)
			InitGUIStyles();

		// perform drawing method specific to the current demo screen
		switch (m_CurrentScreen)
		{
			case 1: DemoIntro(); break;
			case 2: DemoExamples(); break;
			case 3: DemoForces(); break;
			case 4: DemoMouseInput(); break;
			case 5: DemoWeaponPerspective(); break;
			case 6: DemoWeaponLayer(); break;
			case 7: DemoPivot(); break;
			case 8: DemoShooter(); break;
			case 9: DemoPreset(); break;
			case 10: DemoOutro(); break;
		}

		// uncomment to show a 'toggle fullscreen on 'F'' message.
		// this text is shown for 3 seconds right at the beginning
		//		DrawFullScreenText();

		// FPS crosshair will be shown in most demo screens
		DrawCrosshair();

		// handle fading between demo screens
		DoScreenTransition();

	}

}

