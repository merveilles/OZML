/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSWeaponEditor.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	custom inspector for the vp_FPSWeapon class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(vp_FPSWeapon))]
public class vp_FPSWeaponEditor : Editor
{

	// target component
	private vp_FPSWeapon m_Component = null;

	// weapon foldouts
	private bool m_WeaponRenderingFoldout = false;
	private bool m_WeaponRotationFoldout = false;
	private bool m_WeaponPositionFoldout = false;
	private bool m_WeaponShakeFoldout = false;
	private bool m_WeaponBobFoldout = false;
	private bool m_PresetFoldout = false;

	// pivot
	private bool m_WeaponPivotVisible = false;

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

		m_Component = (vp_FPSWeapon)target;

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

		GUILayout.Label(objectInfo);
		vp_EditorGUIUtility.Separator();

		if (!m_Component.gameObject.active)
		{
			GUI.enabled = true;
			return;
		}

		// --- Rendering ---
		m_WeaponRenderingFoldout = EditorGUILayout.Foldout(m_WeaponRenderingFoldout, "Rendering");
		if (m_WeaponRenderingFoldout)
		{

			// weapon fov
			Vector2 fovDirty = new Vector2(0.0f, m_Component.RenderingFieldOfView);
			m_Component.RenderingFieldOfView = EditorGUILayout.Slider("Field of View", m_Component.RenderingFieldOfView, 1, 179);
			if (fovDirty != new Vector2(0.0f, m_Component.RenderingFieldOfView))
				m_Component.Zoom();
			m_Component.RenderingZoomDamping = EditorGUILayout.Slider("Zoom Damping", m_Component.RenderingZoomDamping, 0.1f, 5.0f);
			m_Component.RenderingClippingPlanes = EditorGUILayout.Vector2Field("Clipping Planes (Near:Far)", m_Component.RenderingClippingPlanes);
			GUI.enabled = false;
			GUILayout.Label("To add weapons, add child GameObjects to the\nFPSCamera transform and add FPSWeapon\nscripts to them. See the docs for more info.", m_NoteStyle);
			GUI.enabled = true;

			vp_EditorGUIUtility.Separator();

		}

		// --- Position ---
		m_WeaponPositionFoldout = EditorGUILayout.Foldout(m_WeaponPositionFoldout, "Position");
		if (m_WeaponPositionFoldout)
		{
			m_Component.PositionOffset = EditorGUILayout.Vector3Field("Offset", m_Component.PositionOffset);
			Vector3 currentPivot = m_Component.PositionPivot;
			m_Component.PositionPivot = EditorGUILayout.Vector3Field("Pivot", m_Component.PositionPivot);
			m_Component.PositionPivotSpringStiffness = EditorGUILayout.Slider("Pivot Stiffness", m_Component.PositionPivotSpringStiffness, 0, 1);
			m_Component.PositionPivotSpringDamping = EditorGUILayout.Slider("Pivot Damping", m_Component.PositionPivotSpringDamping, 0, 1);

			if (!Application.isPlaying)
				GUI.enabled = false;
			bool currentPivotVisible = m_WeaponPivotVisible;
			m_WeaponPivotVisible = EditorGUILayout.Toggle("Show Pivot", m_WeaponPivotVisible);
			if (Application.isPlaying)
			{
				if (m_Component.PositionPivot != currentPivot)
				{
					m_Component.SnapPivot();
					m_WeaponPivotVisible = true;
				}
				if (currentPivotVisible != m_WeaponPivotVisible)
					m_Component.SetPivotVisible(m_WeaponPivotVisible);
				GUI.enabled = false;
				GUILayout.Label("Set Pivot Z to about -0.5 to bring it into view.", m_NoteStyle);
				GUI.enabled = true;
			}
			else
				GUILayout.Label("Pivot can be shown when the game is playing.", m_NoteStyle);

			GUI.enabled = true;

			m_Component.PositionSpringStiffness = EditorGUILayout.Slider("Spring Stiffness", m_Component.PositionSpringStiffness, 0, 1);
			m_Component.PositionSpringDamping = EditorGUILayout.Slider("Spring Damping", m_Component.PositionSpringDamping, 0, 1);
			m_Component.PositionSpring2Stiffness = EditorGUILayout.Slider("Spring2 Stiffn.", m_Component.PositionSpring2Stiffness, 0, 1);
			m_Component.PositionSpring2Damping = EditorGUILayout.Slider("Spring2 Damp.", m_Component.PositionSpring2Damping, 0, 1);
			GUI.enabled = false;
			GUILayout.Label("Spring2 is intended for recoil. See the docs for usage.", m_NoteStyle);
			GUI.enabled = true;
			m_Component.PositionKneeling = EditorGUILayout.Slider("Kneeling", m_Component.PositionKneeling, 0, 1);
			m_Component.PositionFallRetract = EditorGUILayout.Slider("Fall Retract", m_Component.PositionFallRetract, 0, 10);
			m_Component.PositionWalkSlide = EditorGUILayout.Vector3Field("Walk Sliding", m_Component.PositionWalkSlide);

			vp_EditorGUIUtility.Separator();
		}

		// --- Rotation ---
		m_WeaponRotationFoldout = EditorGUILayout.Foldout(m_WeaponRotationFoldout, "Rotation");
		if (m_WeaponRotationFoldout)
		{
			m_Component.RotationOffset = EditorGUILayout.Vector3Field("Offset", m_Component.RotationOffset);
			m_Component.RotationSpringStiffness = EditorGUILayout.Slider("Spring Stiffness", m_Component.RotationSpringStiffness, 0, 1);
			m_Component.RotationSpringDamping = EditorGUILayout.Slider("Spring Damping", m_Component.RotationSpringDamping, 0, 1);
			m_Component.RotationSpring2Stiffness = EditorGUILayout.Slider("Spring2 Stiffn.", m_Component.RotationSpring2Stiffness, 0, 1);
			m_Component.RotationSpring2Damping = EditorGUILayout.Slider("Spring2 Damp.", m_Component.RotationSpring2Damping, 0, 1);
			GUI.enabled = false;
			GUILayout.Label("Spring2 is intended for recoil. See the docs for usage.", m_NoteStyle);
			GUI.enabled = true;
			m_Component.RotationLookSway = EditorGUILayout.Vector3Field("Look Sway", m_Component.RotationLookSway);
			m_Component.RotationStrafeSway = EditorGUILayout.Vector3Field("Strafe Sway", m_Component.RotationStrafeSway);
			m_Component.RotationFallSway = EditorGUILayout.Vector3Field("Fall Sway", m_Component.RotationFallSway);
			m_Component.RotationSlopeSway = EditorGUILayout.Slider("Slope Sway", m_Component.RotationSlopeSway, 0, 1);
			GUI.enabled = false;
			GUILayout.Label("SlopeSway multiplies FallSway when grounded\nand will take effect on slopes.", m_NoteStyle);
			GUI.enabled = true;

			vp_EditorGUIUtility.Separator();
		}

		// --- Shake ---
		m_WeaponShakeFoldout = EditorGUILayout.Foldout(m_WeaponShakeFoldout, "Shake");
		if (m_WeaponShakeFoldout)
		{
			m_Component.ShakeSpeed = EditorGUILayout.Slider("Speed", m_Component.ShakeSpeed, 0, 1);
			m_Component.ShakeAmplitude = EditorGUILayout.Vector3Field("Amplitude", m_Component.ShakeAmplitude);

			vp_EditorGUIUtility.Separator();
		}

		// --- Bob ---
		m_WeaponBobFoldout = EditorGUILayout.Foldout(m_WeaponBobFoldout, "Bob");
		if (m_WeaponBobFoldout)
		{
			m_Component.BobRate = EditorGUILayout.Vector4Field("Rate", m_Component.BobRate);
			m_Component.BobAmplitude = EditorGUILayout.Vector4Field("Amplitude", m_Component.BobAmplitude);
			m_Component.BobMaxInputVelocity = EditorGUILayout.FloatField("Max Input Vel.", m_Component.BobMaxInputVelocity);
			GUI.enabled = false;
			GUILayout.Label("XYZ is angular bob... W is position along the\nforward vector. X & Z rate should be (Y/2) for a\nclassic weapon bob.", m_NoteStyle);
			GUI.enabled = true;

			vp_EditorGUIUtility.Separator();
		}
			
		if (GUI.changed)
		{
			m_Dirty = true;
			if (m_Component.Persist)
				vp_ComponentPersister.Persist(m_Component);
			m_Component.RefreshSettings();
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
				m_Component.RefreshSettings(); 
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

