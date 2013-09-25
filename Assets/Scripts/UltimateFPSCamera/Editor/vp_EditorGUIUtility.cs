/////////////////////////////////////////////////////////////////////////////////
//
//	vp_EditorGUIUtility.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	helper methods for standard editor GUI tasks
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;

public static class vp_EditorGUIUtility
{


	///////////////////////////////////////////////////////////
	// creates a foldout button to clearly distinguish a section
	// of controls from others
	///////////////////////////////////////////////////////////
	public static bool SectionButton(string label, bool state)
	{

		GUI.color = new Color(0.9f, 0.9f, 1, 1);
		if (GUILayout.Button((state ? "- " : "+ ") + label.ToUpper(), GUILayout.Height(20)))
			state = !state;
		GUI.color = Color.white;

		return state;

	}


	///////////////////////////////////////////////////////////
	// creates a big 2-button toggle
	///////////////////////////////////////////////////////////
	public static bool ButtonToggle(string label, bool state)
	{

		GUIStyle onStyle = new GUIStyle("Button");
		GUIStyle offStyle = new GUIStyle("Button");

		if (state)
			onStyle.normal = onStyle.active;
		else
			offStyle.normal = offStyle.active;

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label(label);
		if (GUILayout.Button("ON", onStyle))
			state = true;
		if (GUILayout.Button("OFF", offStyle))
			state = false;
		EditorGUILayout.EndHorizontal();

		return state;

	}


	///////////////////////////////////////////////////////////
	// creates a horizontal line to visually separate groups of
	// controls
	///////////////////////////////////////////////////////////
	public static void Separator()
	{

		GUI.color = new Color(1, 1, 1, 0.25f);
		GUILayout.Box("", "HorizontalSlider", GUILayout.Height(16));
		GUI.color = Color.white;

	}


}

