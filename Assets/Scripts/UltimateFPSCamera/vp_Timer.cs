/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Timer.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	the vp_Timer class expands the functionality of Invoke, by adding
//					support for delegates, arguments, canceling individual calls
//					to methods, and the ability to call methods in other classes.
//					upon first use, 'vp_Timer.At' creates a GameObject to which all
//					subsequent vp_Timers are added in the form of components.
//
/////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using UnityEngine;

public class vp_Timer : MonoBehaviour
{

	private static GameObject m_GameObject = new GameObject("Timers");

	// callback for methods with no parameters
	public delegate void Callback();
	private Callback m_Function = null;

	// callback for methods that have parameters
	public delegate void ArgCallback(object args);
	private ArgCallback m_ArgFunction = null;
	
	// the arguments passed by the user
	private object m_Arguments = null;

	// if a timer is repeating, it will put copies of itself
	// in this list, so they can be canceled all at once
	private List<vp_Timer> m_Iterations = new List<vp_Timer>();


	///////////////////////////////////////////////////////////
	// 
	///////////////////////////////////////////////////////////
	void Awake()
	{

		// by default the "Timers" gameobject is invisible in the
		// hierarchy. disabling this may be useful for debugging
		m_GameObject.hideFlags = HideFlags.HideInHierarchy;

	}


	///////////////////////////////////////////////////////////
	// 'At' is the main tool of vp_Timer. it is used to create
	// timers, schedule them and hook them to delegates.
	// you can schedule methods with or without arguments,
	// and with optional repeat counts and repeat intervals.
	///////////////////////////////////////////////////////////

	// schedule a method with no arguments
	public static vp_Timer At(float time, Callback function, int iterations = 1, float interval = 0.0f)
	{
		return At(time, function, null, null, iterations, interval);
	}

	// schedule a method with arguments
	public static vp_Timer At(float time, ArgCallback function, object args, int iterations = 1, float interval = 0.0f)
	{
		return At(time, null, function, args, iterations, interval);
	}

	// internal schedule method
	private static vp_Timer At(float time, Callback func, ArgCallback argFunc, object args, int iterations, float interval)
	{

		vp_Timer firstTimer = null;

		float currentTime = time;

		interval = (interval == 0.0f) ? time : interval;

		for (int i = 0; i < iterations; i++)
		{
			vp_Timer timer = m_GameObject.AddComponent<vp_Timer>();
			if (i == 0)
				firstTimer = timer;
			else
				firstTimer.m_Iterations.Add(timer);

			if(func != null)
				timer.Schedule(currentTime, func);
			else if (argFunc != null)
				timer.Schedule(currentTime, argFunc, args);

			currentTime += interval;
		}

		return firstTimer;

	}


	///////////////////////////////////////////////////////////
	// static version of the Cancel method. NOTE: calling
	// 'CancelInvoke' on a vp_Timer is the same as calling
	// 'Cancel'.
	///////////////////////////////////////////////////////////
	public static void Cancel(vp_Timer timer)
	{
		if (timer == null)
			return;
		timer.Cancel();
	}


	///////////////////////////////////////////////////////////
	// object version of the Cancel method
	///////////////////////////////////////////////////////////
	public void Cancel()
	{

		if (this.m_Iterations.Count > 0)
		{
			for (int t = m_Iterations.Count - 1; t >= 0; t--)
			{
				m_Iterations[t].CancelInstance();
				m_Iterations.Remove(m_Iterations[t]);
			}
		}
		this.CancelInstance();

	}


	///////////////////////////////////////////////////////////
	// these overrides re-route the 'CancelInvoke' method inherited
	// from 'MonoBehaviour'. so that calling 'CancelInvoke' on a
	// vp_Timer is the same as calling 'Cancel'
	///////////////////////////////////////////////////////////
	new public void CancelInvoke(string methodName)
	{
		Cancel();
	}

	new public void CancelInvoke()
	{
		Cancel();
	}


	///////////////////////////////////////////////////////////
	// internal disabling of the vp_Timer
	///////////////////////////////////////////////////////////
	private void CancelInstance()
	{
		if (this != null)
		{
			m_Function = null;
			m_ArgFunction = null;
			m_Arguments = null;
			enabled = false;
			hideFlags = HideFlags.HideInInspector;
		}
	}


	///////////////////////////////////////////////////////////
	// 'Schedule' performs the Invoke internally
	///////////////////////////////////////////////////////////

	// schedules a method with no arguments
	private void Schedule(float time, Callback function)
	{
		m_Function = function;
		Invoke("Execute", time);
	}

	// schedules a method with arguments
	private void Schedule(float time, ArgCallback function, object args)
	{
		m_ArgFunction = function;
		m_Arguments = args;
		Invoke("ArgExecute", time);
	}


	///////////////////////////////////////////////////////////
	// 'Execute' is called internally when the Invoke fires. it
	// calls the user defined delegate then destroys the timer.
	///////////////////////////////////////////////////////////
	private void Execute()
	{

		if (m_Function != null)
			m_Function();

		Destroy(this);

	}


	///////////////////////////////////////////////////////////
	// calls a user defined delegate with arguments, then
	// destroys the timer.
	///////////////////////////////////////////////////////////
	private void ArgExecute()
	{

		if (m_ArgFunction != null)
			m_ArgFunction(m_Arguments);

		Destroy(this);

	}


}
