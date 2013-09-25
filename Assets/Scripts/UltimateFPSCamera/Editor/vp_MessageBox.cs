/////////////////////////////////////////////////////////////////////////////////
//
//	vp_MessageBox.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	a classic OS-type message box with a range of buttons and
//					possible return values
//
/////////////////////////////////////////////////////////////////////////////////


using UnityEngine;
using UnityEditor;

public class vp_MessageBox : EditorWindow
{

	static Vector2 m_DialogSize = new Vector2(354, 146);
	static string m_Message;

	public enum Mode
	{
		OK,
		OKCancel,
		AbortRetryIgnore,
		YesNoCancel,
		YesNo,
		RetryCancel
	}
	private static Mode m_Mode = Mode.OK;

	public enum Answer
	{
		OK,
		Cancel,
		Abort,
		Retry,
		Ignore,
		Yes,
		No
	}

	public delegate void Callback(Answer answer);
	private static Callback m_Callback;
	

	///////////////////////////////////////////////////////////
	// this method creates and shows the message box
	///////////////////////////////////////////////////////////
	public static void Create(Mode mode, string caption, string message)
	{
		Create(mode, caption, message, delegate(vp_MessageBox.Answer answer) { });
	}
	public static void Create(Mode mode, string caption, string message, Callback callback)
	{

		vp_MessageBox msgBox = (vp_MessageBox)EditorWindow.GetWindow(typeof(vp_MessageBox));

		msgBox.title = string.IsNullOrEmpty(caption) ? "Message" : caption;
		m_Message = message;
		m_Callback = callback;
		m_Mode = mode;

		msgBox.minSize = new Vector2(m_DialogSize.x, m_DialogSize.y);
		msgBox.maxSize = new Vector2(m_DialogSize.x + 1, m_DialogSize.y + 1);
		msgBox.position = new Rect(
			(Screen.currentResolution.width / 2) - (m_DialogSize.x / 2),
			(Screen.currentResolution.height / 2) - (m_DialogSize.y / 2),
			m_DialogSize.x,
			m_DialogSize.y);
		msgBox.Show();

	}


	///////////////////////////////////////////////////////////
	// draws the dialog
	///////////////////////////////////////////////////////////
	void OnGUI()
	{

		// buttons (done first, or message text will be able to block mouse input)
		GUILayout.BeginArea(new Rect(20, Screen.height - 60, Screen.width - 40, Screen.height - 40));
		GUILayout.BeginHorizontal();
		switch (m_Mode)
		{
			case Mode.OK:				DoOK();	break;
			case Mode.OKCancel:			DoOKCancel(); break;
			case Mode.AbortRetryIgnore:	DoAbortRetryIgnore(); break;
			case Mode.YesNoCancel:		DoYesNoCancel(); break;
			case Mode.YesNo:			DoYesNo(); break;
			case Mode.RetryCancel:		DoRetryCancel(); break;
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();

		// message
		GUILayout.BeginArea(new Rect(20, 20, Screen.width - 40, Screen.height - 40));
		GUI.backgroundColor = Color.clear;
		GUILayout.TextArea(m_Message);
		GUI.backgroundColor = Color.white;
		GUILayout.EndArea();

	}


	///////////////////////////////////////////////////////////
	// draws button for an 'OK' type dialog
	///////////////////////////////////////////////////////////
	private void DoOK()
	{
		DoSpace();
		if (GUILayout.Button("OK")) { m_Callback(Answer.OK); this.Close(); }
		DoSpace();
	}


	///////////////////////////////////////////////////////////
	// draws buttons for an 'OK / Cancel' type dialog
	///////////////////////////////////////////////////////////
	private void DoOKCancel()
	{
		DoSpace();
		if (GUILayout.Button("OK")) { m_Callback(Answer.OK); this.Close(); }
		if (GUILayout.Button("Cancel")) { m_Callback(Answer.Cancel); this.Close(); }
	}


	///////////////////////////////////////////////////////////
	// draws buttons for an 'Abort / Retry / Ignore' type dialog
	///////////////////////////////////////////////////////////
	private void DoAbortRetryIgnore()
	{
		if (GUILayout.Button("Abort")) { m_Callback(Answer.Abort); this.Close(); }
		if (GUILayout.Button("Retry")) { m_Callback(Answer.Retry); this.Close(); }
		if (GUILayout.Button("Ignore")) { m_Callback(Answer.Ignore); this.Close(); }
	}


	///////////////////////////////////////////////////////////
	// draws buttons for a 'Yes / No / Cancel' type dialog
	///////////////////////////////////////////////////////////
	private void DoYesNoCancel()
	{
		if (GUILayout.Button("Yes")) { m_Callback(Answer.Yes); this.Close(); }
		if (GUILayout.Button("No")) { m_Callback(Answer.No); this.Close(); }
		if (GUILayout.Button("Cancel")) { m_Callback(Answer.Cancel); this.Close(); }
	}


	///////////////////////////////////////////////////////////
	// draws buttons for a 'Yes / No' type dialog
	///////////////////////////////////////////////////////////
	private void DoYesNo()
	{
		DoSpace();
		if (GUILayout.Button("Yes")) { m_Callback(Answer.Yes); this.Close(); }
		if (GUILayout.Button("No")) { m_Callback(Answer.No); this.Close(); }
	}


	///////////////////////////////////////////////////////////
	// draws buttons for a 'Retry / Cancel' type dialog
	///////////////////////////////////////////////////////////
	private void DoRetryCancel()
	{
		DoSpace();
		if (GUILayout.Button("Retry")) { m_Callback(Answer.Retry); this.Close(); }
		if (GUILayout.Button("Cancel")) { m_Callback(Answer.Cancel); this.Close(); }
	}


	///////////////////////////////////////////////////////////
	// draws a space on the dialog
	///////////////////////////////////////////////////////////
	private void DoSpace()
	{
		GUILayout.Space((m_DialogSize.x - 40) / 3);
	}


}

