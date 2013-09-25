/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPSControllerEditor.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	custom inspector for the vp_FPSController class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(vp_FPSController))] 
public class vp_FPSControllerEditor : Editor
{

	// target component
	private vp_FPSController m_Component;

	// foldouts
	private bool m_MotorFoldout = true;
	private bool m_PhysicsFoldout = true;
	private bool m_PresetFoldout = true;
	
	private bool m_Dirty = false;


	///////////////////////////////////////////////////////////
	// hooks up the FPSController object as the inspector target
	///////////////////////////////////////////////////////////
	void OnEnable()
	{

		m_Component = (vp_FPSController)target;

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

		m_MotorFoldout = EditorGUILayout.Foldout(m_MotorFoldout, "Motor");
		if (m_MotorFoldout)
		{

			m_Component.MotorAcceleration = EditorGUILayout.Slider("Acceleration", m_Component.MotorAcceleration, 0, 1);
			m_Component.MotorDamping = EditorGUILayout.Slider("Damping", m_Component.MotorDamping, 0, 1);
			m_Component.MotorJumpForce = EditorGUILayout.Slider("Jump Force", m_Component.MotorJumpForce, 0, 1);
			m_Component.MotorAirSpeed = EditorGUILayout.Slider("Air Speed", m_Component.MotorAirSpeed, 0, 1);
			m_Component.MotorSlopeSpeedUp = EditorGUILayout.Slider("Slope Speed Up", m_Component.MotorSlopeSpeedUp, 0, 2);
			m_Component.MotorSlopeSpeedDown = EditorGUILayout.Slider("Slope Sp. Down", m_Component.MotorSlopeSpeedDown, 0, 2);

		}

		m_PhysicsFoldout = EditorGUILayout.Foldout(m_PhysicsFoldout, "Physics");
		if (m_PhysicsFoldout)
		{

			m_Component.PhysicsForceDamping = EditorGUILayout.Slider("Force Damping", m_Component.PhysicsForceDamping, 0, 1);
			m_Component.PhysicsPushForce = EditorGUILayout.Slider("Push Force", m_Component.PhysicsPushForce, 0, 100);
			m_Component.PhysicsGravityModifier = EditorGUILayout.Slider("Gravity Modifier", m_Component.PhysicsGravityModifier, 0, 1);
			m_Component.PhysicsWallBounce = EditorGUILayout.Slider("Wall Bounce", m_Component.PhysicsWallBounce, 0, 1);

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
				if (m_Component.Persist)
					vp_ComponentPersister.Persist(m_Component);
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
			if (m_Component.Persist)
				vp_ComponentPersister.Persist(m_Component);
			m_Dirty = false;

			if (showLoadDialogAfterwards)
				ShowLoadDialog();

		}, ".txt");


	}
	
}

