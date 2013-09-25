using UnityEngine;
using System.Collections;

public class LookAt : MonoBehaviour 
{
	public Transform Target;
	public float Speed = 1.0f;

	void Update() 
	{
		if( Target != null )
		{
			Quaternion to = Quaternion.LookRotation( Target.position - transform.position );
			transform.rotation = Quaternion.Slerp( transform.rotation, to, Speed * Time.deltaTime );
		}
	}
}
