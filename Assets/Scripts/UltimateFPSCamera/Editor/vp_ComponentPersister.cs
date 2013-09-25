/////////////////////////////////////////////////////////////////////////////////
//
//	vp_ComponentPersister.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	persists changes made to a component at runtime. hook this up
//					by putting the following code in the 'OnEnable'method of your
//					component's editor class.
//
//	EditorApplication.playmodeStateChanged = delegate()
//	{
//		vp_ComponentPersister.m_Component = (Component)target;
//		vp_ComponentPersister.PlayModeCallback();
//	};
//
//					also, your component must have a public bool called 'Persist'.
//					as long as this is set to 'true', the component will be
//					persisted every time the application is stopped.
//
//					NOTE: the component persister will only work while your
//					component is selected in the inspector, that is: only one
//					component can be persisted at a time.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;


public sealed class vp_ComponentPersister
{

	private static bool m_IsPlaying = false;		// used to properly detect when the application starts or stops
	public static Component m_Component = null;		// component to persist

	public static readonly vp_ComponentPersister instance = new vp_ComponentPersister();
	public static List<object> m_PlayModePersistFields = new List<object>();


	///////////////////////////////////////////////////////////
	// explicit static constructor to tell c# compiler
	// not to mark type as beforefieldinit
	///////////////////////////////////////////////////////////
	static vp_ComponentPersister()	{}


	///////////////////////////////////////////////////////////
	// constructor
	///////////////////////////////////////////////////////////
	private vp_ComponentPersister()
	{
	}


	///////////////////////////////////////////////////////////
	// called every time the editor changes its play mode, provided
	// that the callback has been set up as detailed in the header
	// of this file
	///////////////////////////////////////////////////////////
	public static void PlayModeCallback()
	{

		if(m_Component == null)
		{
			Debug.LogError("Error! vp_ComponentPersister: Component is null.");
			return;
		}

		if (!m_IsPlaying && EditorApplication.isPlaying)
		{
			// if we end up here, we have detected that the application
			// was started, so store all the component's variables

			// 'm_IsPlaying' is used to only trigger this once per application
			// start (the callback is triggered several times per start).
			m_IsPlaying = true;
			Persist((Component)m_Component);
			return;
		}

		if (EditorApplication.isCompiling ||
			EditorApplication.isPaused ||
			EditorApplication.isPlaying ||
			EditorApplication.isPlayingOrWillChangePlaymode)
			return;
		
		// if we end up here, we have detected that the application
		// was stopped, so see if the component should be restored

		if (GetPersistState() == false)
			return;

		PersistRestore();
		
	}
	

	///////////////////////////////////////////////////////////
	// backs up the fields of the component. this should be done
	// every time the component is modified in the inspector.
	// for example, in your component editor class:
	//	if (GUI.changed)
	//		vp_ComponentPersister.Persist(target);
	///////////////////////////////////////////////////////////
	public static void Persist(Component component)
	{

		if (component == null)
		{
			Debug.LogError("Error! vp_ComponentPersister: Component is null.");
			return;
		}

		m_Component = component;
		m_PlayModePersistFields.Clear();

		foreach (FieldInfo f in m_Component.GetType().GetFields())
		{
			//Debug.Log("(" + f.FieldType.Name + ") " + f.Name + " = " + f.GetValue(m_Controller));
			m_PlayModePersistFields.Add(f.GetValue(m_Component));
		}
	}


	///////////////////////////////////////////////////////////
	// restores the backed up fields. this is called every time
	// the application is stopped, if 'GetPersistState' is true
	///////////////////////////////////////////////////////////
	private static void PersistRestore()
	{
		
		// or persist the rest of the values
		int v = 0;
		foreach (FieldInfo f in m_Component.GetType().GetFields())
		{

			if (f.FieldType == typeof(float) ||
			f.FieldType == typeof(Vector4) ||
			f.FieldType == typeof(Vector3) ||
			f.FieldType == typeof(Vector2) ||
			f.FieldType == typeof(int) ||
			f.FieldType == typeof(bool) ||
			f.FieldType == typeof(string))
			{
				f.SetValue(m_Component, m_PlayModePersistFields[v]);
			}
			else
			{
				//Debug.LogError("Warning! vp_ComponentPersister can't persist type '" + f.FieldType.Name.ToString() + "'");
			}
			v++;
		}

	}


	///////////////////////////////////////////////////////////
	// returns true if target component has a bool called 'Persist'
	// which is currently set to 'true', otherwise returns false
	///////////////////////////////////////////////////////////
	private static bool GetPersistState()
	{

		bool state = false;

		try
		{

			// first fetch current persist state. it must always be
			// persisted and may have changed during play.
			int d = 0;
			foreach (FieldInfo f in m_Component.GetType().GetFields())
			{
				// if there is a field called 'Persist' and it is true,
				// this method will return true, otherwise false.
				if (f.Name == "Persist")
				{
					state = (bool)m_PlayModePersistFields[d];
				}
				d++;
			}

		}
		catch
		{
			// if we end up here there has been some kind of exception
			// (usually the result of re-compilation or a funky editor
			// state and nothing to worry about).
//			Debug.LogError("Failed to get persist state.");
		}

		return state;

	}


}






