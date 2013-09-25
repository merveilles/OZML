/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSShooterEditor.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	custom inspector for the vp_FPSShooter class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(vp_FPSShooter))]
public class vp_FPSShooterEditor : Editor
{

	// target component
	private vp_FPSShooter m_Component = null;

	// foldouts
	private bool m_ProjectileFoldout = false;
	private bool m_MotionFoldout = false;
	private bool m_MuzzleFlashFoldout = false;
	private bool m_ShellFoldout = false;
	private bool m_SoundFoldout = false;
	private bool m_PresetFoldout = false;

	// muzzleflash
	private bool m_MuzzleFlashVisible = false;		// display the muzzle flash in the editor?

	// load / save preset
	private bool m_Dirty = false;

	// styles
	private GUIStyle m_NoteStyle = null;
	private bool m_GUIStylesInitialized = false;


	///////////////////////////////////////////////////////////
	// hooks up the FPSCamera object to the inspector target
	///////////////////////////////////////////////////////////
	void OnEnable()
	{

		m_Component = (vp_FPSShooter)target;

		EditorApplication.playmodeStateChanged = delegate()
		{
			vp_ComponentPreset.m_Component = (Component)target;
			vp_ComponentPersister.m_Component = (Component)target;
			vp_ComponentPersister.PlayModeCallback();
		};

	}


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	public override void OnInspectorGUI()
	{

		if (!m_GUIStylesInitialized)
			InitGUIStyles();

		string objectInfo = m_Component.gameObject.name;

		if (m_Component.gameObject.active)
			GUI.enabled = true;
		else
		{
			GUI.enabled = false;
			objectInfo += " (INACTIVE)";
		}

		if (!m_Component.gameObject.active)
		{
			GUI.enabled = true;
			return;
		}
		
		// --- Projectile ---
		m_ProjectileFoldout = EditorGUILayout.Foldout(m_ProjectileFoldout, "Projectile");
		if (m_ProjectileFoldout)
		{

			m_Component.ProjectileFiringRate = EditorGUILayout.FloatField("Firing Rate", m_Component.ProjectileFiringRate);
			m_Component.ProjectilePrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", m_Component.ProjectilePrefab, typeof(GameObject), false);
			GUI.enabled = false;
			GUILayout.Label("Prefab should be a gameobject with a projectile\nlogic script added to it (such as vp_Bullet).", m_NoteStyle);
			GUI.enabled = true;
			m_Component.ProjectileScale = EditorGUILayout.Slider("Scale", m_Component.ProjectileScale, 0, 2);
			m_Component.ProjectileCount = EditorGUILayout.IntField("Count", m_Component.ProjectileCount);
			m_Component.ProjectileSpread = EditorGUILayout.Slider("Spread", m_Component.ProjectileSpread, 0, 360);

			vp_EditorGUIUtility.Separator();

		}

		// --- Motion ---

		m_MotionFoldout = EditorGUILayout.Foldout(m_MotionFoldout, "Motion");
		if (m_MotionFoldout)
		{

			m_Component.MotionPositionRecoil = EditorGUILayout.Vector3Field("Position Recoil", m_Component.MotionPositionRecoil);
			m_Component.MotionRotationRecoil = EditorGUILayout.Vector3Field("Rotation Recoil", m_Component.MotionRotationRecoil);
			GUI.enabled = false;
			GUILayout.Label("Recoil forces are added to the secondary\nposition and rotation springs of the weapon.", m_NoteStyle);
			GUI.enabled = true;
			m_Component.MotionPositionReset = EditorGUILayout.Slider("Position Reset", m_Component.MotionPositionReset, 0, 1);
			m_Component.MotionRotationReset = EditorGUILayout.Slider("Rotation Reset", m_Component.MotionRotationReset, 0, 1);
			GUI.enabled = false;
			GUILayout.Label("Upon firing, primary position and rotation springs\nwill snap back to their rest state by this factor.", m_NoteStyle);
			GUI.enabled = true;
			m_Component.MotionPositionPause = EditorGUILayout.Slider("Position Pause", m_Component.MotionPositionPause, 0, 5);
			m_Component.MotionRotationPause = EditorGUILayout.Slider("Rotation Pause", m_Component.MotionRotationPause, 0, 5);
			GUI.enabled = false;
			GUILayout.Label("Upon firing, primary spring forces will pause and\nease back in over this time interval in seconds.", m_NoteStyle);
			GUI.enabled = true;

			vp_EditorGUIUtility.Separator();

		}

		// --- MuzzleFlash ---
		m_MuzzleFlashFoldout = EditorGUILayout.Foldout(m_MuzzleFlashFoldout, "Muzzle Flash");
		if (m_MuzzleFlashFoldout)
		{

			m_Component.MuzzleFlashPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", m_Component.MuzzleFlashPrefab, typeof(GameObject), false);
			GUI.enabled = false;
			GUILayout.Label("Prefab should be a mesh with a Particles/Additive\nshader and a vp_MuzzleFlash script added to it.", m_NoteStyle);
			GUI.enabled = true;
			Vector3 currentPosition = m_Component.MuzzleFlashPosition;
			m_Component.MuzzleFlashPosition = EditorGUILayout.Vector3Field("Position", m_Component.MuzzleFlashPosition);
			Vector3 currentScale = m_Component.MuzzleFlashScale;
			m_Component.MuzzleFlashScale = EditorGUILayout.Vector3Field("Scale", m_Component.MuzzleFlashScale);
			m_Component.MuzzleFlashFadeSpeed = EditorGUILayout.Slider("Fade Speed", m_Component.MuzzleFlashFadeSpeed, 0.001f, 0.2f);
			if (!Application.isPlaying)
				GUI.enabled = false;
			bool currentMuzzleFlashVisible = m_MuzzleFlashVisible;
			m_MuzzleFlashVisible = EditorGUILayout.Toggle("Show Muzzle Fl.", m_MuzzleFlashVisible);
			if (Application.isPlaying)
			{
				if (m_Component.MuzzleFlashPosition != currentPosition ||
					m_Component.MuzzleFlashScale != currentScale)
					m_MuzzleFlashVisible = true;

				vp_MuzzleFlash mf = (vp_MuzzleFlash)m_Component.MuzzleFlash.GetComponent("vp_MuzzleFlash");
				if (mf != null)
					mf.ForceShow = currentMuzzleFlashVisible;

				GUI.enabled = false;
				GUILayout.Label("Set Muzzle Flash Z to about 0.5 to bring it into view.", m_NoteStyle);
				GUI.enabled = true;
			}
			else
				GUILayout.Label("Muzzle Flash can be shown when the game is playing.", m_NoteStyle);
			GUI.enabled = true;

			vp_EditorGUIUtility.Separator();

		}

		// --- Shell ---
		m_ShellFoldout = EditorGUILayout.Foldout(m_ShellFoldout, "Shell");
		if (m_ShellFoldout)
		{

			m_Component.ShellPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", m_Component.ShellPrefab, typeof(GameObject), false);
			GUI.enabled = false;
			GUILayout.Label("Prefab should be a mesh with a collider, a rigidbody\nand a vp_Shell script added to it.", m_NoteStyle);
			GUI.enabled = true;
			m_Component.ShellScale = EditorGUILayout.Slider("Scale", m_Component.ShellScale, 0, 1);
			m_Component.ShellEjectPosition = EditorGUILayout.Vector3Field("Eject Position", m_Component.ShellEjectPosition);
			m_Component.ShellEjectDirection = EditorGUILayout.Vector3Field("Eject Direction", m_Component.ShellEjectDirection);
			m_Component.ShellEjectVelocity = EditorGUILayout.Slider("Eject Velocity", m_Component.ShellEjectVelocity, 0, 0.5f);
			m_Component.ShellEjectSpin = EditorGUILayout.Slider("Eject Spin", m_Component.ShellEjectSpin, 0, 1.0f);
			m_Component.ShellEjectDelay = Mathf.Abs(EditorGUILayout.FloatField("Eject Delay", m_Component.ShellEjectDelay));
			vp_EditorGUIUtility.Separator();

		}

		// --- Sound ---
		m_SoundFoldout = EditorGUILayout.Foldout(m_SoundFoldout, "Sound");
		if (m_SoundFoldout)
		{
			m_Component.SoundFire = (AudioClip)EditorGUILayout.ObjectField("Fire", m_Component.SoundFire, typeof(AudioClip), false);
			m_Component.SoundFirePitch = EditorGUILayout.Vector2Field("Fire Pitch (Min:Max)", m_Component.SoundFirePitch);
			EditorGUILayout.MinMaxSlider(ref m_Component.SoundFirePitch.x, ref m_Component.SoundFirePitch.y, 0.5f, 1.5f);
			vp_EditorGUIUtility.Separator();
		}

		m_PresetFoldout = EditorGUILayout.Foldout(m_PresetFoldout, "Preset");
		if (m_PresetFoldout)
		{

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Load"))
				ShowLoadDialog();
			if (GUILayout.Button("Save"))
				ShowSaveDialog();
			EditorGUILayout.EndHorizontal();

			bool oldPersistState = m_Component.Persist;
			m_Component.Persist = vp_EditorGUIUtility.ButtonToggle("Persist Play Mode Changes", m_Component.Persist);
			if (oldPersistState != m_Component.Persist)
			{
				if (m_Component.Persist)
					vp_ComponentPersister.Persist(m_Component);
			}

		}

		if (GUI.changed)
		{
			m_Dirty = true;
			if (m_Component.Persist)
				vp_ComponentPersister.Persist(m_Component);
			m_Component.RefreshSettings();
		}

	}

	
	///////////////////////////////////////////////////////////
	// opens a dialog for loading camera presets
	///////////////////////////////////////////////////////////
	private void ShowLoadDialog()
	{
		ShowLoadDialog(true);
	}
	private void ShowLoadDialog(bool checkDirty)
	{
		if (m_Dirty && checkDirty)
		{
			vp_MessageBox.Create(vp_MessageBox.Mode.YesNoCancel, "Save?", "This component has unsaved changes. Would you like to save the changes now?", delegate(vp_MessageBox.Answer answer)
			{
				switch (answer)
				{
					case vp_MessageBox.Answer.Yes: ShowSaveDialog(true); return;
					case vp_MessageBox.Answer.No: ShowLoadDialog(false); return;
					case vp_MessageBox.Answer.Cancel: return;
				}
			});
		}
		else
		{
			string path = Application.dataPath.Replace("\\", "/");
			vp_FileDialog.Create(vp_FileDialog.Mode.Open, "Load Preset", path, delegate(string filename)
			{
				vp_ComponentPreset.Load(m_Component, filename);
				m_Dirty = false;
			}, ".txt");
		}

	}
	

	///////////////////////////////////////////////////////////
	// opens a dialog for saving weapon presets
	///////////////////////////////////////////////////////////
	private void ShowSaveDialog()
	{
		ShowSaveDialog(false);
	}
	private void ShowSaveDialog(bool showLoadDialogAfterwards)
	{
		string path = Application.dataPath;

		vp_FileDialog.Create(vp_FileDialog.Mode.Save, "Save Preset", path, delegate(string filename)
		{
			vp_ComponentPreset.Save(m_Component, filename);
			m_Dirty = false;

			if (showLoadDialogAfterwards)
				ShowLoadDialog();

		}, ".txt");


	}
	

	///////////////////////////////////////////////////////////
	// sets up any special gui styles
	///////////////////////////////////////////////////////////
	private void InitGUIStyles()
	{

		m_NoteStyle = new GUIStyle("Label");
		m_NoteStyle.fontSize = 9;
		m_NoteStyle.alignment = TextAnchor.LowerCenter;

		m_GUIStylesInitialized = true;

	}


}

