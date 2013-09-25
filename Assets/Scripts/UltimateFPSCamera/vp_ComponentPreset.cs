/////////////////////////////////////////////////////////////////////////////////
//
//	vp_ComponentPreset.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	loads and saves component field values to text scripts
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;


public sealed class vp_ComponentPreset
{

	public static readonly vp_ComponentPreset instance = new vp_ComponentPreset();

	public static Component m_Component = null;				// component to save or load
	private static string m_FullPath = null;				// path to save or load from
	private static ReadMode m_ReadMode = ReadMode.Normal;	// determines whether we are reading commands or comments
	private static int m_LineNumber = 0;					// the current line being read

	private enum ReadMode
	{
		Normal,
		LineComment,
		BlockComment
	}
	

	///////////////////////////////////////////////////////////
	// explicit static constructor to tell c# compiler
	// not to mark type as beforefieldinit
	///////////////////////////////////////////////////////////
	static vp_ComponentPreset()	{}


	///////////////////////////////////////////////////////////
	// constructor
	///////////////////////////////////////////////////////////
	private vp_ComponentPreset()
	{
	}


	///////////////////////////////////////////////////////////
	// saves every supported field of 'component' to a text
	// file at 'fullPath'
	///////////////////////////////////////////////////////////
	public static void Save(Component component, string fullPath)
	{

		m_Component = component;
		m_FullPath = fullPath;

		ClearTextFile();

		Append("///////////////////////////////////////////////////////////");
		Append("// Component Preset Script");
		Append("///////////////////////////////////////////////////////////\n");

		// append component type
		Append("ComponentType " + component.GetType().Name);

		// scan component for all its fields. NOTE: any types
		// to be supported must be included here.
		foreach (FieldInfo f in component.GetType().GetFields())
		{

			string prefix = "";
			string value = "";
			if (f.FieldType == typeof(float))
				value = String.Format("{0:0.#######}", ((float)f.GetValue(component)));
			else if (f.FieldType == typeof(Vector4))
			{
				Vector4 val = ((Vector4)f.GetValue(component));
				value = String.Format("{0:0.#######}", val.x) + " " +
						String.Format("{0:0.#######}", val.y) + " " +
						String.Format("{0:0.#######}", val.z) + " " +
						String.Format("{0:0.#######}", val.w);
			}
			else if (f.FieldType == typeof(Vector3))
			{
				Vector3 val = ((Vector3)f.GetValue(component));
				value = String.Format("{0:0.#######}", val.x) + " " +
						String.Format("{0:0.#######}", val.y) + " " +
						String.Format("{0:0.#######}", val.z);
			}
			else if (f.FieldType == typeof(Vector2))
			{
				Vector2 val = ((Vector2)f.GetValue(component));
				value = String.Format("{0:0.#######}", val.x) + " " +
						String.Format("{0:0.#######}", val.y);
			}
			else if (f.FieldType == typeof(int))
				value = ((int)f.GetValue(component)).ToString();
			else if (f.FieldType == typeof(bool))
				value = ((bool)f.GetValue(component)).ToString();
			else if (f.FieldType == typeof(string))
				value = ((string)f.GetValue(component));
			else
			{
				prefix = "//";
				value = "<NOTE: Type '" + f.FieldType.Name.ToString() + "' can't be saved to preset.>";
			}

			// print field name and value to the text file
			if(!string.IsNullOrEmpty(value) && f.Name!= "Persist")
				Append(prefix + f.Name + " " + value);

		}
		
	}
		

	///////////////////////////////////////////////////////////
	// reads the text file at 'fullPath' and parses every line
	// as field to be set on 'component'
	///////////////////////////////////////////////////////////
	public static bool Load(Component component, string fullPath)
	{

		m_Component = component;
		m_FullPath = fullPath;

		// if running in webplayer mode, we can only read the file
		// from within a 'Resources' folder.
		if (Application.isWebPlayer)
			return LoadFromResources(component, fullPath);

		// the rest of this method won't compile in a webplayer because
		// it uses 'System.IO.FileInfo.OpenText' in order to read files
		// outside the 'Resources' folder.

		// NOTE: the Unity Editor will stay in the platform mode to
		// which it last compiled. if this method fails to read a
		// text file in editor mode after you have built a webplayer;
		// open the Build Settings, choose 'PC and Mac Standalone'
		// and press the 'Switch Platform' button.

#if !UNITY_WEBPLAYER

		// if we end up here, we're running in the editor or a
		// standalone build, where we can load files from
		// outside the 'Resources' folder.

		FileInfo fileInfo = null;
		TextReader file = null;

		// load file as text
		fileInfo = new FileInfo(m_FullPath);
		if (fileInfo != null && fileInfo.Exists)
		{
			file = fileInfo.OpenText();
		}
		else
		{
			Debug.LogError("Failed to read file." + " '" + m_FullPath + "'");
			return false;
		}

		// extract lines of text from the TextReader
		string txt;
		List<string> lines = new List<string>();
		while ((txt = file.ReadLine()) != null)
		{
			lines.Add(txt);
		}

		if (lines == null)
		{
			Debug.LogError("Preset is empty." + " '" + m_FullPath + "'");
			return false;
		}

		// execute the lines in the preset one by one
		ParseLines(lines);

		file.Close();

#endif

		return true;

	}


	///////////////////////////////////////////////////////////
	// reads a text file from the resources folder and parses
	// every line as a command
	///////////////////////////////////////////////////////////
	public static bool LoadFromResources(Component component, string resourcePath)
	{

		// NOTE: keep in mind that for resource loading the file 
		// extension must always be omitted.

		m_Component = component;
		m_FullPath = resourcePath;

		// load text file as textasset
		TextAsset file = Resources.Load(m_FullPath) as TextAsset;
		if (file == null)
		{
			Debug.LogError("Failed to read file." + " '" + m_FullPath + "'");
			return false;
		}

		// split textasset into lines
		string[] splitLines = file.text.Split('\n');
		List<string> lines = new List<string>();
		foreach (string s in splitLines)
		{
			lines.Add(s);
		}

		if (lines == null)
		{
			Debug.LogError("Preset is empty." + " '" + m_FullPath + "'");
			return false;
		}

		// execute the lines in the preset one by one
		ParseLines(lines);

		return true;
	}


	///////////////////////////////////////////////////////////
	// writes a single line of text to the file at 'm_FullPath'
	///////////////////////////////////////////////////////////
	private static void Append(string str)
	{

		// replace newlines
		str = str.Replace("\n", System.Environment.NewLine);

		try
		{
			StreamWriter file = null;
			file = new StreamWriter(m_FullPath, true);
			file.WriteLine(str);
			if (file != null)
				file.Close();
		}
		catch
		{
			Debug.LogError("Failed to write to file: '" + m_FullPath + "'");
		}

	}


	///////////////////////////////////////////////////////////
	// clears the text file at 'm_FullPath'
	///////////////////////////////////////////////////////////
	private static void ClearTextFile()
	{

		try
		{
			StreamWriter file = null;
			file = new StreamWriter(m_FullPath, false);
			if (file != null)
				file.Close();
		}
		catch
		{
			Debug.LogError("Failed to clear file: '" + m_FullPath + "'");
		}

	}


	///////////////////////////////////////////////////////////
	// sends every string in the 'lines' list to the parser
	///////////////////////////////////////////////////////////
	private static void ParseLines(List<string> lines)
	{

		// reset comment mode, i.e. for cases when we were in
		// block comment mode when the previous file ended.
		m_ReadMode = ReadMode.Normal;

		// reset line number
		m_LineNumber = 0;

		// feed all lines to parser
		foreach (string s in lines)
		{

			m_LineNumber++;

			// ignore line- and block comments
			string line = RemoveComments(s);

			// if line is empty here, cancel
			if (string.IsNullOrEmpty(line))
				continue;

			// done, try executing the line
			if (!Parse(line))
				return;

		}

		// reset line number again. it should always be zero
		// outside of the above loop
		m_LineNumber = 0;

	}


	///////////////////////////////////////////////////////////
	// parses a string for field name and values and, if they
	// seem healthy, sets them on the component.
	// if this method returns false, 'ParseLines' will stop.
	///////////////////////////////////////////////////////////
	static private bool Parse(string line)
	{

		line = line.Trim();

		if (string.IsNullOrEmpty(line))
		{
			// return since we have nothing to parse, but don't
			// treat this as an error
			return true;
		}

		// create an array with the tokens
		string[] tokens = line.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
		for (int v = 0; v < tokens.Length; v++)
		{
			tokens[v] = tokens[v].Trim();
		}

		if (tokens[0] == "ComponentType")
		{

			if (tokens.Length < 2)
			{
				PresetError("Component type missing.");
				// return and abort further parsing
				return false;
			}

			if (tokens[1] != m_Component.GetType().Name)
			{
				PresetError("Wrong component type: '" + tokens[1] + "'");
				// return and abort further parsing
				return false;
			}

			// return since 'ComponentType' is not a field in the component,
			// but allow further parsing
			return true;

		}

		// see if the component has a field with the same name as
		// the first token
		FieldInfo field = null;
		foreach (FieldInfo f in m_Component.GetType().GetFields())
		{
			if (f.Name == tokens[0])
				field = f;
		}

		// return if the field does not exist in the component
		if (field == null)
		{
			PresetError("'" + m_Component.GetType().Name + "' has no such field: '" + tokens[0] + "'");
			// return, but allow further parsing
			return true;
		}
		
		// attempt to set field to the arguments
		if (field.FieldType == typeof(float))
			field.SetValue(m_Component, ArgsToFloat(tokens));
		else if (field.FieldType == typeof(Vector4))
			field.SetValue(m_Component, ArgsToVector4(tokens));
		else if (field.FieldType == typeof(Vector3))
			field.SetValue(m_Component, ArgsToVector3(tokens));
		else if (field.FieldType == typeof(Vector2))
			field.SetValue(m_Component, ArgsToVector2(tokens));
		else if (field.FieldType == typeof(int))
			field.SetValue(m_Component, ArgsToInt(tokens));
		else if (field.FieldType == typeof(bool))
			field.SetValue(m_Component, ArgsToBool(tokens));
		else if (field.FieldType == typeof(string))
			field.SetValue(m_Component, ArgsToString(tokens));

		// return and allow further parsing
		return true;

	}


	///////////////////////////////////////////////////////////
	// removes line and block comments from a string.
	// preset scripts support both traditional C line '//' and
	// /* block comments */
	///////////////////////////////////////////////////////////
	private static string RemoveComments(string str)
	{

		string result = "";

		for (int v = 0; v < str.Length; v++)
		{
			switch (m_ReadMode)
			{

				// in Normal mode, we usually copy text but go into
				// BlockComment mode upon /* and into LineComment mode upon //
				case ReadMode.Normal:
					if (str[v] == '/' && str[v + 1] == '*')
					{
						m_ReadMode = ReadMode.BlockComment;
						v++;
						break;
					}
					else if (str[v] == '/' && str[v + 1] == '/')
					{
						m_ReadMode = ReadMode.LineComment;
						v++;
						break;
					}

					// copy non-comment text
					result += str[v];

					break;

				// in LineComment mode, we go into Normal mode upon newline
				case ReadMode.LineComment:
					if (v == str.Length - 1)
					{
						m_ReadMode = ReadMode.Normal;
						break;
					}
					break;

				// in BlockComment mode, we go into normal mode upon */
				case ReadMode.BlockComment:
					if (str[v] == '*' && str[v + 1] == '/')
					{
						m_ReadMode = ReadMode.Normal;
						v++;
						break;
					}
					break;

			}
		}

		return result;

	}


	///////////////////////////////////////////////////////////
	// methods to convert from a preset command into target
	// value types. will report a preset error if the field
	// amount is wrong, or if the values are illegal.
	///////////////////////////////////////////////////////////
	private static Vector4 ArgsToVector4(string[] args)
	{

		Vector4 v;

		if ((args.Length - 1) != 4)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return Vector4.zero;
		}

		try
		{
			v = new Vector4(Convert.ToSingle(args[1]), Convert.ToSingle(args[2]),
							Convert.ToSingle(args[3]), Convert.ToSingle(args[4]));
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + ", " + args[2] + ", " + args[3] + ", " + args[4] + "'");
			return Vector4.zero;
		}
		return v;

	}

	private static Vector3 ArgsToVector3(string[] args)
	{

		Vector3 v;

		if ((args.Length - 1) != 3)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return Vector3.zero;
		}

		try
		{
			v = new Vector3(Convert.ToSingle(args[1]), Convert.ToSingle(args[2]), Convert.ToSingle(args[3]));
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + ", " + args[2] + ", " + args[3] + "'");
			return Vector3.zero;
		}
		return v;

	}

	private static Vector2 ArgsToVector2(string[] args)
	{

		Vector2 v;

		if ((args.Length - 1) != 2)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return Vector2.zero;
		}

		try
		{
			v = new Vector2(Convert.ToSingle(args[1]), Convert.ToSingle(args[2]));
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + ", " + args[2] + "'");
			return Vector2.zero;
		}
		return v;

	}

	private static float ArgsToFloat(string[] args)
	{

		float f;

		if ((args.Length - 1) != 1)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return 0.0f;
		}

		try
		{
			f = Convert.ToSingle(args[1]);
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + "'");
			return 0.0f;
		}
		return f;

	}

	private static int ArgsToInt(string[] args)
	{

		int i;

		if ((args.Length - 1) != 1)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return 0;
		}

		try
		{
			i = Convert.ToInt32(args[1]);
		}
		catch
		{
			PresetError("Illegal value: '" + args[1] + "'");
			return 0;
		}
		return i;

	}

	private static bool ArgsToBool(string[] args)
	{

		if ((args.Length - 1) != 1)
		{
			PresetError("Wrong number of fields for '" + args[0] + "'");
			return false;
		}

		if (args[1].ToLower() == "true")
			return true;
		else if (args[1].ToLower() == "false")
			return false;

		PresetError("Illegal value: '" + args[1] + "'");
		return false;

	}
	

	private static string ArgsToString(string[] args)
	{

		string s = "";

		// put all arguments, with spaces inbetween,
		// into a single string
		for(int v = 1; v<args.Length; v++)
		{
			s += args[v];
			if (v < args.Length-1)
				s += " ";
		}

		return s;

	}


	///////////////////////////////////////////////////////////
	// logs a preset error to the console
	///////////////////////////////////////////////////////////
	private static void PresetError(string message)
	{
		Debug.LogError("Preset Error: " + m_FullPath + " (at " + m_LineNumber + ") " + message);
	}
	
	
}






