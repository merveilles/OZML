/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSCameraEditor.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	custom inspector for the vp_FPSCamera class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(vp_FPSCamera))]

public class vp_FPSCameraEditor : Editor
{

	// target component
	private vp_FPSCamera m_Component = null;

	// camera foldouts
	private bool m_CameraMouseFoldout = false;
	private bool m_CameraRenderingFoldout = false;
	private bool m_CameraRotationFoldout = false;
	private bool m_CameraPositionFoldout = false;
	private bool m_CameraShakeFoldout = false;
	private bool m_CameraBobFoldout = false;
	private bool m_PresetFoldout = false;

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

		m_Component = (vp_FPSCamera)target;

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

		// --- Mouse ---
		m_CameraMouseFoldout = EditorGUILayout.Foldout(m_CameraMouseFoldout, "Mouse");
		if (m_CameraMouseFoldout)
		{
			m_Component.MouseSensitivity = EditorGUILayout.Vector2Field("Sensitivity", m_Component.MouseSensitivity);
			m_Component.MouseSmoothSteps = EditorGUILayout.IntSlider("Smooth Steps", m_Component.MouseSmoothSteps, 1, 20);
			m_Component.MouseSmoothWeight = EditorGUILayout.Slider("Smooth Weight", m_Component.MouseSmoothWeight, 0, 1);
			m_Component.MouseAcceleration = EditorGUILayout.Toggle("Acceleration", m_Component.MouseAcceleration);
			if (!m_Component.MouseAcceleration)
				GUI.enabled = false;
			m_Component.MouseAccelerationThreshold = EditorGUILayout.Slider("Acc. Threshold", m_Component.MouseAccelerationThreshold, 0, 5);
			GUI.enabled = true;
				
			vp_EditorGUIUtility.Separator();
		}
			
		// --- Rendering ---
		m_CameraRenderingFoldout = EditorGUILayout.Foldout(m_CameraRenderingFoldout, "Rendering");
		if (m_CameraRenderingFoldout)
		{
			Vector2 fovDirty = new Vector2(m_Component.RenderingFieldOfView, 0.0f);
			m_Component.RenderingFieldOfView = EditorGUILayout.Slider("Field of View", m_Component.RenderingFieldOfView, 1, 179);
			if (fovDirty != new Vector2(m_Component.RenderingFieldOfView, 0.0f))
				m_Component.Zoom();
			m_Component.RenderingZoomDamping = EditorGUILayout.Slider("Zoom Damping", m_Component.RenderingZoomDamping, 0.1f, 5.0f);

			vp_EditorGUIUtility.Separator();
		}

		// --- Position ---
		m_CameraPositionFoldout = EditorGUILayout.Foldout(m_CameraPositionFoldout, "Position");
		if (m_CameraPositionFoldout)
		{
			m_Component.PositionOffset = EditorGUILayout.Vector3Field("Offset", m_Component.PositionOffset);
			m_Component.PositionOffset.y = Mathf.Max(m_Component.PositionGroundLimit, m_Component.PositionOffset.y);
			m_Component.PositionGroundLimit = EditorGUILayout.Slider("Ground Limit", m_Component.PositionGroundLimit, -5 , 0);
			m_Component.PositionSpringStiffness = EditorGUILayout.Slider("Spring Stiffness", m_Component.PositionSpringStiffness, 0, 1);
			m_Component.PositionSpringDamping = EditorGUILayout.Slider("Spring Damping", m_Component.PositionSpringDamping, 0, 1);
			m_Component.PositionSpring2Stiffness = EditorGUILayout.Slider("Spring2 Stiffn.", m_Component.PositionSpring2Stiffness, 0, 1);
			m_Component.PositionSpring2Damping = EditorGUILayout.Slider("Spring2 Damp.", m_Component.PositionSpring2Damping, 0, 1);
			m_Component.PositionKneeling = EditorGUILayout.Slider("Kneeling", m_Component.PositionKneeling, 0, 0.5f);
			GUI.enabled = false;
			GUILayout.Label("Spring2 is a scripting feature. See the docs for usage.", m_NoteStyle);
			GUI.enabled = true;

			vp_EditorGUIUtility.Separator();
		}
			
		// --- Rotation ---
		m_CameraRotationFoldout = EditorGUILayout.Foldout(m_CameraRotationFoldout, "Rotation");
		if (m_CameraRotationFoldout)
		{
			m_Component.RotationPitchLimit = EditorGUILayout.Vector2Field("Pitch Limit (Min:Max)", m_Component.RotationPitchLimit);
			EditorGUILayout.MinMaxSlider(ref m_Component.RotationPitchLimit.x, ref m_Component.RotationPitchLimit.y, -90.0f, 90.0f);
			m_Component.RotationYawLimit = EditorGUILayout.Vector2Field("Yaw Limit (Min:Max)", m_Component.RotationYawLimit);
			EditorGUILayout.MinMaxSlider(ref m_Component.RotationYawLimit.x, ref m_Component.RotationYawLimit.y, -360.0f, 360.0f);
			m_Component.RotationKneeling = EditorGUILayout.Slider("Kneeling", m_Component.RotationKneeling, 0, 0.5f);
			m_Component.RotationStrafeRoll = EditorGUILayout.Slider("Strafe Roll", m_Component.RotationStrafeRoll, -5.0f, 5.0f);

			vp_EditorGUIUtility.Separator();
		}
			
		// --- Shake ---
		m_CameraShakeFoldout = EditorGUILayout.Foldout(m_CameraShakeFoldout, "Shake");
		if (m_CameraShakeFoldout)
		{
			m_Component.ShakeSpeed = EditorGUILayout.Slider("Speed", m_Component.ShakeSpeed, 0, 1);
			m_Component.ShakeAmplitude = EditorGUILayout.Vector3Field("Amplitude", m_Component.ShakeAmplitude);

			vp_EditorGUIUtility.Separator();
		}

		// --- Bob ---
		m_CameraBobFoldout = EditorGUILayout.Foldout(m_CameraBobFoldout, "Bob");
		if (m_CameraBobFoldout)
		{
			m_Component.BobRate = EditorGUILayout.Vector4Field("Rate", m_Component.BobRate);
			m_Component.BobAmplitude = EditorGUILayout.Vector4Field("Amplitude", m_Component.BobAmplitude);
			GUI.enabled = false;
			GUILayout.Label("XYZ is positional bob... W is roll around forward vector.", m_NoteStyle);
			GUI.enabled = true;
			m_Component.BobMaxInputVelocity = EditorGUILayout.FloatField("Max Input Vel.", m_Component.BobMaxInputVelocity);
			m_Component.BobStepThreshold = EditorGUILayout.FloatField("Step Threshold", m_Component.BobStepThreshold);
			GUI.enabled = false;
			GUILayout.Label("Step Threshold is a scripting feature. See docs for usage.", m_NoteStyle);
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
				m_Component.BobStepCallback = null;
				m_Component.RefreshSettings(); 
				m_Dirty = false;
			}, ".txt");
		}

	}
	

	///////////////////////////////////////////////////////////
	// opens a dialog for saving camera presets
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

